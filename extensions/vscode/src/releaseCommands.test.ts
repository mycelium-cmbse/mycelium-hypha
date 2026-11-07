import assert from "node:assert/strict";
import * as fs from "node:fs/promises";
import { mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import { readFile as readSource } from "node:fs/promises";
import test from "node:test";
import vm from "node:vm";

interface Release {
  tag: string;
  isDefault: boolean;
}

interface HarnessOptions {
  storagePath?: string;
  availableOnline?: string[];
  releases?: Release[];
  selection?: string | null;
  answer?: string | undefined;
  failCommand?: string;
  cancelCommand?: string;
  beforeCommand?: (args: string[]) => Promise<void>;
  beforeRemoveReleaseLock?: () => Promise<void>;
}

async function createHarness(options: HarnessOptions = {}) {
  const storagePath = options.storagePath ?? await mkdtemp(path.join(os.tmpdir(), "hypha-release-tests-"));
  const ownsStorage = options.storagePath === undefined;
  const commands = new Map<string, () => Promise<void>>();
  const calls: string[][] = [];
  const jsonCalls: string[][] = [];
  const notifications: string[] = [];
  const errors: string[] = [];
  const exports: {
    registerReleaseCommands?: (context: unknown, output: unknown) => unknown;
  } = {};
  const releases = options.releases ?? [{ tag: "2026-09", isDefault: true }];
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
        return Promise.resolve(undefined);
      },
      showErrorMessage(message: string) {
        errors.push(message);
        return Promise.resolve(undefined);
      },
      async showQuickPick(items: { label: string }[]) {
        if (options.selection === null) {
          return undefined;
        }
        return items.find((item) => item.label === options.selection) ?? items[0];
      },
      async showWarningMessage(_message: string, ...actions: unknown[]) {
        return options.answer === undefined ? actions.at(-1) : options.answer;
      },
      async withProgress(_options: unknown, callback: Function) {
        return callback({ report() {} }, { isCancellationRequested: false });
      }
    },
    ProgressLocation: { Notification: 1 },
    CancellationTokenSource: class {
      token = { isCancellationRequested: false };
      dispose() {}
    },
    CancellationError: class extends Error {}
  };
  const cli = {
    async ensureCli() { return "hypha"; },
    async runCli(_executable: string, _storage: string, args: string[]) {
      calls.push([...args]);
      await options.beforeCommand?.(args);
      if (args[0] === options.failCommand) {
        throw new Error(`failed ${args[0]}`);
      }
      if (args[0] === options.cancelCommand) {
        throw new vscode.CancellationError();
      }
      return "";
    },
    async runCliJson(_executable: string, _storage: string, args: string[]) {
      jsonCalls.push([...args]);
      return args[0] === "check"
        ? {
          installedTags: releases.map((release) => release.tag),
          defaultTag: releases.find((release) => release.isDefault)?.tag ?? null,
          availableOnline: options.availableOnline ?? ["2026-09"]
        }
        : {
          defaultTag: releases.find((release) => release.isDefault)?.tag ?? null,
          releases: releases.map((release) => ({
            ...release,
            artifacts: {
              metamodel: true,
              textualNotation: true,
              modelLibrary: true,
              crossReferences: true,
              specification: false,
              core: true
            }
          }))
        };
    }
  };
  vm.runInNewContext(await readSource(path.join(__dirname, "releaseCommands.js"), "utf8"), {
    exports,
    process,
    require(name: string) {
      if (name === "vscode") { return vscode; }
      if (name === "./cli") { return cli; }
      if (name === "./runtimeAssets") { return { provisionRuntimeAssets: async () => {} }; }
      if (name === "node:crypto") { return require("node:crypto"); }
      if (name === "node:fs/promises") {
        return {
          ...fs,
          async rm(target: Parameters<typeof rm>[0], rmOptions?: Parameters<typeof rm>[1]) {
            if (String(target) === path.join(storagePath, ".release-operation.lock")) {
              await options.beforeRemoveReleaseLock?.();
            }
            return rm(target, rmOptions);
          }
        };
      }
      if (name === "node:path") { return require("node:path"); }
      throw new Error(`Unexpected import: ${name}`);
    }
  });
  exports.registerReleaseCommands!(
    { globalStorageUri: { fsPath: storagePath }, extensionUri: { fsPath: "extension" } },
    { appendLine() {}, show() {} }
  );

  return {
    calls,
    commands,
    errors,
    jsonCalls,
    notifications,
    storagePath,
    async dispose() {
      if (ownsStorage) {
        await rm(storagePath, { recursive: true, force: true });
      }
    }
  };
}

