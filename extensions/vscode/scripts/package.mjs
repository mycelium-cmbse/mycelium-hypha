import { spawnSync } from "node:child_process";
import { cp, readFile } from "node:fs/promises";
import { createRequire } from "node:module";
import path from "node:path";
import { fileURLToPath } from "node:url";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
// Usage: package.mjs [target] [--pre-release]. The Marketplace reads the pre-release flag from the
// VSIX itself, so it has to be set here rather than when publishing.
const args = process.argv.slice(2);
const preRelease = args.includes("--pre-release");
const unknown = args.filter(arg => arg.startsWith("--") && arg !== "--pre-release");
if (unknown.length > 0) {
  throw new Error(`Unknown option(s): ${unknown.join(" ")}`);
}
const target = args.find(arg => !arg.startsWith("--")) ?? `${process.platform}-${process.arch}`;
const clean = spawnSync(process.execPath, [path.join(root, "scripts/clean-output.mjs")], { stdio: "inherit" });
if (clean.error || clean.status !== 0) {
  throw clean.error ?? new Error("Cleaning the previous extension build failed.");
}
const build = spawnSync(process.execPath, [path.join(root, "scripts/build-cli.mjs"), target], { stdio: "inherit" });
if (build.error || build.status !== 0) {
  throw build.error ?? new Error("Building the bundled CLI failed.");
}
const require = createRequire(import.meta.url);
for (const name of ["LICENSE", "NOTICE"]) {
  await cp(path.resolve(root, "../..", name), path.join(root, name));
}
const vsce = require.resolve("@vscode/vsce/vsce");
const result = spawnSync(process.execPath, [
  vsce, "package", "--target", target, "--out", path.join(root, `hypha-${target}.vsix`),
  ...(preRelease ? ["--pre-release"] : [])
], { cwd: root, stdio: "inherit" });
if (result.error || result.status !== 0) {
  throw result.error ?? new Error("VSIX packaging failed.");
}
const listing = spawnSync(process.execPath, [vsce, "ls", "--no-dependencies"], {
  cwd: root, encoding: "utf8"
});
if (listing.error || listing.status !== 0) {
  throw listing.error ?? new Error(`Could not validate package contents: ${listing.stderr}`);
}
const files = listing.stdout.trim().split(/\r?\n/);
const required = [
  "out/extension.js", "out/cli.js", "out/cli/target.json",
  `out/cli/${target.startsWith("win32-") ? "hypha.exe" : "hypha"}`,
  "out/resources/sources/PrimitiveTypes.xmi",
  "out/resources/spec-extract/pyproject.toml", "out/resources/spec-extract/uv.lock",
  "out/resources/spec-extract/src/spec_extract/__main__.py", "LICENSE", "NOTICE", "icon.png", "CHANGELOG.md"
];
for (const file of required) {
  if (!files.includes(file)) {
    throw new Error(`Required VSIX input is missing: ${file}`);
  }
}
for (const file of files) {
  if (/^(?:node_modules|host-tests|knowledge|sources)\//.test(file) ||
      /(?:\.pdf|\.pyc|\.test\.js)$/.test(file) || file.includes("__pycache__/")) {
    throw new Error(`Forbidden VSIX input: ${file}`);
  }
}
const built = JSON.parse(await readFile(path.join(root, "out/cli/target.json"), "utf8"));
if (built.target !== target) {
  throw new Error(`CLI target ${built.target} does not match VSIX target ${target}.`);
}
const expectedVersion = process.env.HYPHA_CLI_VERSION || "0.0.0-dev";
if (built.version !== expectedVersion) {
  throw new Error(`CLI version ${built.version} does not match the requested ${expectedVersion}.`);
}
console.log(`Validated ${files.length} package inputs for ${target}${preRelease ? " (pre-release)" : ""} ` +
  `with hypha CLI ${built.version}.`);
