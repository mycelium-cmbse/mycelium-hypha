import assert from "node:assert/strict";
import { mkdir, mkdtemp, rm, symlink, writeFile } from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import test from "node:test";

import { lookupKnowledge } from "./knowledge";

async function fixture(run: (root: string) => Promise<void>): Promise<void> {
  const root = await mkdtemp(path.join(os.tmpdir(), "hypha-lookup-"));
  try {
    await mkdir(path.join(root, "knowledge/2026-09/metamodel/elements"), { recursive: true });
    await mkdir(path.join(root, "knowledge/2026-09/model-library/packages"), { recursive: true });
    await writeFile(path.join(root, "knowledge/versions.json"), JSON.stringify({
      default: "2026-09", versions: [{ tag: "2026-09" }]
    }));
    await writeFile(path.join(root, "knowledge/2026-09/metamodel/index.json"), JSON.stringify({
      entries: {
        PartUsage: { file: "elements/PartUsage.md" },
        PartDefinition: { file: "elements/PartDefinition.md" }
      }
    }));
    await writeFile(path.join(root, "knowledge/2026-09/metamodel/elements/PartUsage.md"),
      Array.from({ length: 170 }, (_, i) => `source line ${i + 1}`).join("\n"));
    await writeFile(path.join(root, "knowledge/2026-09/model-library/index.json"), JSON.stringify({
      declarations: { "ScalarValues::Real": { file: "packages/scalar.md" } }
    }));
    await writeFile(path.join(root, "knowledge/2026-09/model-library/packages/scalar.md"), "# ScalarValues");
    await run(root);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
}

test("reads exact metamodel sources from global storage with citation and bounded pagination", async () => {
  await fixture(async root => {
    const result = JSON.parse(await lookupKnowledge(root, { kind: "metamodel", name: "PartUsage" }));
    assert.equal(result.release, "2026-09");
    assert.equal(result.endLine, 160);
    assert.equal(result.nextLine, 161);
    assert.match(result.source, /PartUsage.md$/);
    const tail = JSON.parse(await lookupKnowledge(root, { kind: "metamodel", name: "PartUsage", startLine: 161 }));
    assert.equal(tail.nextLine, null);
    assert.equal(tail.content.split("\n").length, 10);
  });
});

test("looks up qualified standard library declarations", async () => {
  await fixture(async root => {
    const result = JSON.parse(await lookupKnowledge(root, { kind: "model-library", name: "ScalarValues::Real" }));
    assert.equal(result.content, "# ScalarValues");
    assert.equal(result.release, "2026-09");
  });
});

test("returns alternatives rather than arbitrarily selecting ambiguous names", async () => {
  await fixture(async root => {
    const result = JSON.parse(await lookupKnowledge(root, { kind: "metamodel", name: "Part" }));
    assert.deepEqual(result.matches, ["PartUsage", "PartDefinition"]);
    assert.equal(result.content, undefined);
  });
});

test("rejects unavailable releases, malformed pagination and missing artifacts", async () => {
  await fixture(async root => {
    await assert.rejects(lookupKnowledge(root, { kind: "metamodel", name: "PartUsage", tag: "2025-01" }), /not installed/);
    await assert.rejects(lookupKnowledge(root, { kind: "metamodel", name: "PartUsage", startLine: 0 }), /positive integer/);
    await rm(path.join(root, "knowledge/2026-09/metamodel/elements/PartUsage.md"));
    await assert.rejects(lookupKnowledge(root, { kind: "metamodel", name: "PartUsage" }), { code: "ENOENT" });
  });
});

test("rejects index paths and symbolic links outside the release knowledge directory", async () => {
  await fixture(async root => {
    const index = path.join(root, "knowledge/2026-09/metamodel/index.json");
    await writeFile(index, JSON.stringify({ entries: { Escape: { file: "../../../outside.md" } } }));
    await writeFile(path.join(root, "outside.md"), "not knowledge");
    await assert.rejects(lookupKnowledge(root, { kind: "metamodel", name: "Escape" }), /outside/);
    const link = path.join(root, "knowledge/2026-09/metamodel/elements/link.md");
    await symlink(path.join(root, "outside.md"), link);
    await writeFile(index, JSON.stringify({ entries: { Escape: { file: "elements/link.md" } } }));
    await assert.rejects(lookupKnowledge(root, { kind: "metamodel", name: "Escape" }), /outside/);
  });
});