test("install cancellation points and empty upstream results do not mutate releases", async () => {
  for (const options of [
    { selection: null },
    { answer: "Cancel" },
    { availableOnline: [] }
  ] satisfies HarnessOptions[]) {
    const harness = await createHarness(options);
    try {
      await harness.commands.get("mycelium-hypha.installRelease")!();
      assert.deepEqual(harness.calls, []);
    } finally {
      await harness.dispose();
    }
  }
});

test("install fetches without changing the default, then generates and selects the release", async () => {
  const harness = await createHarness();
  try {
    await harness.commands.get("mycelium-hypha.installRelease")!();
    assert.deepEqual(harness.calls.map((args) => args.slice(0, 3)), [
      ["fetch", "--tag", "2026-09"],
      ["generate", "--tag", "2026-09"],
      ["use", "--tag", "2026-09"]
    ]);
    assert.ok(harness.calls[0].includes("--no-default"));
  } finally {
    await harness.dispose();
  }
});

test("failed or cancelled generation never selects the new release and releases its guard", async () => {
  const options: HarnessOptions = { failCommand: "generate" };
  const harness = await createHarness(options);
  try {
    await harness.commands.get("mycelium-hypha.installRelease")!();
    assert.ok(!harness.calls.some((args) => args[0] === "use"));
    assert.ok(harness.errors.some((message) => message.includes("failed generate")));

    options.failCommand = undefined;
    harness.calls.length = 0;
    options.cancelCommand = "generate";
    await harness.commands.get("mycelium-hypha.installRelease")!();
    assert.ok(!harness.calls.some((args) => args[0] === "use"));
    assert.equal(harness.errors.length, 1);

    options.cancelCommand = undefined;
    harness.calls.length = 0;
    await harness.commands.get("mycelium-hypha.installRelease")!();
    assert.ok(harness.calls.some((args) => args[0] === "fetch"));
    assert.ok(harness.calls.some((args) => args[0] === "use"));
    assert.ok(!harness.notifications.includes("A Hypha release operation is already in progress."));
  } finally {
    await harness.dispose();
  }
});

test("use reports an empty installation without invoking a mutation", async () => {
  const harness = await createHarness({ releases: [] });
  try {
    await harness.commands.get("mycelium-hypha.useRelease")!();
    assert.deepEqual(harness.calls, []);
    assert.ok(harness.notifications.includes("No Hypha releases are installed."));
  } finally {
    await harness.dispose();
  }
});

test("a stale lock from an exited process is removed before mutation", async () => {
  const harness = await createHarness();
  try {
    await writeFile(
      path.join(harness.storagePath, ".release-operation.lock"),
      JSON.stringify({ pid: 2147483647, token: "stale" })
    );
    await harness.commands.get("mycelium-hypha.installRelease")!();
    assert.ok(harness.calls.some((args) => args[0] === "fetch"));
    await assert.rejects(readFile(path.join(harness.storagePath, ".release-operation.lock")), { code: "ENOENT" });
  } finally {
    await harness.dispose();
  }
});

test("concurrent stale-lock recovery cannot remove a new owner's lock", async () => {
  const storagePath = await mkdtemp(path.join(os.tmpdir(), "hypha-recovery-tests-"));
  let releaseRemoval!: () => void;
  let removalStarted!: () => void;
  const removalReached = new Promise<void>((resolve) => { removalStarted = resolve; });
  const removalMayContinue = new Promise<void>((resolve) => { releaseRemoval = resolve; });
  let releaseMutation!: () => void;
  let mutationStarted!: () => void;
  const mutationReached = new Promise<void>((resolve) => { mutationStarted = resolve; });
  const mutationMayContinue = new Promise<void>((resolve) => { releaseMutation = resolve; });
  const first = await createHarness({
    storagePath,
    async beforeRemoveReleaseLock() {
      removalStarted();
      await removalMayContinue;
    },
    async beforeCommand(args) {
      if (args[0] === "fetch") {
        mutationStarted();
        await mutationMayContinue;
      }
    }
  });
  const second = await createHarness({ storagePath });
  try {
    await writeFile(
      path.join(storagePath, ".release-operation.lock"),
      JSON.stringify({ pid: 2147483647, token: "stale" })
    );
    const firstInstall = first.commands.get("mycelium-hypha.installRelease")!();
    await removalReached;
    await second.commands.get("mycelium-hypha.installRelease")!();
    assert.ok(second.errors.some((message) => message.includes("recovering a release-operation lock")));
    assert.deepEqual(second.calls, []);

    releaseRemoval();
    await mutationReached;
    await second.commands.get("mycelium-hypha.installRelease")!();
    assert.ok(second.errors.some((message) => message.includes("already running in another VS Code window")));
    assert.deepEqual(second.calls, []);
    assert.equal(
      JSON.parse(await readFile(path.join(storagePath, ".release-operation.lock"), "utf8")).pid,
      process.pid
    );
    releaseMutation();
    await firstInstall;
  } finally {
    releaseRemoval();
    releaseMutation();
    await first.dispose();
    await second.dispose();
    await rm(storagePath, { recursive: true, force: true });
  }
});

