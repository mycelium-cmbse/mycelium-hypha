import { access, cp, mkdir } from "node:fs/promises";
import path from "node:path";

export async function provisionRuntimeAssets(
  extensionPath: string,
  storagePath: string
): Promise<void> {
  const resources = path.join(extensionPath, "out", "resources");
  const source = path.join(resources, "spec-extract");
  const destination = path.join(storagePath, "tools", "spec-extract");
  const primitiveTypes = path.join(resources, "sources", "PrimitiveTypes.xmi");

  try {
    await access(path.join(source, "pyproject.toml"));
    await access(path.join(source, "src", "spec_extract"));
    await access(primitiveTypes);
  } catch (error) {
    throw new Error("The extension package is missing required generation runtime assets (specification extractor or PrimitiveTypes.xmi). Rebuild or reinstall Hypha.", { cause: error });
  }

  await mkdir(path.join(storagePath, "sources"), { recursive: true });
  await cp(primitiveTypes, path.join(storagePath, "sources", "PrimitiveTypes.xmi"), { force: true });
  await mkdir(path.dirname(destination), { recursive: true });
  await cp(source, destination, { recursive: true, force: true });
}
