import { mkdtemp, mkdir, writeFile, rm } from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { downloadAndUnzipVSCode, resolveCliArgsFromVSCodeExecutablePath, runTests } from "@vscode/test-electron";
import { spawnSync } from "node:child_process";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const target = `${process.platform}-${process.arch}`;
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
  const installation = process.platform === "win32"
    ? spawnSync(executable, [path.resolve(path.dirname(cli), "../resources/app/out/cli.js"), ...installArgs], {
      encoding: "utf8", env: { ...process.env, ELECTRON_RUN_AS_NODE: "1" }
    })
    : spawnSync(cli, installArgs, {
      encoding: "utf8", env: { ...process.env, DONT_PROMPT_WSL_INSTALL: "1" }
    });
  if (installation.error || installation.status !== 0) {
    throw installation.error ?? new Error(installation.stdout + installation.stderr);
  }
  console.log(installation.stdout);
  const storage = path.join(userData, "User", "globalStorage", "mycelium-cmbse.mycelium-hypha");
  const metamodel = path.join(storage, "knowledge/2026-09/metamodel");
  await mkdir(path.join(metamodel, "elements"), { recursive: true });
  await writeFile(path.join(storage, "knowledge/versions.json"),
    JSON.stringify({ default: "2026-09", versions: [{ tag: "2026-09" }] }));
  await writeFile(path.join(metamodel, "index.json"),
    JSON.stringify({ entries: { PartUsage: { file: "elements/PartUsage.md" } } }));
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
