# mycelium-hypha

Hypha is the AI agent of the Mycelium ecosystem, a connecting filament between engineers and the
SysML v2 specification. It is packaged as a Claude plugin with skills and subagents for KerML / SysML
v2 metamodel lookup, standard model library lookup, normative spec citation, and SysML v2
textual-notation validation.

## What's in here

mycelium-hypha is two things in one repository:

1. **A Claude plugin** (`hypha`) – the skills and subagents an engineer installs to get grounded
   answers about KerML and SysML v2, backed by a committed knowledge base.
2. **The generation pipelines** that build that knowledge base from the upstream OMG sources (the
   metamodel XMI, the specification PDFs, and the textual-notation grammar and examples).

```
mycelium-hypha/
├── .claude-plugin/         Plugin + marketplace manifests (plugin.json, marketplace.json)
├── hooks/                  Plugin hooks (SessionStart check for the spec PDFs)
│
├── skills/                 Skills (user- and model-invocable workflows)
│   ├── metamodel-lookup/
│   ├── model-library-lookup/
│   ├── spec-citation/
│   └── sysml-validation/
│
├── agents/                 Subagents (metamodel-navigator, spec-citation, sysml-validator)
│
├── knowledge/              Knowledge base the plugin reads, one folder per release tag
│   ├── versions.json       Installed release tags + which is the default   (committed)

│   └── <tag>/              e.g. 2026-05
│       ├── textual-notation/  Keyword reference + every model shipped at the tag, verbatim
│       ├── model-library/  Standard model libraries (ISQ, ScalarValues, SysML.sysml, ...), verbatim,
│       │                   + a qualified-name → declaration index.json  (committed)
│       ├── metamodel/      Combined KerML + SysML v2 metamodel: one file per element, + index.json,
│       │                   metamodel.json (the structural graph) and
│       │                   diagrams/ (a Mermaid class diagram per package)  (committed)
│       ├── spec/           Per-clause specification text          (generated locally, git-ignored)
│       └── cross-references.json  Element → clause id, grammar production, example  (committed)
│
├── sources/                Raw inputs, one folder per release tag (see sources/README.md)
│   ├── PrimitiveTypes.xmi  OMG UML primitives library         (shared by every release, committed)
│   └── <tag>/
│       ├── xmi/            Metamodel XMI                      (EPL-2.0, committed)
│       ├── specs/          OMG PDF specifications             (copyrighted, git-ignored)
│       └── textual/        Grammar + example models + standard model libraries  (EPL-2.0, committed)
│
└── tools/                  Generation pipelines (not part of the shipped plugin)
    ├── knowledge-gen/      C#:           releases (discovery, fetching, versions.json); BNF + models -> textual-notation/; cross-references.json
    ├── metamodel-gen/      C# (uml4net): XMI → knowledge/<tag>/metamodel/ (elements, index, JSON sidecar, diagrams)
    └── spec-extract/       Python:       PDFs -> knowledge/<tag>/spec/ (git-ignored) — the only Python left
```

## Install

In Claude Code, add the marketplace and install the plugin:

```
/plugin marketplace add mycelium-cmbse/mycelium-hypha
/plugin install hypha@mycelium
```

### Recommended: `jq`

