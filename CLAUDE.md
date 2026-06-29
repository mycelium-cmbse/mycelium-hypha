# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

Two things in one repository, scoped to the **OMG SysML v2 / KerML** standards (not UML):

1. The **`hypha` Claude plugin** — `.claude-plugin/`, `skills/`, `agents/`, `commands/`, `hooks/`, and
   the `knowledge/` base the plugin reads at runtime.
2. The **generation pipelines** in `tools/` that build `knowledge/` from upstream OMG sources in
   `sources/`. The pipelines are not shipped with the plugin.

## Build & test

**.NET** (`tools/metamodel-gen`, `net10.0`, NUnit), from repo root:

```sh
dotnet build mycelium-hypha.sln
dotnet test mycelium-hypha.sln
dotnet test mycelium-hypha.sln --filter "FullyQualifiedName~MetamodelClosureTests"   # one fixture/test
```

**Python** (`tools/spec-extract`, Python ≥ 3.12), from `tools/spec-extract/`:

```sh
python -m venv .venv && . .venv/Scripts/activate   # or source .venv/bin/activate
pip install -e .[dev]
ruff check .
pytest
pytest tests/test_clauses.py::test_detects_clauses_and_groups_body   # one test (or: pytest -k <expr>)
```

## Generation is test-driven (no CLI)

There is no command-line entry point — **the unit tests are the generators**, and running them
(re)writes the knowledge base:

- `metamodel-gen` tests write `knowledge/sysml2/` from `sources/xmi/*.uml`. The **committed files are the
  golden**; after an intended format change, run the `[Explicit]` `Bless_*` tests to regenerate them.
- `spec-extract`'s `test_generate.py` writes `knowledge/spec/` from `sources/specs/*.pdf`.
- Tests **skip** (never fail) when their inputs are absent (no XMI / no PDFs), so CI stays green without
  the copyrighted/optional inputs.
- "Interesting" metaclasses are discovered via `ModelInspector`, never hardcoded.

## Committed vs git-ignored (and why)

- **Committed:** `knowledge/sysml2/`, `knowledge/textual-notation/`; `sources/xmi/` and
  `sources/textual/` (both EPL-2.0).
- **Git-ignored — never commit:** `sources/specs/*.pdf` and the generated `knowledge/spec/`. These are
  **full verbatim OMG specification text**; the OMG license forbids redistributing it, so it is
  regenerated locally only. A SessionStart hook (`hooks/check-spec-pdfs.py`) tells the user when the
  PDFs are missing.
- Exact upstream sources, commits and licenses are recorded in `sources/README.md` and `NOTICE`.
  (Metamodel XMI ← `SysML-v2-Pilot-Implementation`; PDFs + textual sources ← `SysML-v2-Release`.)

## Determinism of committed artifacts

Anything generated and committed must be byte-stable: sort collections with `StringComparer.Ordinal`,
emit LF (`.gitattributes` pins `knowledge/** eol=lf`), and include no timestamps, machine paths, or
culture-sensitive formatting. JSON uses `System.Text.Json` with fixed options and `\r\n`→`\n`
normalization. Golden tests compare against the committed files, so non-deterministic output breaks CI.

## Knowledge-base shape (what the skills/agents consume)

- `knowledge/sysml2/elements/<Name>.md` — one file per metaclass / enumeration / primitive: front
  matter, Generalizations/Specializations, **Owned features**, an **Inherited features** table giving
  the *full effective* feature set (read it directly — do not re-walk the generalization chain), and
  Constraints (OCL). `index.md` + `index.json` resolve a name → element.
- `knowledge/spec/{kerml,sysml2}/<clause>.md` — verbatim clause text + front matter
  (`clause`, `title`, `document`, `version`, `pages`, `normative`), plus `index.md` / `index.json`.
- `knowledge/textual-notation/` — grammar summary + keyword reference (`index.md`), worked `examples/`
  (each ` ```sysml ` block is a **byte-exact copy** of a `sources/textual/` model), and validator
  `fixtures/` (valid by reference + synthetic invalid snippets with expected findings).

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
- **Python:** ruff-clean, standard-library-friendly; each file carries the
  `# Copyright … / # SPDX-License-Identifier: Apache-2.0` header.
- **Prose/docs** use a spaced en dash (` – `), not an em dash.