test("an interrupted recovery fails safely without deleting either lock", async () => {
  const harness = await createHarness();
  const lockPath = path.join(harness.storagePath, ".release-operation.lock");
  const recoveryPath = `${lockPath}.recovery`;
  const stale = JSON.stringify({ pid: 2147483647, token: "stale" });
  try {
    await writeFile(lockPath, stale);
    await writeFile(recoveryPath, "");
    await harness.commands.get("mycelium-hypha.installRelease")!();
    assert.deepEqual(harness.calls, []);
    assert.ok(harness.errors.some((message) => message.includes("no Hypha processes are running")));
    assert.equal(await readFile(lockPath, "utf8"), stale);
    assert.equal(await readFile(recoveryPath, "utf8"), "");
  } finally {
    await harness.dispose();
  }
});

test("use selects an installed release with exactly the CLI tag arguments", async () => {
  const harness = await createHarness({
    releases: [
      { tag: "2026-09", isDefault: true },
      { tag: "2026-05", isDefault: false }
    ],
    selection: "2026-05"
  });
  try {
    await harness.commands.get("mycelium-hypha.useRelease")!();
    assert.deepEqual(harness.calls.map((args) => args.slice(0, 3)), [["use", "--tag", "2026-05"]]);
  } finally {
    await harness.dispose();
  }
});

test("remove refuses the default while another release is installed and declines without mutation", async () => {
  const defaultHarness = await createHarness({
    releases: [
      { tag: "2026-09", isDefault: true },
      { tag: "2026-05", isDefault: false }
    ]
  });
  try {
    await defaultHarness.commands.get("mycelium-hypha.removeRelease")!();
    assert.deepEqual(defaultHarness.calls, []);
    assert.ok(defaultHarness.notifications.some((message) => message.includes("Switch to another default")));
  } finally {
    await defaultHarness.dispose();
  }

  const declinedHarness = await createHarness({ answer: "Cancel" });
  try {
    await declinedHarness.commands.get("mycelium-hypha.removeRelease")!();
    assert.deepEqual(declinedHarness.calls, []);
  } finally {
    await declinedHarness.dispose();
  }
});

test("remove uses force only for the final installed release", async () => {
  const finalRelease = await createHarness();
  try {
    await finalRelease.commands.get("mycelium-hypha.removeRelease")!();
    assert.ok(finalRelease.calls[0].includes("--force"));
  } finally {
    await finalRelease.dispose();
  }

  const nonFinalRelease = await createHarness({
    releases: [
      { tag: "2026-09", isDefault: true },
      { tag: "2026-05", isDefault: false }
    ],
    selection: "2026-05"
  });
  try {
    await nonFinalRelease.commands.get("mycelium-hypha.removeRelease")!();
    assert.deepEqual(nonFinalRelease.calls[0].slice(0, 3), ["remove", "--tag", "2026-05"]);
    assert.ok(!nonFinalRelease.calls[0].includes("--force"));
  } finally {
    await nonFinalRelease.dispose();
  }
});

test("a second VS Code window cannot mutate the shared storage during an operation", async () => {
  const storagePath = await mkdtemp(path.join(os.tmpdir(), "hypha-shared-release-tests-"));
  let releaseFirstCommand!: () => void;
  let firstCommandStarted!: () => void;
  const firstStarted = new Promise<void>((resolve) => { firstCommandStarted = resolve; });
  const firstMayFinish = new Promise<void>((resolve) => { releaseFirstCommand = resolve; });
  const first = await createHarness({
    storagePath,
    async beforeCommand(args) {
      if (args[0] === "fetch") {
        firstCommandStarted();
        await firstMayFinish;
      }
    }
  });
  const second = await createHarness({ storagePath });
  try {
    const firstInstall = first.commands.get("mycelium-hypha.installRelease")!();
    await firstStarted;
    await first.commands.get("mycelium-hypha.removeRelease")!();
    assert.ok(first.notifications.includes("A Hypha release operation is already in progress."));
    await second.commands.get("mycelium-hypha.installRelease")!();
    assert.ok(second.errors.some((message) => message.includes("already running in another VS Code window")));
    assert.deepEqual(second.calls, []);
    releaseFirstCommand();
    await firstInstall;
  } finally {
    releaseFirstCommand();
    await first.dispose();
    await second.dispose();
    await rm(storagePath, { recursive: true, force: true });
  }
});
