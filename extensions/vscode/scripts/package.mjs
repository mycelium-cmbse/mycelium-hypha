import { spawnSync } from "node:child_process";
import { cp, readFile, rm } from "node:fs/promises";
import { createRequire } from "node:module";
import path from "node:path";
import { fileURLToPath } from "node:url";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const target = process.argv[2] ?? `${process.platform}-${process.arch}`;
const build = spawnSync(process.execPath, [path.join(root, "scripts/build-cli.mjs"), target], { stdio: "inherit" });
if (build.error || build.status !== 0) {
  throw build.error ?? new Error("Building the bundled CLI failed.");
}
const require = createRequire(import.meta.url);
for (const name of ["LICENSE", "NOTICE"]) {
  await cp(path.resolve(root, "../..", name), path.join(root, name));
}
for (const name of ["specExtractor.js", "specExtractor.js.map"]) {
  await rm(path.join(root, "out", name), { force: true });
}
const vsce = require.resolve("@vscode/vsce/vsce");
const result = spawnSync(process.execPath, [
  vsce, "package", "--target", target, "--out", path.join(root, `hypha-${target}.vsix`)
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
  "out/extension.js", "out/knowledge.js", "out/cli/target.json",
  `out/cli/${target.startsWith("win32-") ? "hypha.exe" : "hypha"}`,
  "out/resources/sources/PrimitiveTypes.xmi",
  "out/resources/spec-extract/pyproject.toml", "out/resources/spec-extract/uv.lock",
  "out/resources/spec-extract/src/spec_extract/__main__.py", "LICENSE", "NOTICE"
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
console.log(`Validated ${files.length} package inputs for ${target}.`);
