import * as vscode from "vscode";

import { ensureCli, runCli, runCliJson, type CheckResult, type ListResult, type ReleaseStatus } from "./cli";
import { provisionRuntimeAssets } from "./runtimeAssets";

/**
 * Registers release discovery and mutation commands backed by the bundled CLI.
 * @param context VS Code extension context for the bundled CLI and shared storage.
 * @param output Output channel for command progress and diagnostics.
 * @returns Disposable registrations for the extension context.
 */
export function registerReleaseCommands(
  context: vscode.ExtensionContext,
  output: vscode.OutputChannel
): vscode.Disposable[] {
  const storagePath = context.globalStorageUri.fsPath;
  // Share one guard across install, use and remove because all mutate the same manifest.
  const releaseOperation = { inProgress: false };

  return [
    vscode.commands.registerCommand("mycelium-hypha.checkReleases", async () => {
      try {
        const check = await checkReleases(context, output);
        const message = formatCheck(check);
        output.appendLine(message);
        output.show(true);
        void vscode.window.showInformationMessage(message);
      } catch (error) {
        await showError(output, "Could not check Hypha releases", error);
      }
    }),
    registerReleaseMutation(
      "mycelium-hypha.installRelease",
      "Hypha release installation did not complete",
      output,
      releaseOperation,
      async () => {
        const check = await checkReleases(context, output);
        if (check.availableOnline.length === 0) {
          void vscode.window.showInformationMessage("Hypha did not find any installable releases upstream.");
          return;
        }

        const selection = await vscode.window.showQuickPick(
          check.availableOnline.map((tag) => ({
            label: tag,
            description: check.installedTags.includes(tag) ? "Already installed" : "Available upstream"
          })),
          { title: "Choose a SysML v2 / KerML release to install", placeHolder: "Newest releases are listed first" }
        );
        if (!selection) {
          return;
        }

        const answer = await vscode.window.showWarningMessage(
          `Fetch release ${selection.label} and generate its Hypha knowledge in this VS Code extension's global storage? It will become the default only after generation succeeds. This uses the bundled CLI, fetches upstream inputs including OMG specification PDFs, and may download uv, Python and extraction dependencies. These files remain local on this extension host; nothing is added to your modeling workspace.`,
          { modal: true },
          "Fetch and Generate"
        );
        if (answer !== "Fetch and Generate") {
          return;
        }

        output.appendLine(`Installing ${selection.label} in ${storagePath}.`);
        await vscode.window.withProgress(
          {
            location: vscode.ProgressLocation.Notification,
            title: `Installing Hypha release ${selection.label}`,
            cancellable: true
          },
          async (progress, token) => {
            progress.report({ message: "Preparing bundled CLI and generation inputs" });
            const executable = await ensureCli(context.extensionUri.fsPath);
            await provisionRuntimeAssets(context.extensionUri.fsPath, storagePath);
            progress.report({ message: "Fetching release inputs", increment: 10 });
            // Keep the existing default usable until fetch and generation both succeed.
            await runCli(
              executable,
              storagePath,
              ["fetch", "--tag", selection.label, "--no-default", "--log-level", "Information"],
              output,
              token
            );
            progress.report({ message: "Generating local knowledge", increment: 45 });
            await runCli(
              executable,
              storagePath,
              ["generate", "--tag", selection.label, "--log-level", "Information"],
              output,
              token
            );
            // Only publish the new default after generated knowledge is ready.
            progress.report({ message: "Setting the default release", increment: 5 });
            await runCli(
              executable,
              storagePath,
              ["use", "--tag", selection.label, "--log-level", "Information"],
              output,
              token
            );
            progress.report({ message: "Checking generated capabilities", increment: 40 });
          }
        );

        const executable = await ensureCli(context.extensionUri.fsPath);
        const releases = await readReleases(executable, storagePath, output);
        const installed = releases.releases.find((release) => release.tag === selection.label);
        const result = `Release ${selection.label} installation finished. ${installed ? formatReadiness(installed) : "Release status is unavailable."}`;
        output.appendLine(result);
        output.show(true);
        void vscode.window.showInformationMessage(result);
      }
    ),
    registerReleaseMutation(
      "mycelium-hypha.useRelease",
      "Could not change the default Hypha release",
      output,
      releaseOperation,
      async () => {
        const executable = await ensureCli(context.extensionUri.fsPath);
        const installed = await readReleases(executable, storagePath, output);
        if (installed.releases.length === 0) {
          void vscode.window.showInformationMessage("No Hypha releases are installed.");
          return;
        }

        const selection = await vscode.window.showQuickPick(
          installed.releases.map((release) => ({
            label: release.tag,
            description: release.isDefault ? "Current default" : "Installed"
          })),
          { title: "Choose the default Hypha release" }
        );
        if (!selection) {
          return;
        }

        await runReleaseCommand(
          "Changing the default Hypha release",
          executable,
          storagePath,
          ["use", "--tag", selection.label],
          output
        );
        const message = `Hypha now uses ${selection.label} as its default release.`;
        output.appendLine(message);
        output.show(true);
        void vscode.window.showInformationMessage(message);
      }
    ),
    registerReleaseMutation(
      "mycelium-hypha.removeRelease",
      "Could not remove the Hypha release",
      output,
      releaseOperation,
      async () => {
        const executable = await ensureCli(context.extensionUri.fsPath);
        const installed = await readReleases(executable, storagePath, output);
        if (installed.releases.length === 0) {
          void vscode.window.showInformationMessage("No Hypha releases are installed.");
          return;
        }

        const selection = await vscode.window.showQuickPick(
          installed.releases.map((release) => ({
            label: release.tag,
            description: release.isDefault ? "Current default; switch first to remove" : "Installed"
          })),
          { title: "Choose a Hypha release to remove" }
        );
        if (!selection) {
          return;
        }

        const selected = installed.releases.find((release) => release.tag === selection.label)!;
        if (selected.isDefault && installed.releases.length > 1) {
          void vscode.window.showInformationMessage("Switch to another default release before removing this one.");
          return;
        }

        const force = installed.releases.length === 1;
        const answer = await vscode.window.showWarningMessage(
          force
            ? `Remove the final installed release ${selection.label} and clear the default? Its fetched inputs and generated knowledge will be deleted from this extension host.`
            : `Remove release ${selection.label} and its fetched inputs and generated knowledge from this extension host?`,
          { modal: true },
          "Remove Release"
        );
        if (answer !== "Remove Release") {
          return;
        }

        await runReleaseCommand(
          `Removing Hypha release ${selection.label}`,
          executable,
          storagePath,
          ["remove", "--tag", selection.label, ...(force ? ["--force"] : [])],
          output
        );
        const message = `Hypha release ${selection.label} was removed.`;
        output.appendLine(message);
        output.show(true);
        void vscode.window.showInformationMessage(message);
      }
    )
  ];
}