Install [`jq`](https://jqlang.github.io/jq/) – `brew install jq`, `sudo apt install jq`, or
`winget install jqlang.jq`.

The metamodel ships as a structural graph (`knowledge/<tag>/metamodel/metamodel.json`, ~8 MB) with the
inheritance closures precomputed. Set-shaped and cross-cutting questions – *"which metaclasses have a
feature typed by `Expression`"*, *"every concrete subclass of `Usage`"* – are one query against it,
and `jq` is how the skills run that query. It is a small standalone binary with no runtime behind it.

Hypha still works without `jq`: the skills fall back to reading the per-element markdown, which is
slower, pulls far more into context, and cannot distinguish a field match from a mention in prose. If
you use metamodel lookup or validation regularly, install it.

Then use the skills: ask a metamodel-lookup question, request a spec citation, or paste SysML v2
textual notation to validate. Metamodel lookup and validation work out of the box; **spec citation
needs the specification text generated locally first** (see below). For example:

- *Metamodel lookup* – "What features does `PartUsage` own and inherit?", "How does `ConnectionUsage`
  relate to `ConnectionDefinition`?", or "Which metaclasses specialize `Feature`?"
- *Spec citation* – "What does the SysML v2 spec say about conformance?" or "Quote the normative rule
  for redefinition."
- *Validation* – "Is this valid SysML v2? `part def Vehicle { attribute mass : Real[2..1]; }`"

## Releases

KerML and SysML v2 are released on a rolling tag cadence (`YYYY-MM`) across two upstream
repositories, and hypha generates its knowledge base **per release tag**. A hypha version is one tag
that exists in *both* upstreams — the metamodel XMI comes from the Pilot-Implementation repo, the
specs, grammar and models from the Release repo.

`knowledge/versions.json` records which releases this checkout carries and which one answers by
default. The repository ships a **rolling window of the two most recent releases**, maintained with
one command — `hypha move-window --tag <release>` (see
[tools/hypha-cli](tools/hypha-cli/README.md)) — that fetches, regenerates, verifies and evicts in
one step; any other release can be generated locally (see
[tools/spec-extract](tools/spec-extract/README.md)).

The **tag is the version identifier.** The model URI inside the XMI (`…/SysML/20250201`) is
deliberately ignored: it tracks neither the release nor the content — the 2026-05 metamodel still
declares a 2025 URI. A consequence, and an accepted one, is that the same model may appear under
several tags.

The skills state which release an answer came from, and answer from the default unless you name one.

## The knowledge base

| Tree | Built from | Pipeline | Shipped |
| --- | --- | --- | --- |
| `knowledge/<tag>/metamodel/` | `sources/<tag>/xmi/*.uml` | `tools/metamodel-gen` (C# / uml4net) | committed |
| `knowledge/<tag>/spec/` | `sources/<tag>/specs/*.pdf` | `tools/spec-extract` (Python) | **git-ignored – regenerate locally** |
| `knowledge/<tag>/textual-notation/` | `sources/<tag>/textual/` (grammar + models) | `tools/knowledge-gen` (C#) | committed |
| `knowledge/<tag>/model-library/` | `sources/<tag>/textual/sysml.library/` | `tools/knowledge-gen` (C#) | committed |
| `knowledge/<tag>/cross-references.json` | the trees above | `tools/knowledge-gen` (C#) | committed |

All of it is generated by one command – `hypha generate` (see
[tools/hypha-cli](tools/hypha-cli/README.md)), installable as a `dotnet tool` or as a self-contained
executable that needs no toolchain at all. **.NET alone produces everything that ships**; Python is
needed only for the git-ignored `knowledge/<tag>/spec/`.

`knowledge/<tag>/metamodel/` and `knowledge/<tag>/textual-notation/` are committed, so the plugin works without
running any pipeline. `knowledge/<tag>/spec/` holds **verbatim OMG specification text** and is deliberately
**not committed** (the OMG license forbids redistributing it). To enable spec citation, obtain the
three PDFs and regenerate it locally with `tools/spec-extract`; a SessionStart hook
(`hooks/check-spec-pdfs.py`) reminds you when the PDFs are missing.

`cross-references.json` links each metamodel element to the clauses that treat it, its grammar
production and any worked example. It records clause **identifiers only, never clause text**, so it
ships even though `knowledge/<tag>/spec/` cannot – which is what lets spec citation still name the
governing clause when the PDFs are absent, instead of refusing outright. Regenerating it does need
the PDFs, so it is committed and only rebuilt when the specification version changes.

### Provenance

Facts carry one of three tiers, so a reader can tell what was read from what was inferred:
`NORMATIVE` (verbatim clause-anchored spec text), `MODEL` (read from the metamodel XMI) and
`DERIVED` (computed here – closures, name-matched cross-references). The tier definitions travel
with the data, in the `provenanceTiers` block of `cross-references.json`.

## Sources

- [Systems-Modeling/SysML-v2-Pilot-Implementation](https://github.com/Systems-Modeling/SysML-v2-Pilot-Implementation) – the metamodel XMI (`sources/<tag>/xmi/`, EPL-2.0).
- [Systems-Modeling/SysML-v2-Release](https://github.com/Systems-Modeling/SysML-v2-Release) – the specification PDFs (`doc/`) and the textual-notation grammar (`bnf/`) and example models.
- [STARIONGROUP/uml4net](https://github.com/STARIONGROUP/uml4net) – C# library to read XMI models.
- [STARIONGROUP/SysML2.NET](https://github.com/STARIONGROUP/SysML2.NET) – .NET implementation of the OMG SysML v2 specification, used as a reference for the generator.

Exact upstream commits and versions are recorded in [sources/README.md](sources/README.md).

## License

This repository's own code and content are licensed under **Apache-2.0** – see [LICENSE](LICENSE) and
[NOTICE](NOTICE).

### Specifications & licensing

The knowledge base is built from third-party, separately-licensed inputs:

- The **committed** knowledge (`knowledge/<tag>/metamodel/`, `knowledge/<tag>/textual-notation/`) is a derivative
  "special purpose specification … based upon" the OMG specifications, used for informational purposes
  as permitted by the OMG specification license; it ships under this repository's Apache-2.0 license.
  The upstream OMG attributions and the OMG license text are reproduced in [NOTICE](NOTICE).
- **OMG specification PDFs are copyrighted and intentionally not committed** – the OMG license forbids
  posting the specifications on a network, so they stay git-ignored (`sources/<tag>/specs/`), and the
  spec-derived `knowledge/<tag>/spec/` is git-ignored too. Obtain the PDFs from OMG:
  [KerML 1.0](https://www.omg.org/spec/KerML/1.0) (`formal/26-03-01`),
  [SysML 2.0](https://www.omg.org/spec/SysML/2.0) (`formal/26-03-02`),
  [Systems Modeling API & Services 1.0](https://www.omg.org/spec/SystemsModelingAPI/1.0) (`formal/26-03-04`).
- The **committed metamodel XMI** (`sources/<tag>/xmi/`) and the **textual-notation
  sources** (`sources/<tag>/textual/`) come from the SysML v2 submission team's repositories under the
  **Eclipse Public License 2.0** (see [NOTICE](NOTICE) and [sources/README.md](sources/README.md)).

## Contributing

See [CONTRIBUTING](.github/CONTRIBUTING.md). `dotnet test mycelium-hypha.sln` and `pytest` in
`tools/spec-extract` exercise the generators; CI runs both with SonarQube analysis. The .NET tests
**verify** the knowledge base – one of them regenerates it into a scratch folder and compares it byte
for byte against the committed files – but never write it. Regenerating is `hypha generate`'s job
(see [tools/hypha-cli](tools/hypha-cli/README.md)), so a test run leaves the working tree clean.

## Code quality

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)

