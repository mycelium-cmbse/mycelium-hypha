import assert from "node:assert/strict";
import { chmod, mkdir, mkdtemp, rm, writeFile } from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import test from "node:test";

import { cliPath, ensureCli } from "./cli";

test("the bundled executable is named for its platform", () => {
  assert.equal(cliPath("/extension", "win32"), path.join("/extension", "out", "cli", "hypha.exe"));
  assert.equal(cliPath("/extension", "linux"), path.join("/extension", "out", "cli", "hypha"));
  assert.equal(cliPath("/extension", "darwin"), path.join("/extension", "out", "cli", "hypha"));
});

test("a missing executable says how to get one", async () => {
  const extension = await mkdtemp(path.join(os.tmpdir(), "hypha-cli-missing-"));
  try {
    await assert.rejects(ensureCli(extension), (error: Error & { cause?: { code?: string } }) => {
      assert.match(error.message, /bundled Hypha CLI is missing or not executable.*Install the VSIX/);
      assert.equal(error.cause?.code, "ENOENT", "the cause says which of the two it was");
      return true;
    });
  } finally {
    await rm(extension, { recursive: true, force: true });
  }
});

test("an executable that is there is returned by its path", async () => {
  const extension = await mkdtemp(path.join(os.tmpdir(), "hypha-cli-present-"));
  try {
    // Named here, not by the code under test, so a wrong default platform would show.
    const executable = path.join(extension, "out", "cli", process.platform === "win32" ? "hypha.exe" : "hypha");
    await mkdir(path.dirname(executable), { recursive: true });
    await writeFile(executable, "");
    await chmod(executable, 0o755);

    assert.equal(await ensureCli(extension), executable);
  } finally {
    await rm(extension, { recursive: true, force: true });
  }
});

// Windows has no execute bit (X_OK is F_OK there), and root may execute anything.
const canRefuse = process.platform !== "win32" && process.getuid?.() !== 0;

test("an executable without the execute bit is refused", { skip: !canRefuse }, async () => {
  const extension = await mkdtemp(path.join(os.tmpdir(), "hypha-cli-noexec-"));
  try {
    const executable = cliPath(extension);
    await mkdir(path.dirname(executable), { recursive: true });
    await writeFile(executable, "");
    await chmod(executable, 0o644);

    await assert.rejects(ensureCli(extension), (error: Error & { cause?: { code?: string } }) => {
      assert.match(error.message, /missing or not executable/);
      assert.equal(error.cause?.code, "EACCES");
      return true;
    });
  } finally {
    await rm(extension, { recursive: true, force: true });
  }
});
