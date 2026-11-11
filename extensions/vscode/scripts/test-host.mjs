import { existsSync } from "node:fs";
import { mkdtemp, mkdir, readdir, readFile, writeFile, rm } from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { downloadAndUnzipVSCode, resolveCliArgsFromVSCodeExecutablePath, runTests } from "@vscode/test-electron";
import { spawnSync } from "node:child_process";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const target = `${process.platform}-${process.arch}`;

/**
 * Finds the `cli.js` a Windows VS Code installation runs extension commands with. Older archives
 * keep it at `resources/app/out/` next to `Code.exe`; newer ones (seen in 1.145) put it under a
 * folder named after the build's commit (`<commit>/resources/app/out/cli.js`). The installation's own
 * launcher, `bin/code.cmd`, names the one it runs, so that is asked first; looking in both layouts
 * is the fallback for a launcher that does not.
 */
async function windowsCliScript(executable) {
  const installation = path.dirname(executable);
  try {
    const launcher = await readFile(path.join(installation, "bin", "code.cmd"), "utf8");
    // e.g. "%~dp0..\Code.exe" "%~dp0..\c4453f8ea4\resources\app\out\cli.js" %*
    const named = /"%~dp0\.\.\\([^"]*\\cli\.js)"/.exec(launcher);
    if (named && existsSync(path.join(installation, named[1]))) {
      return path.join(installation, named[1]);
    }
  } catch {
    // No readable launcher: fall back to the known layouts.
  }
  const relative = path.join("resources", "app", "out", "cli.js");
  const candidates = [path.join(installation, relative)];
  for (const entry of await readdir(installation, { withFileTypes: true })) {
    if (entry.isDirectory()) {
      candidates.push(path.join(installation, entry.name, relative));
    }
  }
  const script = candidates.find(candidate => existsSync(candidate));
  if (!script) {
    throw new Error(`No ${relative} found in ${installation} or a folder directly below it.`);
  }
  return script;
}

const temporary = await mkdtemp(path.join(os.tmpdir(), "hypha-vsix-host-"));
try {
  const userData = path.join(temporary, "user-data");
  const extensions = path.join(temporary, "extensions");
  const harness = path.join(temporary, "harness");
  const workspace = path.join(temporary, "modeling-workspace");
  await mkdir(harness, { recursive: true });
  await mkdir(workspace);
  await writeFile(path.join(harness, "package.json"), JSON.stringify({
    name: "hypha-validation-harness", publisher: "hypha-test", version: "0.0.1",
    engines: { vscode: "^1.100.0" }
  }));
  const executable = await downloadAndUnzipVSCode({
    version: process.env.VSCODE_TEST_VERSION ?? "stable",
    cachePath: path.join(root, ".vscode-test")
  });
  const [cli, ...cliArgs] = resolveCliArgsFromVSCodeExecutablePath(executable, { reuseMachineInstall: true });
  const installArgs = [...cliArgs,
    "--user-data-dir", userData, "--extensions-dir", extensions,
    "--install-extension", path.join(root, `hypha-${target}.vsix`), "--force"
  ];
  // The Linux launcher redirects to the current remote host when this is set.
  const { VSCODE_IPC_HOOK_CLI: _remoteCliHook, ...installationEnv } = process.env;
  const installation = process.platform === "win32"
    ? spawnSync(executable, [await windowsCliScript(executable), ...installArgs], {
      encoding: "utf8", env: { ...installationEnv, ELECTRON_RUN_AS_NODE: "1" }
    })
    : spawnSync(cli, installArgs, {
      encoding: "utf8", env: { ...installationEnv, DONT_PROMPT_WSL_INSTALL: "1" }
    });
  if (installation.error || installation.status !== 0) {
    throw installation.error ?? new Error(installation.stdout + installation.stderr);
  }
  console.log(installation.stdout);
  const storage = path.join(userData, "User", "globalStorage", "mycelium-cmbse.mycelium-hypha");
  const metamodel = path.join(storage, "knowledge/2026-09/metamodel");
  await mkdir(path.join(metamodel, "elements"), { recursive: true });
  await writeFile(path.join(storage, "knowledge/versions.json"),
    JSON.stringify({
      schemaVersion: "1.0.0",
      default: "2026-09",
      versions: [{
        tag: "2026-09",
        release: { repo: "fixture/release", commit: "fixture" },
        pilot: { repo: "fixture/pilot", commit: "fixture" }
      }]
    }));
  // Every field the metamodel generator writes: hypha reads the index through the same strict
  // contract as its query API and refuses one the generator could not have written.
  await writeFile(path.join(metamodel, "index.json"),
    JSON.stringify({
      schemaVersion: "1.0.0",
      metamodel: "SysML",
      modelVersionUri: "https://www.omg.org/spec/SysML/20250201",
      entries: {
        PartUsage: {
          kind: "class",
          package: "Parts",
          qualifiedName: "SysML::Systems::Parts::PartUsage",
          isAbstract: false,
          summary: "",
          extends: [],
          file: "elements/PartUsage.md"
        }
      }
    }));
  await writeFile(path.join(metamodel, "elements/PartUsage.md"), "# Synthetic PartUsage\nHost test fixture only.");
  await runTests({
    vscodeExecutablePath: executable,
    extensionDevelopmentPath: harness,
    extensionTestsPath: path.join(root, "host-tests/index.cjs"),
    launchArgs: [workspace, "--user-data-dir", userData, "--extensions-dir", extensions,
      "--disable-workspace-trust", "--skip-welcome", "--skip-release-notes", "--no-sandbox"],
    extensionTestsEnv: {
      HYPHA_EXPECTED_EXTENSION_DIRECTORY: extensions,
      HYPHA_EXPECTED_STORAGE_DIRECTORY: storage,
      HYPHA_TEST_NODE: process.execPath
    }
  });
} finally {
  await rm(temporary, { recursive: true, force: true });
}
