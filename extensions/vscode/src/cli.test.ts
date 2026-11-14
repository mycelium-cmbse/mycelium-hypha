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
    await assert.rejects(ensureCli(extension), /bundled Hypha CLI is missing or not executable.*Install the VSIX/);
  } finally {
    await rm(extension, { recursive: true, force: true });
  }
});

test("an executable that is there is returned by its path", async () => {
  const extension = await mkdtemp(path.join(os.tmpdir(), "hypha-cli-present-"));
  try {
    const executable = cliPath(extension);
    await mkdir(path.dirname(executable), { recursive: true });
    await writeFile(executable, "");
    await chmod(executable, 0o755);

    assert.equal(await ensureCli(extension), executable);
  } finally {
    await rm(extension, { recursive: true, force: true });
  }
});
