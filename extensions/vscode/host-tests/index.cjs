const assert = require("node:assert/strict");
const { spawn } = require("node:child_process");
const fs = require("node:fs/promises");
const path = require("node:path");
const vscode = require("vscode");

/**
 * Starts the bundled `hypha serve` as the editor would and asks it, over stdio, what it offers.
 * The editor only starts a registered MCP server once a chat needs it, which this host has no way to
 * trigger, so the server is driven directly: the same executable, arguments and transport.
 */
function askServer(executable, home) {
  return new Promise((resolve, reject) => {
    // Started from the scratch home: hypha prefers a source checkout above its working directory, and
    // this test is usually run from inside one.
    const child = spawn(executable, ["serve"], {
      cwd: home,
      env: { ...process.env, HYPHA_HOME: home },
      windowsHide: true
    });
    const answers = new Map();
    let buffer = "";
    const timer = setTimeout(() => {
      child.kill();
      reject(new Error("The bundled hypha serve did not answer within two minutes."));
    }, 120000);
    const send = (message) => child.stdin.write(JSON.stringify(message) + "\n");

    child.on("error", (error) => { clearTimeout(timer); reject(error); });
    child.stdout.setEncoding("utf8");
    child.stdout.on("data", (chunk) => {
      buffer += chunk;
      let newline;
      while ((newline = buffer.indexOf("\n")) >= 0) {
        const line = buffer.slice(0, newline).trim();
        buffer = buffer.slice(newline + 1);
        if (!line) { continue; }
        const message = JSON.parse(line);
        if (message.id === undefined || message.method !== undefined) { continue; }
        answers.set(message.id, message);
        if (message.id === 1) {
          send({ jsonrpc: "2.0", method: "notifications/initialized" });
          send({ jsonrpc: "2.0", id: 2, method: "tools/list" });
          send({ jsonrpc: "2.0", id: 3, method: "tools/call", params: { name: "hypha_release_status", arguments: {} } });
        }
        if (answers.has(2) && answers.has(3)) {
          clearTimeout(timer);
          child.stdin.end();
          child.on("close", () => resolve({ initialize: answers.get(1), tools: answers.get(2), status: answers.get(3) }));
        }
      }
    });
    send({
      jsonrpc: "2.0", id: 1, method: "initialize",
      params: { protocolVersion: "2025-06-18", capabilities: {}, clientInfo: { name: "hypha-host-test", version: "0" } }
    });
  });
}

exports.run = async function () {
  const extension = vscode.extensions.getExtension("mycelium-cmbse.mycelium-hypha");
  assert.ok(extension, "VSIX must be installed");
  assert.ok(extension.extensionPath.startsWith(process.env.HYPHA_EXPECTED_EXTENSION_DIRECTORY),
    "Must load the installed VSIX, not the source checkout");

  // Activation registers the MCP server definition provider; the editor throws there when the
  // provider's id is not the one the manifest declares.
  await extension.activate();
  const providers = extension.packageJSON.contributes.mcpServerDefinitionProviders;
  assert.deepEqual(providers.map((provider) => provider.id), ["mycelium-hypha.server"]);
  assert.equal(extension.packageJSON.contributes.languageModelTools, undefined, "the extension has no tools of its own");
  assert.equal(extension.packageJSON.contributes.commands, undefined, "nor commands");

  const executable = path.join(extension.extensionPath, "out/cli", process.platform === "win32" ? "hypha.exe" : "hypha");
  await fs.access(executable);
  await assert.rejects(fs.access(path.join(extension.extensionPath, "out/resources")),
    "hypha carries the specification extractor and the UML primitives; the extension ships no copy");

  const home = process.env.HYPHA_TEST_HOME;
  const { initialize, tools, status } = await askServer(executable, home);
  assert.equal(initialize.result.serverInfo.name, "hypha");
  const names = tools.result.tools.map((tool) => tool.name);
  assert.ok(names.length >= 17 && names.length < 20, `expected hypha's tools, got ${names.length}`);
  assert.ok(names.every((name) => name.startsWith("hypha_")));
  for (const name of ["hypha_release_status", "hypha_install_release", "hypha_metaclass", "hypha_lookup", "hypha_spec"]) {
    assert.ok(names.includes(name), `${name} is offered`);
  }

  // Nothing is installed in the scratch home, and the server says so rather than failing.
  assert.notEqual(status.result.isError, true);
  const answer = JSON.parse(status.result.content[0].text);
  assert.deepEqual(answer.releases, []);
  assert.deepEqual((await fs.readdir(home)).filter((name) => name === "knowledge"), [],
    "a read-only question installs nothing");

  console.log(`Installed VSIX activated, and its bundled hypha serve offered ${names.length} tools on a scratch HYPHA_HOME.`);
};
