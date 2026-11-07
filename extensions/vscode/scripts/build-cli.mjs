import { spawnSync } from "node:child_process";
import { mkdir, rm, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const targets = {
  "linux-x64": "linux-x64",
  "linux-arm64": "linux-arm64",
  "win32-x64": "win-x64",
  "darwin-x64": "osx-x64",
  "darwin-arm64": "osx-arm64"
};
const target = process.argv[2] ?? `${process.platform}-${process.arch}`;
const rid = targets[target];
if (!rid) {
  throw new Error(`Unsupported extension target: ${target}`);
}
// The version the bundled CLI reports. A VSIX meant for publication sets HYPHA_CLI_VERSION to the
// hypha tools release it is built from; any other build is marked as a development build rather
// than taking the project's placeholder version.
const version = process.env.HYPHA_CLI_VERSION || "0.0.0-dev";
if (!/^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$/.test(version)) {
  throw new Error(`HYPHA_CLI_VERSION '${version}' is not SemVer.`);
}
const destination = path.join(root, "out", "cli");
await rm(destination, { recursive: true, force: true });
await mkdir(destination, { recursive: true });
const result = spawnSync(process.env.DOTNET ?? "dotnet", [
  "publish", path.resolve(root, "../../tools/hypha-cli/Hypha.Tools"),
  "-c", "Release", "-r", rid, "--self-contained", "true",
  "-o", destination, "-p:PublishTrimmed=false", `-p:Version=${version}`
], { stdio: "inherit" });
if (result.error || result.status !== 0) {
  throw result.error ?? new Error(`CLI publish failed (${result.status}).`);
}
await writeFile(path.join(destination, "target.json"), JSON.stringify({ target, rid, version }) + "\n");
