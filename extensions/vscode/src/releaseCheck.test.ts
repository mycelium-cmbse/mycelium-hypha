import assert from "node:assert/strict";
import test from "node:test";

import { availableReleases } from "./releaseCheck";

test("returns only release tags available in both upstream repositories", () => {
  assert.deepEqual(
    availableReleases(
      ["2026-09", "2026-08-pre", "2026-07.2", "2026-06", "2026-07.1"],
      ["2026-09", "2026-07.2", "2026-07.1"]
    ),
    ["2026-09", "2026-07.2", "2026-07.1"]
  );
});

test("sorts numeric point releases after their base release", () => {
  assert.deepEqual(
    availableReleases(["2026-09", "2026-09.10", "2026-09.2"], ["2026-09", "2026-09.10", "2026-09.2"]),
    ["2026-09.10", "2026-09.2", "2026-09"]
  );
});
