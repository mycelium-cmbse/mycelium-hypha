# Copilot instructions

Scope is the **OMG SysML v2 / KerML** standards (not UML). The repo has three parts: the `hypha` Claude plugin (`.claude-plugin/`, `skills/`, `commands/`, `hooks/`, `bin/`, `knowledge/`), the VS Code extension (`extensions/vscode/`), and the generation pipelines in `tools/` that build `knowledge/` from upstream OMG sources in `sources/`.

## Build, test, lint

.NET 10 (`tools/metamodel-gen`, `tools/knowledge-gen`, `tools/hypha-cli`; NUnit), from the repo root:

```sh
dotnet build mycelium-hypha.sln
dotnet test mycelium-hypha.sln
dotnet test mycelium-hypha.sln --filter "FullyQualifiedName~MetamodelClosureTests"   # one fixture/test
dotnet test mycelium-hypha.sln --filter "TestCategory=Live"   # [Explicit], hits GitHub; set GITHUB_TOKEN
```

Python >= 3.12 (`tools/spec-extract`), from that folder:

```sh
uv sync --extra dev
uv run --extra dev ruff check .
uv run --extra dev pytest
uv run --extra dev pytest tests/test_clauses.py::test_detects_clauses_and_groups_body   # one test, or -k <expr>
```

VS Code extension (`extensions/vscode/`): `npm test` (clean, tsc, copy assets, `node --test out/*.test.js`), `npm run compile`, `npm run package`. See its `DEVELOPMENT.md`.

## Architecture

- **Two knowledge libraries, one namespace root.** `Hypha.Knowledge` (`tools/knowledge-gen`) holds the layout, manifests and contracts, lookup, and discovery, fetching, generation and install / use / remove; `Hypha.Knowledge.Metamodel` (`tools/metamodel-gen`) generates the metamodel from XMI and references `Hypha.Knowledge`, never the reverse, so uml4net and Handlebars stay out of it. Generators and the code that reads their output share the same types, serialized through a source-generated `JsonSerializerContext` beside them, never a reflection-based `JsonSerializer` call.
- **The CLI generates, tests verify.** Generation is driven by `tools/hypha-cli` (`dotnet run --project tools/hypha-cli/Hypha.Tools -- generate [artifact] --tag <release>`, also `fetch`, `move-window`). No test may write into `knowledge/`; a test that dirties the working tree is a bug.
- **Orchestration lives in the library.** Each artifact is an `IKnowledgeGenerator` (`Artifact`, `Order`, `GenerateAsync(tag)`) registered as a collection; the CLI loops over `Order` (metamodel 10, grammar references 20, textual notation 30, model library 35, cross-references 40) and knows no artifact names, so adding a generator adds its verb. Generators whose inputs are absent return `Skipped`; inputs present but yielding wrong output must throw.
- **Systems Modeling API is a second version axis**, parallel to releases: `sources/api/<version>/`, `knowledge/api/<version>/`, its own manifest, `IApiKnowledgeGenerator` registered as a **separate collection** (`pim`, `openapi`, `spec`, `cross-references`), and `hypha api ...` commands. Registering a generator under the wrong interface runs it against the wrong kind of tag.
- **Nothing per-release is committed.** `sources/<tag>/`, `knowledge/<tag>/` and `knowledge/versions.json` are git-ignored (root-anchored `/sources/*/`, `/knowledge/*/`) and built on the user's machine. Committed: `knowledge/README.md`, the `*.schema.json` files, `sources/PrimitiveTypes.xmi`, `hooks/native/<rid>/hypha-hook(.exe)`, and the XMI test fixture under `tools/metamodel-gen/Hypha.Knowledge.Metamodel.Tests/Fixtures/`.
- **Never commit specification text**: `sources/<tag>/specs/*.pdf` and `knowledge/<tag>/spec/` are verbatim OMG text that the licence forbids redistributing. `cross-references.json` carries identifiers only, never clause text, and each edge records its provenance `method` (prefer stated links such as `mapping-table` over `same-name`).
- **spec-extract** is a layered pure-function pipeline: `pdf_reader` -> `layout` -> `clauses` -> `normative` -> `markdown` -> `pipeline`; each layer is unit-tested on synthetic data.
- "Interesting" metaclasses are discovered via `ModelInspector`, never hardcoded.

## Conventions

- **Determinism:** generated output must be byte-stable: `StringComparer.Ordinal` sorting, LF endings, no timestamps, machine paths or culture-sensitive formatting; JSON via `System.Text.Json` with fixed options. `KnowledgeRegenerationTests` checks two independent runs match byte for byte; `Hypha.Knowledge.Metamodel.Tests` `Expected/` golden files are re-blessed with the `[Explicit]` `Bless_expected_files` tests, and after an intended format change you review the diff by hand.
- **C#:** block-scoped namespaces with `using`s inside the namespace, one public type per file, classic `.sln`, Apache-2.0 `<copyright>` header (Starion Group S.A.) on every file. Services are registered with `Microsoft.Extensions.DependencyInjection` and resolved, not constructed; keep constructors injectable with no hidden statics. Caller-variable settings go on `HyphaKnowledgeOptions`, not extra `AddHyphaKnowledge` parameters; use `ILogger`, not `Console`. Prefer maintained NuGet packages over hand-rolled infrastructure.
- **Python:** ruff-clean, standard-library-friendly, each file has the `# Copyright ... / # SPDX-License-Identifier: Apache-2.0` header.
- **Prose, comments, XML docs, CHANGELOG, issues, PRs and commit messages use no en or em dashes** as punctuation; use a comma, colon, parentheses, semicolon or full stop. Hyphens in compound words are fine.
- **Git:** never work on `development` or `main`. Branch `GH<issue-number>` off `development`, PR back into `development`, rebase rather than merge. Ask before committing or pushing. Commit subject: `[Verb] <summary>; fixes #<n>`.

## Knowledge-base shape (what the skills consume)

- `knowledge/<tag>/metamodel/elements/<Name>.md`: one file per metaclass, with an **Inherited features** table giving the full effective feature set (do not re-walk generalizations); `index.md`/`index.json` resolve names.
- `knowledge/<tag>/spec/{kerml,sysml2}/<clause>.md`: verbatim clause text with front matter, plus an index.
- `knowledge/<tag>/textual-notation/`: `grammar-{kerml,sysml,graphical}.md` (productions grouped by clause, with the metaclass built and features populated), `index.md` keyword reference, and `examples/` that are byte-exact copies of `sources/<tag>/textual/` models.
