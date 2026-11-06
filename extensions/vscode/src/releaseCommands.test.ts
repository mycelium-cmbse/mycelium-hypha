import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import path from "node:path";
import test from "node:test";
import vm from "node:vm";

test("completed removal releases the guard while its notification remains open", async () => {
  const commands = new Map<string, () => Promise<void>>();
  const notifications: string[] = [];
  const cliCalls: string[][] = [];
  const exports: Record<string, any> = {};
  const vscode = {
    commands: {
      registerCommand(name: string, callback: () => Promise<void>) {
        commands.set(name, callback);
        return { dispose() {} };
      }
    },
    window: {
      showInformationMessage(message: string) {
        notifications.push(message);
        return new Promise(() => {});
      },
      showErrorMessage(message: string) {
        assert.fail(message);
      },
      async showQuickPick(items: { label: string }[]) { return items[0]; },
      async showWarningMessage() { return "Remove Release"; },
      async withProgress(_options: unknown, callback: Function) {
        return callback({ report() {} }, {});
      }
    },
    ProgressLocation: { Notification: 1 },
    CancellationTokenSource: class {
      token = {};
      dispose() {}
    },
    CancellationError: class extends Error {}
  };
  const cli = {
    async ensureCli() { return "hypha"; },
    async runCli(_executable: string, _storage: string, args: string[]) {
      cliCalls.push(args);
      return "";
    },
    async runCliJson(_executable: string, _storage: string, args: string[]) {
      cliCalls.push(args);
      return args[0] === "list"
        ? { defaultTag: "2026-09", releases: [{ tag: "2026-09", isDefault: true }] }
        : { installedTags: [], defaultTag: null, availableOnline: [] };
    }
  };
  vm.runInNewContext(await readFile(path.join(__dirname, "releaseCommands.js"), "utf8"), {
    exports,
    require(name: string) {
      if (name === "vscode") { return vscode; }
      if (name === "./cli") { return cli; }
      if (name === "./runtimeAssets") { return {}; }
      throw new Error(`Unexpected import: ${name}`);
    }
  });
  exports.registerReleaseCommands(
    { globalStorageUri: { fsPath: "storage" }, extensionUri: { fsPath: "extension" } },
    { appendLine() {}, show() {} }
  );

  const removal = commands.get("mycelium-hypha.removeRelease")!();
  await new Promise<void>((resolve) => setImmediate(resolve));
  assert.ok(notifications.includes("Hypha release 2026-09 was removed."));
  await commands.get("mycelium-hypha.installRelease")!();
  assert.ok(cliCalls.some((args) => args[0] === "check"));
  assert.ok(!notifications.includes("A Hypha release operation is already in progress."));
  await removal;
});