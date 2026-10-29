import * as vscode from "vscode";

import { releaseReadiness } from "./readiness";
import { registerReleaseCommands } from "./releaseCommands";
import { parseReleaseManifest, type ReleaseManifest } from "./manifest";
import { lookupKnowledge, type LookupInput } from "./knowledge";

const releaseStatusToolName = "mycelium-hypha_get_release_status";

export function activate(context: vscode.ExtensionContext): void {
  const output = vscode.window.createOutputChannel("Hypha");

  context.subscriptions.push(
    output,
    vscode.lm.registerTool(releaseStatusToolName, new ReleaseStatusTool(context.globalStorageUri)),
    vscode.lm.registerTool<LookupInput>("mycelium-hypha_lookup_knowledge", {
      async invoke(options, token) {
        if (token.isCancellationRequested) {
          throw new vscode.CancellationError();
        }
        try {
          const result = await lookupKnowledge(context.globalStorageUri.fsPath, options.input);
          return new vscode.LanguageModelToolResult([new vscode.LanguageModelTextPart(result)]);
        } catch (error) {
          const detail = error instanceof Error ? error.message : String(error);
          output.appendLine(`Knowledge lookup failed: ${detail}`);
          throw new Error(`Hypha lookup failed: ${detail}. Check installed releases; do not silently substitute another release or general knowledge.`, { cause: error });
        }
      }
    }),
    ...registerReleaseCommands(context, output),
    vscode.commands.registerCommand("mycelium-hypha.showReleaseStatus", async () => {
      try {
        const status = await readReleaseStatus(context.globalStorageUri);
        output.appendLine(status);
        output.appendLine(`Data location: ${context.globalStorageUri.toString(true)}`);
        output.show(true);
      } catch (error) {
        const message = error instanceof Error ? error.message : String(error);
        output.appendLine(`Error: ${message}`);
        output.show(true);
        await vscode.window.showErrorMessage(`Hypha could not read its release data: ${message}`);
      }
    })
  );
}

export function deactivate(): void {}

class ReleaseStatusTool implements vscode.LanguageModelTool<Record<string, never>> {
  constructor(private readonly globalStorageUri: vscode.Uri) {}

  async invoke(
    _options: vscode.LanguageModelToolInvocationOptions<Record<string, never>>,
    _token: vscode.CancellationToken
  ): Promise<vscode.LanguageModelToolResult> {
    const status = await readReleaseStatus(this.globalStorageUri);
    return new vscode.LanguageModelToolResult([new vscode.LanguageModelTextPart(status)]);
  }
}

async function readReleaseStatus(globalStorageUri: vscode.Uri): Promise<string> {
  const manifestUri = vscode.Uri.joinPath(globalStorageUri, "knowledge", "versions.json");
  let contents: Uint8Array;

  try {
    contents = await vscode.workspace.fs.readFile(manifestUri);
  } catch (error) {
    if (error instanceof vscode.FileSystemError && error.code === "FileNotFound") {
      return "No Hypha releases are recorded as installed in this VS Code extension host.";
    }

    const detail = error instanceof Error ? error.message : String(error);
    throw new Error(`Could not read the Hypha release manifest: ${detail}`, { cause: error });
  }

  let manifest: ReleaseManifest;
  try {
    manifest = parseReleaseManifest(new TextDecoder().decode(contents));
  } catch (error) {
    const detail = error instanceof Error ? error.message : String(error);
    throw new Error(`The Hypha release manifest is invalid: ${detail}`, { cause: error });
  }

  if (manifest.defaultTag === null) {
    return manifest.tags.length === 0
      ? "No Hypha releases are recorded as installed in this VS Code extension host."
      : `Hypha records these installed releases: ${manifest.tags.join(", ")}. No default release is selected.`;
  }

  return `Hypha records these installed releases: ${manifest.tags.join(", ")}. The configured default is ${manifest.defaultTag}. ${await releaseReadiness(globalStorageUri, manifest.defaultTag)}`;
}
