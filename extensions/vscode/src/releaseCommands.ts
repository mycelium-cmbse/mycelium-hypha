import * as vscode from "vscode";

import { ensureCli, runCli } from "./cli";
import { parseReleaseManifest, type ReleaseManifest } from "./manifest";
import { releaseReadiness } from "./readiness";
import { availableReleases, type ReleaseCheck } from "./releaseCheck";
import { provisionRuntimeAssets } from "./runtimeAssets";

export function registerReleaseCommands(
  context: vscode.ExtensionContext,
  output: vscode.OutputChannel
): vscode.Disposable[] {
  const storagePath = context.globalStorageUri.fsPath;

  return [
    vscode.commands.registerCommand("mycelium-hypha.checkReleases", async () => {
      try {
        const check = await checkReleases(context.globalStorageUri, output);
        const installed = check.installedTags.length > 0
          ? `Installed: ${check.installedTags.join(", ")}${check.defaultTag ? ` (default ${check.defaultTag})` : ""}.`
          : "No releases are installed.";
        const available = check.availableOnline.length > 0
          ? `Available upstream: ${check.availableOnline.join(", ")}.`
          : "No releases are currently available upstream.";
        output.appendLine(`${installed} ${available}`);
        output.show(true);
        await vscode.window.showInformationMessage(`${installed} ${available}`);
      } catch (error) {
        await showError(output, "Could not check Hypha releases", error);
      }
    }),
    vscode.commands.registerCommand("mycelium-hypha.installRelease", async () => {
      try {
        const check = await checkReleases(context.globalStorageUri, output);
        if (check.availableOnline.length === 0) {
          await vscode.window.showInformationMessage("Hypha did not find any installable releases upstream.");
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
          `Fetch release ${selection.label}, make it the default, and generate its Hypha knowledge in this VS Code extension's global storage? This uses the bundled CLI, fetches upstream inputs including OMG specification PDFs, and may download uv, Python and extraction dependencies. These files remain local on this extension host; nothing is added to your modeling workspace.`,
          { modal: true },
          "Fetch and Generate"
        );
        if (answer !== "Fetch and Generate") {
          return;
        }

        output.show(true);
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
            await runCli(
              executable,
              storagePath,
              ["fetch", "--tag", selection.label, "--log-level", "Information"],
              output,
              token
            );
            progress.report({ message: "Generating local knowledge", increment: 50 });
            await runCli(
              executable,
              storagePath,
              ["generate", "--tag", selection.label, "--log-level", "Information"],
              output,
              token
            );
            progress.report({ message: "Checking generated capabilities", increment: 40 });
          }
        );

        const result = `Release ${selection.label} installation finished. ${await releaseReadiness(context.globalStorageUri, selection.label)}`;
        output.appendLine(result);
        output.show(true);
        await vscode.window.showInformationMessage(result);
      } catch (error) {
        await showError(output, "Hypha release installation did not complete", error);
      }
    })
  ];
}

async function checkReleases(globalStorageUri: vscode.Uri, output: vscode.OutputChannel): Promise<ReleaseCheck> {
  return vscode.window.withProgress(
    {
      location: vscode.ProgressLocation.Notification,
      title: "Checking Hypha releases",
      cancellable: true
    },
    async (progress, token) => {
      const controller = new AbortController();
      const cancellation = token.onCancellationRequested(() => controller.abort());
      try {
        progress.report({ message: "Reading installed releases and checking upstream tags" });
        const [releaseTags, pilotTags, manifest] = await Promise.all([
          fetchTags("Systems-Modeling/SysML-v2-Release", controller.signal),
          fetchTags("Systems-Modeling/SysML-v2-Pilot-Implementation", controller.signal),
          readInstalled(globalStorageUri)
        ]);
        return {
          ...manifest,
          availableOnline: availableReleases(releaseTags, pilotTags).slice(0, 5)
        };
      } finally {
        cancellation.dispose();
        controller.abort();
      }
    }
  );
}

async function fetchTags(repository: string, signal: AbortSignal): Promise<string[]> {
  const tags: string[] = [];
  const token = process.env.GITHUB_TOKEN?.trim() || process.env.GH_TOKEN?.trim();

  for (let page = 1; page <= 20; page++) {
    const headers: Record<string, string> = {
      Accept: "application/vnd.github+json",
      "User-Agent": "mycelium-hypha-vscode"
    };
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(
      `https://api.github.com/repos/${repository}/tags?per_page=100&page=${page}`,
      { headers, signal }
    );
    if (!response.ok) {
      throw new Error(
        `Could not read release tags from ${repository} (${response.status} ${response.statusText}).`
      );
    }

    const payload: unknown = await response.json();
    if (!Array.isArray(payload)) {
      throw new Error(`GitHub returned an invalid tag list for ${repository}.`);
    }
    if (payload.length === 0) {
      return tags;
    }

    for (const entry of payload) {
      if (!isRecord(entry) || typeof entry.name !== "string") {
        throw new Error(`GitHub returned an invalid tag entry for ${repository}.`);
      }
      tags.push(entry.name);
    }
  }

  throw new Error(`GitHub returned more than 2,000 tags for ${repository}; release discovery stopped safely.`);
}

async function readInstalled(globalStorageUri: vscode.Uri): Promise<Pick<ReleaseCheck, "installedTags" | "defaultTag">> {
  try {
    const contents = await vscode.workspace.fs.readFile(
      vscode.Uri.joinPath(globalStorageUri, "knowledge", "versions.json")
    );
    const manifest: ReleaseManifest = parseReleaseManifest(new TextDecoder().decode(contents));
    return { installedTags: manifest.tags, defaultTag: manifest.defaultTag };
  } catch (error) {
    if (error instanceof vscode.FileSystemError && error.code === "FileNotFound") {
      return { installedTags: [], defaultTag: null };
    }
    throw error;
  }
}

async function showError(
  output: vscode.OutputChannel,
  title: string,
  error: unknown
): Promise<void> {
  const detail = error instanceof Error ? error.message : String(error);
  output.appendLine(`${title}: ${detail}`);
  output.show(true);
  await vscode.window.showErrorMessage(`${title}: ${detail}`);
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}
