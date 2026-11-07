import assert from "node:assert/strict";
import { access } from "node:fs/promises";
import path from "node:path";
import test from "node:test";

const packagedExtractor = path.join(__dirname, "resources", "spec-extract");

test("stages the specification extractor runtime without its test suite", async () => {
  await access(path.join(packagedExtractor, "pyproject.toml"));
  await access(path.join(packagedExtractor, "uv.lock"));
  await access(path.join(packagedExtractor, "src", "spec_extract", "__main__.py"));
  await assert.rejects(access(path.join(packagedExtractor, "tests")));
});
