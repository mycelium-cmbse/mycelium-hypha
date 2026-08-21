# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

Two things in one repository, scoped to the **OMG SysML v2 / KerML** standards (not UML):

1. The **`hypha` Claude plugin** — `.claude-plugin/`, `skills/`, `agents/`, `commands/`, `hooks/`, and
   the `knowledge/` base the plugin reads at runtime.
2. The **generation pipelines** in `tools/` that build `knowledge/` from upstream OMG sources in
   `sources/`. The pipelines are not shipped with the plugin.

## Build & test

**.NET** (`tools/metamodel-gen`, `tools/knowledge-gen` and `tools/hypha-cli`, `net10.0`, NUnit), from
repo root:

```sh
dotnet build mycelium-hypha.sln
dotnet test mycelium-hypha.sln
dotnet test mycelium-hypha.sln --filter "FullyQualifiedName~MetamodelClosureTests"   # one fixture/test
dotnet test mycelium-hypha.sln --filter "TestCategory=Live"   # [Explicit]: really fetches from GitHub
```

Nothing reaches the network in a normal run. The `Live` category is `[Explicit]`, so it is opt-in and
never in CI; set `GITHUB_TOKEN` before running it (the anonymous API allows 60 requests an hour).

**Python** (`tools/spec-extract`, Python ≥ 3.12), from `tools/spec-extract/`:

```sh
python -m venv .venv && . .venv/Scripts/activate   # or source .venv/bin/activate
pip install -e .[dev]
ruff check .
pytest
pytest tests/test_clauses.py::test_detects_clauses_and_groups_body   # one test (or: pytest -k <expr>)
```

## The CLI generates, the tests verify

Generation is driven by **`tools/hypha-cli`** (the `hypha` command), never by the test runner. No
test writes into `knowledge/` — running `dotnet test` leaves the working tree clean, and a test that
dirties it is a bug.

```sh
dotnet run --project tools/hypha-cli/Hypha.Tools -- generate            # all artifacts, all releases
dotnet run --project tools/hypha-cli/Hypha.Tools -- generate metamodel --tag 2026-05
dotnet run --project tools/hypha-cli/Hypha.Tools -- fetch --tag 2026-05 # sources/<tag>/
```

**The orchestration lives in the library, not in the fixtures.** Each artifact is an
`IKnowledgeGenerator` (`Artifact`, `Order`, `GenerateAsync(tag)`), registered as a collection, so a
full run is a loop over `Order` — metamodel (10), grammar references (20), textual notation (30),
model library (35), cross-references (40, last, because it reads the index and the generated
examples). The CLI resolves that collection; it does not know the artifact names, so adding a
generator adds its verb.

