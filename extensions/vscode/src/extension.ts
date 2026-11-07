import * as vscode from "vscode";

import { ensureCli, runCliJson, type ListResult, type LookupInput, type LookupResult } from "./cli";
import { registerReleaseCommands } from "./releaseCommands";

const releaseStatusToolName = "mycelium-hypha_get_release_status";

/**
 * Registers the Copilot tools and release-management commands.
 * @param context VS Code extension context for bundled assets and shared storage.
 */
export function activate(context: vscode.ExtensionContext): void {
  const output = vscode.window.createOutputChannel("Hypha");

  context.subscriptions.push(
    output,
    vscode.lm.registerTool(releaseStatusToolName, new ReleaseStatusTool(
      context.extensionUri.fsPath,
      context.globalStorageUri.fsPath,
      output
    )),
    vscode.lm.registerTool<LookupInput>("mycelium-hypha_lookup_knowledge", {
      async invoke(options, token) {
        if (token.isCancellationRequested) {
          throw new vscode.CancellationError();
        }
        try {
          const executable = await ensureCli(context.extensionUri.fsPath);
          const args = ["lookup", "--kind", options.input.kind, "--name", options.input.name];
          if (options.input.tag) {
            args.push("--tag", options.input.tag);
          }
          if (options.input.startLine !== undefined) {
            args.push("--start-line", String(options.input.startLine));
          }
          args.push("--json");

          const result = await runCliJson<LookupResult>(
            executable,
            context.globalStorageUri.fsPath,
            args,
            output,
            token
          );
          // Convert the CLI's filesystem path to a URI that Copilot can cite as a source.
          const { path: sourcePath, ...page } = result;
          const response = {
            ...page,
            source: sourcePath ? vscode.Uri.file(sourcePath).toString() : null
          };
          return new vscode.LanguageModelToolResult([
            new vscode.LanguageModelTextPart(JSON.stringify(response))
          ]);
        } catch (error) {
          if (isCancellationError(error)) {
            throw error;
          }
          const detail = error instanceof Error ? error.message : String(error);
          output.appendLine(`Knowledge lookup failed: ${detail}`);
          throw new Error(`Hypha lookup failed: ${detail}. Check installed releases; do not silently substitute another release or general knowledge.`, { cause: error });
        }
      }
    }),
    ...registerReleaseCommands(context, output),
    vscode.commands.registerCommand("mycelium-hypha.showReleaseStatus", async () => {
      try {
        const status = await vscode.window.withProgress(
          {
            location: vscode.ProgressLocation.Notification,
            title: "Reading Hypha release status",
            cancellable: true
          },
          async (_progress, token) => {
            const executable = await ensureCli(context.extensionUri.fsPath);
            return readReleaseStatus(executable, context.globalStorageUri.fsPath, output, token);
          }
        );
        output.appendLine(status);
        output.appendLine(`Data location: ${context.globalStorageUri.toString(true)}`);
        output.show(true);
      } catch (error) {
        if (isCancellationError(error)) {
          return;
        }
        const message = error instanceof Error ? error.message : String(error);
        output.appendLine(`Error: ${message}`);
        output.show(true);
        await vscode.window.showErrorMessage(`Hypha could not read its release data: ${message}`);
      }
    })
  );
}

/** No additional shutdown work is needed; VS Code disposes the registered subscriptions. */
export function deactivate(): void {}

/** Adapts the CLI's local release listing to Copilot's read-only status tool. */
class ReleaseStatusTool implements vscode.LanguageModelTool<Record<string, never>> {
  /**
   * @param extensionPath Root folder of the installed extension containing the bundled CLI.
   * @param storagePath Extension-global root containing installed release data.
   * @param output Output channel for CLI diagnostics.
   */
  constructor(
    private readonly extensionPath: string,
    private readonly storagePath: string,
    private readonly output: vscode.OutputChannel
  ) {}

  /** Runs the CLI's read-only release listing and returns its formatted status. */
  async invoke(
    _options: vscode.LanguageModelToolInvocationOptions<Record<string, never>>,
    token: vscode.CancellationToken
  ): Promise<vscode.LanguageModelToolResult> {
    const executable = await ensureCli(this.extensionPath);
    const status = await readReleaseStatus(executable, this.storagePath, this.output, token);
    return new vscode.LanguageModelToolResult([new vscode.LanguageModelTextPart(status)]);
  }
}

/** Formats `hypha list --json` without inspecting the generated files in TypeScript. */
async function readReleaseStatus(
  executable: string,
  storagePath: string,
  output: vscode.OutputChannel,
  token: vscode.CancellationToken
): Promise<string> {
  const result = await runCliJson<ListResult>(executable, storagePath, ["list", "--json"], output, token);
  if (result.releases.length === 0) {
    return "No Hypha releases are recorded as installed in this VS Code extension host.";
  }

  const installed = result.releases.map((release) => {
    const artifacts = release.artifacts;
    const specification = artifacts.specification ? "present" : "not present";
    return `${release.tag}${release.isDefault ? " (default)" : ""}: core ${artifacts.core ? "ready" : "incomplete"} ` +
      `(metamodel ${yesNo(artifacts.metamodel)}, textual notation ${yesNo(artifacts.textualNotation)}, ` +
      `model library ${yesNo(artifacts.modelLibrary)}, cross-references ${yesNo(artifacts.crossReferences)}); ` +
      `specification text ${specification}`;
  });
  const defaultRelease = result.defaultTag
    ? `The configured default is ${result.defaultTag}.`
    : "No default release is selected.";
  return `Hypha records these installed releases: ${installed.join(" ")} ${defaultRelease}`;
}

/** Converts an artifact-presence flag into concise status text. */
function yesNo(value: boolean): string {
  return value ? "present" : "missing";
}

/** Identifies VS Code cancellation so callers can avoid presenting it as a failure. */
function isCancellationError(error: unknown): boolean {
  return error instanceof vscode.CancellationError;
}
