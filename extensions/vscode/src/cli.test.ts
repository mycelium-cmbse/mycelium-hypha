import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { mkdtemp, rm } from "node:fs/promises";
import path from "node:path";
import os from "node:os";
import { readFile } from "node:fs/promises";
import test from "node:test";
import vm from "node:vm";

class CancellationError extends Error {}

function loadCli() {
  const exports: Record<string, unknown> = {};
  vm.runInNewContext(readFileSync(path.join(__dirname, "cli.js"), "utf8"), {
    exports,
    process,
    require(name: string) {
      if (name === "node:child_process") { return require("node:child_process"); }
      if (name === "node:fs/promises") { return require("node:fs/promises"); }
      if (name === "node:path") { return require("node:path"); }
      if (name === "vscode") { return { CancellationError }; }
      throw new Error(`Unexpected import: ${name}`);
    }
  });
  return exports as {
    ensureCli(extensionPath: string): Promise<string>;
    runCli(
      executable: string,
      storagePath: string,
      args: string[],
      output: { append(value: string): void },
      token: { isCancellationRequested: boolean; onCancellationRequested(callback: () => void): { dispose(): void } },
      captureStdout?: boolean
    ): Promise<string>;
    runCliJson<T>(
      executable: string,
      storagePath: string,
      args: string[],
      output: { append(value: string): void },
      token: { isCancellationRequested: boolean; onCancellationRequested(callback: () => void): { dispose(): void } }
    ): Promise<T>;
  };
}

function createToken(initiallyCancelled = false) {
  let cancelled = initiallyCancelled;
  const listeners: (() => void)[] = [];
  return {
    token: {
      get isCancellationRequested() { return cancelled; },
      onCancellationRequested(callback: () => void) {
        listeners.push(callback);
        return { dispose() {} };
      }
    },
    cancel() {
      cancelled = true;
      listeners.forEach((listener) => listener());
    }
  };
}

async function withStorage(run: (storage: string) => Promise<void>): Promise<void> {
  const storage = await mkdtemp(path.join(os.tmpdir(), "hypha-cli-tests-"));
  try {
    await run(storage);
  } finally {
    await rm(storage, { recursive: true, force: true });
  }
}

test("an already-cancelled run rejects without spawning", async () => {
  const cli = loadCli();
  const cancellation = createToken(true);
  await assert.rejects(
    cli.runCli(process.execPath, os.tmpdir(), [], { append() {} }, cancellation.token),
    (error: Error) => error instanceof CancellationError
  );
});

test("cancelling a running CLI stops it and rejects with CancellationError", async () => {
  const cli = loadCli();
  await withStorage(async (storage) => {
    const cancellation = createToken();
    const execution = cli.runCli(
      process.execPath,
      storage,
      ["-e", "setInterval(() => {}, 1000)", "--"],
      { append() {} },
      cancellation.token
    );
    setTimeout(cancellation.cancel, 100);
    await assert.rejects(execution, (error: Error) => error instanceof CancellationError);
  });
});

test("a non-zero JSON command exposes its error and flushes captured stdout", async () => {
  const cli = loadCli();
  await withStorage(async (storage) => {
    const chunks: string[] = [];
    await assert.rejects(
      cli.runCli(
        process.execPath,
        storage,
        ["-e", "console.log(JSON.stringify({error: 'No release is installed.'})); process.exit(1)", "--"],
        { append(value) { chunks.push(value); } },
        createToken().token,
        false
      ),
      /The Hypha CLI failed: No release is installed\./
    );
    assert.ok(chunks.join("").includes('"error":"No release is installed."'));
  });
});

test("a missing executable reports that the CLI could not start", async () => {
  const cli = loadCli();
  await withStorage(async (storage) => {
    await assert.rejects(
      cli.runCli(
        path.join(storage, "missing-hypha"),
        storage,
        [],
        { append() {} },
        createToken().token
      ),
      /Could not start the Hypha CLI/
    );
  });
});

test("ensureCli rejects when the bundled executable is missing", async () => {
  const cli = loadCli();
  await withStorage(async (extensionPath) => {
    await assert.rejects(cli.ensureCli(extensionPath), /missing or not executable/);
  });
});

test("runCliJson rejects malformed output", async () => {
  const cli = loadCli();
  await withStorage(async (storage) => {
    await assert.rejects(
      cli.runCliJson(
        process.execPath,
        storage,
        ["-e", "process.stdout.write('not json')", "--"],
        { append() {} },
        createToken().token
      ),
      /The Hypha CLI returned invalid JSON/
    );
  });
});

test("the CLI receives extension storage roots and uv cache environment", async () => {
  const cli = loadCli();
  await withStorage(async (storage) => {
    const output: string[] = [];
    const result = await cli.runCli(
      process.execPath,
      storage,
      ["-e", "console.log(JSON.stringify({args: process.argv.slice(1), cache: process.env.UV_CACHE_DIR, python: process.env.UV_PYTHON_INSTALL_DIR}))", "--"],
      { append(value) { output.push(value); } },
      createToken().token
    );
    const probe = JSON.parse(result) as { args: string[]; cache: string; python: string };
    assert.deepEqual(probe.args.slice(-5), [
      "--repository-root", storage, "--toolchain-cache-root", path.join(storage, "toolchain"), "--no-logo"
    ]);
    assert.equal(probe.cache, path.join(storage, "toolchain", "uv-cache"));
    assert.equal(probe.python, path.join(storage, "toolchain", "python"));
  });
});
