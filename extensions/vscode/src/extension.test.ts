// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import path from "node:path";
import test from "node:test";
import vm from "node:vm";

interface ServerDefinition {
  label: string;
  command: string;
  args: string[];
  env: Record<string, unknown>;
  version?: string;
  cwd?: { fsPath: string };
}

interface Provider {
  provideMcpServerDefinitions(): ServerDefinition[];
  resolveMcpServerDefinition(server: ServerDefinition): Promise<ServerDefinition>;
}

// Loads the compiled extension with a stand-in for the editor's API, and activates it.
function activateExtension(cliPresent: boolean, packageJSON: Record<string, unknown> = { version: "9.8.7" }) {
  const providers = new Map<string, Provider>();
  const registrations: string[] = [];
  const checked: string[] = [];
  const vscode = {
    lm: {
      registerMcpServerDefinitionProvider(id: string, provider: Provider) {
        providers.set(id, provider);
        return { dispose() {} };
      },
      registerTool(name: string) {
        registrations.push(`tool ${name}`);
        return { dispose() {} };
      }
    },
    commands: {
      registerCommand(name: string) {
        registrations.push(`command ${name}`);
        return { dispose() {} };
      }
    },
    Uri: {
      file(fsPath: string) { return { fsPath }; }
    },
    McpStdioServerDefinition: class {
      cwd?: { fsPath: string };

      constructor(
        public label: string,
        public command: string,
        public args: string[],
        public env: Record<string, unknown>,
        public version?: string
      ) {}
    }
  };
  const cli = {
    cliPath(extensionPath: string) { return `${extensionPath}/out/cli/hypha`; },
    async ensureCli(extensionPath: string) {
      checked.push(extensionPath);
      if (!cliPresent) {
        throw new Error("The bundled Hypha CLI is missing or not executable.");
      }
      return `${extensionPath}/out/cli/hypha`;
    }
  };
  const exports: Record<string, any> = {};
  vm.runInNewContext(readFileSync(path.join(__dirname, "extension.js"), "utf8"), {
    exports,
    require(name: string) {
      if (name === "vscode") { return vscode; }
      if (name === "./cli") { return cli; }
      throw new Error(`Unexpected import: ${name}`);
    }
  });
  const subscriptions: unknown[] = [];
  exports.activate({
    subscriptions,
    extensionUri: { fsPath: "/extension" },
    extension: { packageJSON }
  });

  return { providers, registrations, subscriptions, checked, providerId: exports.providerId as string };
}

const manifest = JSON.parse(readFileSync(path.join(__dirname, "..", "package.json"), "utf8"));

test("the extension registers the provider its manifest declares, and nothing else", () => {
  const extension = activateExtension(true);
  const declared = manifest.contributes.mcpServerDefinitionProviders.map((provider: { id: string }) => provider.id);

  assert.deepEqual([...extension.providers.keys()], [extension.providerId]);
  assert.deepEqual(declared, [extension.providerId], "the editor refuses a provider whose id the manifest does not declare");
  assert.deepEqual(extension.registrations, [], "no tools and no commands of its own");
  assert.equal(extension.subscriptions.length, 1);
  assert.equal(manifest.contributes.commands, undefined);
  assert.equal(manifest.contributes.languageModelTools, undefined);
});

test("the server is the bundled hypha, started as hypha serve on the shared HYPHA_HOME", () => {
  const extension = activateExtension(true);
  const [server, ...others] = extension.providers.get(extension.providerId)!.provideMcpServerDefinitions();

  assert.equal(others.length, 0);
  assert.equal(server.label, "hypha", "the same server name as in every other client's manifest");
  assert.equal(server.command, "/extension/out/cli/hypha");
  assert.deepEqual([...server.args], ["serve"], "no --repository-root: hypha reads HYPHA_HOME");
  assert.deepEqual({ ...server.env }, {}, "no folder or toolchain of the extension's own");
  assert.equal(server.version, "9.8.7", "a new extension version makes the editor refresh the tools");
  assert.equal(server.cwd, undefined,
    "no working directory: hypha serve never reads it, and Copilot cannot migrate a definition that sets one");
});

test("a manifest without a version gives the server an empty version", () => {
  const extension = activateExtension(true, {});
  const [server] = extension.providers.get(extension.providerId)!.provideMcpServerDefinitions();

  assert.equal(server.version, "");
});

test("the manifest activates the extension at startup, since no command or tool does", () => {
  assert.deepEqual(manifest.activationEvents, ["onStartupFinished"]);
});

test("starting the server checks the executable, and a missing one says so", async () => {
  const present = activateExtension(true);
  const provider = present.providers.get(present.providerId)!;
  const [server] = provider.provideMcpServerDefinitions();
  assert.deepEqual(present.checked, [], "discovery does not touch the disk");
  assert.equal(await provider.resolveMcpServerDefinition(server), server);
  assert.deepEqual(present.checked, ["/extension"], "the executable is looked for in the extension's own folder");

  const missing = activateExtension(false);
  const broken = missing.providers.get(missing.providerId)!;
  assert.equal(broken.provideMcpServerDefinitions().length, 1, "discovery does not touch the disk");
  await assert.rejects(broken.resolveMcpServerDefinition(server), /bundled Hypha CLI is missing/);
});

test("the manifest needs the first VS Code that has the provider API", () => {
  assert.equal(manifest.engines.vscode, "^1.101.0");
  assert.equal(manifest.devDependencies["@types/vscode"], "1.101.0");
});
