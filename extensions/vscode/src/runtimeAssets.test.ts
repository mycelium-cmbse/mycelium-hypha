import assert from "node:assert/strict";
import { access, mkdtemp, mkdir, readFile, rm, writeFile } from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import test from "node:test";

import { provisionRuntimeAssets } from "./runtimeAssets";

const extensionPath = path.resolve(__dirname, "..");
const resources = path.join(__dirname, "resources");

test("packages the shared primitives input byte-for-byte with license notices", async () => {
  const expected = await readFile(path.resolve(extensionPath, "../../sources/PrimitiveTypes.xmi"));
  assert.deepEqual(await readFile(path.join(resources, "sources", "PrimitiveTypes.xmi")), expected);
  await access(path.join(resources, "LICENSE"));
  await access(path.join(resources, "NOTICE"));
});

test("provisions generation inputs into an independent global data root and supports retries", async () => {
  const storage = await mkdtemp(path.join(os.tmpdir(), "hypha-runtime-assets-"));
  try {
    const releaseSources = path.join(storage, "sources", "2026-09", "xmi");
    await mkdir(releaseSources, { recursive: true });
    await writeFile(path.join(releaseSources, "existing.uml"), "existing downloaded input");
    await provisionRuntimeAssets(extensionPath, storage);
    await provisionRuntimeAssets(extensionPath, storage);

    assert.deepEqual(
      await readFile(path.join(storage, "sources", "PrimitiveTypes.xmi")),
      await readFile(path.join(resources, "sources", "PrimitiveTypes.xmi"))
    );
    await access(path.join(storage, "tools", "spec-extract", "pyproject.toml"));
    await access(path.join(storage, "tools", "spec-extract", "src", "spec_extract", "__main__.py"));
    assert.equal(await readFile(path.join(releaseSources, "existing.uml"), "utf8"), "existing downloaded input");
  } finally {
    await rm(storage, { recursive: true, force: true });
  }
});

test("rejects a package missing the shared input before modifying release storage", async () => {
  const temporary = await mkdtemp(path.join(os.tmpdir(), "hypha-incomplete-package-"));
  try {
    const fakeExtension = path.join(temporary, "extension");
    const extractor = path.join(fakeExtension, "out", "resources", "spec-extract");
    await mkdir(path.join(extractor, "src", "spec_extract"), { recursive: true });
    await writeFile(path.join(extractor, "pyproject.toml"), "");
    const storage = path.join(temporary, "storage");

    await assert.rejects(provisionRuntimeAssets(fakeExtension, storage), /missing required generation runtime assets/);
    await assert.rejects(access(storage), { code: "ENOENT" });
  } finally {
    await rm(temporary, { recursive: true, force: true });
  }
});
