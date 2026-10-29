import assert from "node:assert/strict";
import test from "node:test";

import { parseReleaseManifest } from "./manifest";

test("parses installed release tags and their default", () => {
  assert.deepEqual(
    parseReleaseManifest('{"default":"2026-09","versions":[{"tag":"2026-09"},{"tag":"2026-05"}]}'),
    { defaultTag: "2026-09", tags: ["2026-09", "2026-05"] }
  );
});

test("parses an empty installation", () => {
  assert.deepEqual(
    parseReleaseManifest('{"default":null,"versions":[]}'),
    { defaultTag: null, tags: [] }
  );
});

test("rejects a default that is not installed", () => {
  assert.throws(
    () => parseReleaseManifest('{"default":"2026-09","versions":[]}'),
    /must identify an installed release/
  );
});

test("rejects malformed release entries", () => {
  assert.throws(
    () => parseReleaseManifest('{"default":null,"versions":[{}]}'),
    /non-empty tag/
  );
});

test("rejects malformed JSON", () => {
  assert.throws(
    () => parseReleaseManifest("{"),
    /not valid JSON/
  );
});