`hypha move-window --tag <release>` is the one verb that deliberately crosses the CLI/test split
above: it drives fetch → generate → the rest of this section's steps, then shells out to `dotnet
test`/`pytest` as subordinate steps (re-blessing the metamodel-gen fixtures, then a final byte-
identical regeneration check) before evicting whatever falls outside the rolling window. It is the
one entry point for moving hypha to a newer release; see `tools/hypha-cli/README.md`.

Inputs are read from `RepositoryRoot`, output is written to `OutputRoot` (`--output`), which defaults
to the same folder. That separation is what lets `KnowledgeRegenerationTests` regenerate everything
into a scratch folder and compare it **byte for byte** against the committed files: the **committed
files are the golden**, and proving they are still reproducible no longer means rewriting them.
After an intended format change, regenerate with `hypha generate`, review the diff, and run the
`[Explicit]` `Bless_*` tests for the metamodel expectations.

- `spec-extract`'s `test_generate.py` writes the git-ignored `knowledge/<tag>/spec/` from
  `sources/<tag>/specs/*.pdf`. That is now all Python does here, and it is the one place the old
  "tests write it" convention survives — because that output is never committed.
- A generator whose inputs are absent returns `Skipped` with a reason, so CI stays green without the
  copyrighted/optional inputs. Inputs that are present but would yield **wrong** output throw instead
  — a missing metamodel index, a clause title reaching the cross-references, a slug collision.
- "Interesting" metaclasses are discovered via `ModelInspector`, never hardcoded.

## Committed vs git-ignored (and why)

- **Committed:** `knowledge/metamodel/`, `knowledge/textual-notation/`; `sources/xmi/` and
  `sources/textual/` (both EPL-2.0); `hooks/native/<rid>/hypha-hook(.exe)`, the small NativeAOT
  binary the plugin's `SessionStart` hook dispatches to (see `tools/hypha-cli/README.md`'s
  "Automatic sync" section) — the only compiled binary this repository commits, and deliberately
  *not* under `bin/`, which `.gitignore` otherwise excludes at any depth.
- **Git-ignored — never commit:** `sources/specs/*.pdf` and the generated `knowledge/spec/`. These are
  **full verbatim OMG specification text**; the OMG license forbids redistributing it, so it is
  regenerated locally only, and needs a maintainer source checkout even when the PDFs are present
  (`tools/spec-extract` is not shipped with the plugin). The plugin's `SessionStart` hook
  (`Hypha.Tools.Hook`, `hooks/native/<rid>/hypha-hook`) tells the user when the PDFs are missing for
  the default release.
- Exact upstream sources, commits and licenses are recorded in `sources/README.md` and `NOTICE`.
  (Metamodel XMI ← `SysML-v2-Pilot-Implementation`; PDFs + textual sources ← `SysML-v2-Release`.)

## Determinism of committed artifacts

Anything generated and committed must be byte-stable: sort collections with `StringComparer.Ordinal`,
emit LF (`.gitattributes` pins `knowledge/** eol=lf`), and include no timestamps, machine paths, or
culture-sensitive formatting. JSON uses `System.Text.Json` with fixed options and `\r\n`→`\n`
normalization. Golden tests compare against the committed files, so non-deterministic output breaks CI.

## Knowledge-base shape (what the skills/agents consume)

- `knowledge/metamodel/elements/<Name>.md` — one file per metaclass / enumeration / primitive: front
  matter, Generalizations/Specializations, **Owned features**, an **Inherited features** table giving
  the *full effective* feature set (read it directly — do not re-walk the generalization chain), and
  Constraints (OCL). `index.md` + `index.json` resolve a name → element.
- `knowledge/spec/{kerml,sysml2}/<clause>.md` — verbatim clause text + front matter
  (`clause`, `title`, `document`, `version`, `pages`, `normative`), plus `index.md` / `index.json`.
- `knowledge/<tag>/textual-notation/grammar-{kerml,sysml,graphical}.md` — every production grouped by
  specification clause, with the metaclass it builds, the clause it is defined in and the metamodel
  features it populates. All three are **stated by the grammar**, not matched on a name.
- `knowledge/<tag>/textual-notation/index.md` — the release's keyword reference (read from its own
  BNF) and a link to every example — and `examples/` — one page per model the release ships (309),
  each ` ```sysml ` / ` ```kerml ` block a **byte-exact copy** of a `sources/<tag>/textual/` model,
  with front matter naming its upstream path and the metaclasses it declares.

- `knowledge/<tag>/cross-references.json` (+ `knowledge/cross-references.schema.json`) — element →
  clause **identifier**, grammar production, the metamodel features its syntax populates, and worked
  examples. Every edge carries its provenance tier and the method that produced it. **Never contains
  clause text** — that is what lets it be committed while `knowledge/<tag>/spec/` cannot be.

## spec-extract pipeline (layered, char-based)

Each layer is a pure function, unit-tested on synthetic data; a skip-if-absent test exercises the real
PDF: `pdf_reader` (positioned chars) → `layout` (reading-ordered lines: multi-column ordering +
running header/footer stripping) → `clauses` (heading detection via a successor-numbering rule) →
`normative` (code / note / example blocks) → `markdown` → `pipeline`.

## Conventions

- **Git workflow:** never work directly on `development` or `main`. Branch `GH<issue-number>` off
  `development`, and open the PR back into `development` (see `.github/CONTRIBUTING.md`). Ask before
  committing or pushing. Commit subject: `[Verb] <summary>; fixes #<n>`.
- **C#:** block-scoped namespaces with `using`s *inside* the namespace, one public type per file, NUnit,
  the classic `mycelium-hypha.sln`. Every source file opens with the Apache-2.0 `<copyright>` header
  (Starion Group S.A.).
- **C# dependencies and DI:** well-established NuGet packages are welcome – prefer a maintained
  library over hand-rolling infrastructure. Services are registered through
  `Microsoft.Extensions.DependencyInjection` and resolved rather than constructed, so the CLI
  (see #81) composes them in one place. Keep constructors injectable and free of hidden statics.
  Anything a caller can vary belongs on `HyphaKnowledgeOptions`, not as another optional parameter on
  `AddHyphaKnowledge`; anything a caller might want to watch goes through `ILogger`, not `Console`.
- **Python:** ruff-clean, standard-library-friendly; each file carries the
  `# Copyright … / # SPDX-License-Identifier: Apache-2.0` header.
- **Prose/docs** use a spaced en dash (` – `), not an em dash.
