import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import path from "node:path";
import test from "node:test";
import vm from "node:vm";

function createExtensionHarness() {
  const tools = new Map<string, { invoke: (options: unknown, token: unknown) => Promise<any> }>();
  const outputLines: string[] = [];
  const calls: string[][] = [];
  let lookupResult: Record<string, unknown> | Error = {
    release: "2026-09",
    kind: "metamodel",
    status: "found",
    name: "PartUsage",
    path: "/storage/knowledge/2026-09/metamodel/elements/PartUsage.md",
    declarationLine: null,
    startLine: 1,
    endLine: 2,
    totalLines: 2,
    nextLine: null,
    content: "PartUsage",
    matches: [],
    totalMatches: 1
  };
  const listResult = { defaultTag: null, releases: [] };
  const vscode = {
    lm: {
      registerTool(name: string, tool: { invoke: (options: unknown, token: unknown) => Promise<any> }) {
        tools.set(name, tool);
        return { dispose() {} };
      }
    },
    commands: {
      registerCommand() { return { dispose() {} }; }
    },
    window: {
      createOutputChannel() {
        return {
          append(value: string) { outputLines.push(value); },
          appendLine(value: string) { outputLines.push(value); },
          show() {}
        };
      }
    },
    Uri: {
      file(value: string) { return { toString: () => `file://${value}` }; }
    },
    LanguageModelToolResult: class {
      constructor(public content: unknown[]) {}
    },
    LanguageModelTextPart: class {
      constructor(public value: string) {}
    },
    CancellationError: class extends Error {}
  };
  const cli = {
    async ensureCli() { return "/extension/hypha"; },
    async runCliJson(_executable: string, _storage: string, args: string[]) {
      calls.push([...args]);
      if (args[0] === "lookup") {
        if (typeof lookupResult === "object" && lookupResult !== null && "message" in lookupResult) {
          throw lookupResult;
        }
        return lookupResult;
      }
      return listResult;
    }
  };
  const exports: Record<string, any> = {};
  const context = {
    exports,
    require(name: string) {
      if (name === "vscode") { return vscode; }
      if (name === "./cli") { return cli; }
      if (name === "./releaseCommands") { return { registerReleaseCommands: () => [] }; }
      throw new Error(`Unexpected import: ${name}`);
    }
  };
  vm.runInNewContext(require("node:fs").readFileSync(path.join(__dirname, "extension.js"), "utf8"), context);
  exports.activate({
    subscriptions: [],
    extensionUri: { fsPath: "/extension" },
    globalStorageUri: { fsPath: "/storage", toString: () => "file:///storage" }
  });

  return {
    calls,
    outputLines,
    tools,
    setLookupResult(value: Record<string, unknown> | Error) { lookupResult = value; },
    setLookupError(message: string) { lookupResult = vm.runInNewContext(`new Error(${JSON.stringify(message)})`, context); }
  };
}

test("lookup builds optional arguments and puts JSON last", async () => {
  const harness = createExtensionHarness();
  const tool = harness.tools.get("mycelium-hypha_lookup_knowledge")!;
  const token = { isCancellationRequested: false };

  await tool.invoke({
    input: { kind: "metamodel", name: "PartUsage", tag: "2026-09", startLine: 5 }
  }, token);
  assert.deepEqual(harness.calls[0], [
    "lookup", "--kind", "metamodel", "--name", "PartUsage",
    "--tag", "2026-09", "--start-line", "5", "--json"
  ]);

  await tool.invoke({ input: { kind: "metamodel", name: "PartUsage" } }, token);
  assert.deepEqual(harness.calls[1], ["lookup", "--kind", "metamodel", "--name", "PartUsage", "--json"]);
});

test("lookup converts source paths to URIs and omits sources for ambiguous or missing results", async () => {
  const harness = createExtensionHarness();
  const tool = harness.tools.get("mycelium-hypha_lookup_knowledge")!;
  const token = { isCancellationRequested: false };

  const found = await tool.invoke({ input: { kind: "metamodel", name: "PartUsage" } }, token);
  assert.equal(JSON.parse(found.content[0].value).source, "file:///storage/knowledge/2026-09/metamodel/elements/PartUsage.md");

  for (const status of ["ambiguous", "not-found"]) {
    harness.setLookupResult({ status, path: null, matches: [] });
    const result = await tool.invoke({ input: { kind: "metamodel", name: "PartUsage" } }, token);
    assert.equal(JSON.parse(result.content[0].value).source, null);
  }
});

test("lookup wraps CLI errors with actionable context and release status handles an empty manifest", async () => {
  const harness = createExtensionHarness();
  const lookup = harness.tools.get("mycelium-hypha_lookup_knowledge")!;
  harness.setLookupError("No release is installed.");
  await assert.rejects(
    lookup.invoke({ input: { kind: "metamodel", name: "PartUsage" } }, { isCancellationRequested: false }),
    /Hypha lookup failed: No release is installed/
  );
  assert.ok(harness.outputLines.some((line) => line.includes("Knowledge lookup failed")));

  const status = await harness.tools.get("mycelium-hypha_get_release_status")!.invoke(
    { input: {} },
    { isCancellationRequested: false }
  );
  assert.match(status.content[0].value, /No Hypha releases are recorded as installed/);
});