/** Checks installed and upstream releases through the CLI's machine-readable contract. */
async function checkReleases(
  context: vscode.ExtensionContext,
  output: vscode.OutputChannel
): Promise<CheckResult> {
  return vscode.window.withProgress(
    {
      location: vscode.ProgressLocation.Notification,
      title: "Checking Hypha releases",
      cancellable: true
    },
    async (progress, token) => {
      progress.report({ message: "Reading installed releases and checking upstream tags" });
      const executable = await ensureCli(context.extensionUri.fsPath);
      return runCliJson<CheckResult>(
        executable,
        context.globalStorageUri.fsPath,
        ["check", "--json"],
        output,
        token
      );
    }
  );
}

/** Reads installed release readiness without duplicating manifest or path handling. */
async function readReleases(
  executable: string,
  storagePath: string,
  output: vscode.OutputChannel
): Promise<ListResult> {
  // Dispose the token source even if the CLI fails to start or returns invalid JSON.
  const source = new vscode.CancellationTokenSource();
  try {
    return await runCliJson<ListResult>(executable, storagePath, ["list", "--json"], output, source.token);
  } finally {
    source.dispose();
  }
}

/** Runs a mutating release command with cancellable VS Code progress. */
async function runReleaseCommand(
  title: string,
  executable: string,
  storagePath: string,
  args: string[],
  output: vscode.OutputChannel
): Promise<void> {
  await vscode.window.withProgress(
    { location: vscode.ProgressLocation.Notification, title, cancellable: true },
    async (_progress, token) => runCli(executable, storagePath, args, output, token)
  );
}

/** Builds the user-facing summary from the CLI's installed/upstream release result. */
function formatCheck(check: CheckResult): string {
  const installed = check.installedTags.length > 0
    ? `Installed: ${check.installedTags.join(", ")}${check.defaultTag ? ` (default ${check.defaultTag})` : ""}.`
    : "No releases are installed.";
  const available = check.availableOnline.length > 0
    ? `Available upstream: ${check.availableOnline.join(", ")}.`
    : "No releases are currently available upstream.";
  return `${installed} ${available}`;
}

/** Summarizes CLI-reported artifact presence without probing the storage layout. */
function formatReadiness(release: ReleaseStatus): string {
  const artifacts = release.artifacts;
  return `Core readiness: ${artifacts.core ? "ready" : "incomplete"} ` +
    `(metamodel ${yesNo(artifacts.metamodel)}, textual notation ${yesNo(artifacts.textualNotation)}, ` +
    `model library ${yesNo(artifacts.modelLibrary)}, cross-references ${yesNo(artifacts.crossReferences)}). ` +
    `Specification text ${artifacts.specification ? "is present" : "is not present"}.`;
}

/** Maps an artifact-presence flag to the wording used in release summaries. */
function yesNo(value: boolean): string {
  return value ? "present" : "missing";
}

/**
 * Wraps a release mutation with a shared in-progress guard and consistent error handling.
 *
 * The guard is shared by all registered mutations so overlapping CLI processes cannot
 * race while changing the same release manifest.
 */
function registerReleaseMutation(
  command: string,
  errorTitle: string,
  output: vscode.OutputChannel,
  operationState: { inProgress: boolean },
  operation: () => Promise<void>
): vscode.Disposable {
  return vscode.commands.registerCommand(command, async () => {
    if (operationState.inProgress) {
      void vscode.window.showInformationMessage("A Hypha release operation is already in progress.");
      return;
    }

    operationState.inProgress = true;
    try {
      await operation();
    } catch (error) {
      await showError(output, errorTitle, error);
    } finally {
      operationState.inProgress = false;
    }
  });
}

/** Logs command failures while treating user cancellation as a normal outcome. */
async function showError(
  output: vscode.OutputChannel,
  title: string,
  error: unknown
): Promise<void> {
  if (isCancellationError(error)) {
    output.appendLine(`${title}: cancelled.`);
    return;
  }
  const detail = error instanceof Error ? error.message : String(error);
  output.appendLine(`${title}: ${detail}`);
  output.show(true);
  // Non-modal notifications resolve on dismissal, not when the operation completes.
  void vscode.window.showErrorMessage(`${title}: ${detail}`);
}

/** Checks for cancellation before deciding whether to display an error notification. */
function isCancellationError(error: unknown): boolean {
  return error instanceof vscode.CancellationError;
}
