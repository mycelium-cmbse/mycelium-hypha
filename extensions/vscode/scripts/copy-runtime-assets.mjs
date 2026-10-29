import { cp, mkdir } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const source = path.resolve(scriptDirectory, "../../../tools/spec-extract");
const destination = path.resolve(scriptDirectory, "../out/resources/spec-extract");

await mkdir(path.resolve(scriptDirectory, "../out/resources/sources"), { recursive: true });
await cp(
  path.resolve(scriptDirectory, "../../../sources/PrimitiveTypes.xmi"),
  path.resolve(scriptDirectory, "../out/resources/sources/PrimitiveTypes.xmi")
);
for (const name of ["LICENSE", "NOTICE"]) {
  await cp(
    path.resolve(scriptDirectory, "../../..", name),
    path.resolve(scriptDirectory, "../out/resources", name)
  );
}

await mkdir(path.dirname(destination), { recursive: true });
await cp(source, destination, {
  recursive: true,
  filter: (sourcePath) => {
    const relative = path.relative(source, sourcePath);
    if (relative.split(path.sep).includes("__pycache__") || relative.endsWith(".pyc")) {
      return false;
    }
    return relative === "" ||
      relative === "README.md" ||
      relative === "pyproject.toml" ||
      relative === "uv.lock" ||
      relative === "src" ||
      relative.startsWith(`src${path.sep}`);
  }
});
