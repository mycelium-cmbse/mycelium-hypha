const assert = require("node:assert/strict");
const path = require("node:path");
const fs = require("node:fs/promises");
const vscode = require("vscode");

exports.run = async function () {
  const extension = vscode.extensions.getExtension("mycelium-cmbse.mycelium-hypha");
  assert.ok(extension, "VSIX must be installed");
  assert.ok(extension.extensionPath.startsWith(process.env.HYPHA_EXPECTED_EXTENSION_DIRECTORY),
    "Must load the installed VSIX, not the source checkout");
  await extension.activate();
  await fs.access(path.join(extension.extensionPath, "out/cli", process.platform === "win32" ? "hypha.exe" : "hypha"));
  const status = await vscode.lm.invokeTool("mycelium-hypha_get_release_status", {
    input: {}, toolInvocationToken: undefined
  });
  assert.match(status.content.map(part => part.value ?? "").join(""), /2026-09/);
  const result = await vscode.lm.invokeTool("mycelium-hypha_lookup_knowledge", {
    input: { kind: "metamodel", name: "PartUsage" }, toolInvocationToken: undefined
  });
  const data = JSON.parse(result.content[0].value);
  assert.equal(data.release, "2026-09");
  assert.match(data.content, /Host test fixture only/);
  assert.match(data.source, /globalStorage/);
  const storage = process.env.HYPHA_EXPECTED_STORAGE_DIRECTORY;
  const { ensureCli, runCli } = require(path.join(extension.extensionPath, "out/cli.js"));
  const output = { append() {} };
  const cancellation = new vscode.CancellationTokenSource();
  const cli = await ensureCli(extension.extensionPath);
  assert.match(await runCli(cli, storage, ["--version"], output, cancellation.token), /\d+\.\d+/);
  const probe = JSON.parse(await runCli(process.env.HYPHA_TEST_NODE, storage, ["-e", `
    console.log(JSON.stringify({
      args: process.argv.slice(1),
      cache: process.env.UV_CACHE_DIR,
      python: process.env.UV_PYTHON_INSTALL_DIR
    }))
  `, "--"], output, cancellation.token));
  cancellation.dispose();
  assert.equal(probe.cache, path.join(storage, "toolchain", "uv-cache"));
  assert.equal(probe.python, path.join(storage, "toolchain", "python"));
  assert.deepEqual(probe.args, [
    "--repository-root", storage, "--toolchain-cache-root", path.join(storage, "toolchain"), "--no-logo"
  ]);
  const { releaseReadiness } = require(path.join(extension.extensionPath, "out/readiness.js"));
  const uri = vscode.Uri.file(storage);
  const spec = path.join(storage, "knowledge/2026-09/spec");
  await fs.mkdir(path.join(spec, "kerml"), { recursive: true });
  await fs.writeFile(path.join(spec, "index.md"), "obsolete layout");
  await fs.writeFile(path.join(spec, "kerml/index.md"), "synthetic catalog");
  assert.match(await releaseReadiness(uri, "2026-09"), /Normative specification text is not present/);
  await fs.mkdir(path.join(spec, "sysml2"), { recursive: true });
  await fs.writeFile(path.join(spec, "sysml2/index.md"), "synthetic catalog");
  assert.match(await releaseReadiness(uri, "2026-09"), /Normative specification text is present/);
  console.log("Installed VSIX activated and both Copilot tools returned global-storage data.");
};
