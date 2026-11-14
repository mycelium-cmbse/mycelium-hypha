import * as vscode from "vscode";

import { cliPath, ensureCli } from "./cli";

/** The provider id, which `contributes.mcpServerDefinitionProviders` in package.json declares too. */
export const providerId = "mycelium-hypha.server";

/** The server's name, the same as in every other client's manifest. */
export const serverLabel = "hypha";

/**
 * Hands the hypha MCP server to the editor: the bundled `hypha`, started as `hypha serve`.
 *
 * The extension has no tools or commands of its own. Everything Copilot can ask, and installing,
 * switching and removing releases, goes through the server's tools, as in every other client.
 * @param context VS Code extension context, for the bundled executable and the extension's version.
 */
export function activate(context: vscode.ExtensionContext): void {
  const extensionPath = context.extensionUri.fsPath;
  const version = String(context.extension.packageJSON.version ?? "");

  context.subscriptions.push(
    vscode.lm.registerMcpServerDefinitionProvider(providerId, {
      // Discovery stays cheap and silent: it names the executable without touching the disk.
      provideMcpServerDefinitions: () => [serverDefinition(extensionPath, version)],
      // Checked only when the editor is about to start the server, so a broken install says why.
      resolveMcpServerDefinition: async (server) => {
        await ensureCli(extensionPath);
        return server;
      }
    })
  );
}

/** No additional shutdown work is needed; VS Code disposes the registered subscription. */
export function deactivate(): void {}

/**
 * The server as the editor starts it.
 *
 * No `--repository-root` and no environment of its own: `hypha` reads `HYPHA_HOME`, the per-user
 * folder every client shares, so a release installed in one client answers in this one.
 * @param extensionPath Root folder of the installed extension.
 * @param version The extension's version; a change makes the editor refresh the server's tools.
 * @returns The stdio server definition.
 */
export function serverDefinition(extensionPath: string, version: string): vscode.McpStdioServerDefinition {
  return new vscode.McpStdioServerDefinition(serverLabel, cliPath(extensionPath), ["serve"], {}, version);
}
