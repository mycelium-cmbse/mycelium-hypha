# mycelium-hypha

Hypha is the AI agent of the Mycelium ecosystem, a connecting filament between engineers and the
SysML v2 specification. It is packaged as a Claude plugin with skills and subagents for KerML / SysML
v2 metamodel lookup, normative spec citation, and SysML v2 textual-notation validation.

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
│   ├── spec-citation/
│   └── sysml-validation/
│
├── agents/                 Subagents (metamodel-navigator, spec-citation, sysml-validator)
│
├── knowledge/              Knowledge base the plugin reads
│   ├── metamodel/          Combined KerML + SysML v2 metamodel: one file per element, + index.json
│   │                       and metamodel.json (the structural graph)       (committed)
│   ├── spec/               Per-clause specification text                  (generated locally, git-ignored)
│   ├── textual-notation/   Grammar summary, worked examples, fixtures      (committed)
│   └── cross-references.json  Element → clause id, grammar production, example   (committed)
│
├── sources/                Raw inputs (see sources/README.md)
│   ├── xmi/                Metamodel XMI                      (EPL-2.0, committed)
│   ├── specs/              OMG PDF specifications             (copyrighted, git-ignored)
│   └── textual/            Grammar + example models           (EPL-2.0, committed)
│
└── tools/                  Generation pipelines (not part of the shipped plugin)
    ├── metamodel-gen/      C# (uml4net): XMI → knowledge/metamodel/ (element files, index, JSON sidecar)
    └── spec-extract/       Python:       PDFs → knowledge/spec/
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

The metamodel ships as a structural graph (`knowledge/metamodel/metamodel.json`, ~8 MB) with the
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

## The knowledge base

| Tree | Built from | Pipeline | Shipped |
| --- | --- | --- | --- |
| `knowledge/metamodel/` | `sources/xmi/*.uml` | `tools/metamodel-gen` (C# / uml4net) | committed |
| `knowledge/spec/` | `sources/specs/*.pdf` | `tools/spec-extract` (Python) | **git-ignored – regenerate locally** |
| `knowledge/textual-notation/` | `sources/textual/` (grammar + examples) | hand-curated | committed |
| `knowledge/cross-references.json` | the three trees above | `tools/spec-extract` (Python) | committed |

`knowledge/metamodel/` and `knowledge/textual-notation/` are committed, so the plugin works without
running any pipeline. `knowledge/spec/` holds **verbatim OMG specification text** and is deliberately
**not committed** (the OMG license forbids redistributing it). To enable spec citation, obtain the
three PDFs and regenerate it locally with `tools/spec-extract`; a SessionStart hook
(`hooks/check-spec-pdfs.py`) reminds you when the PDFs are missing.

`cross-references.json` links each metamodel element to the clauses that treat it, its grammar
production and any worked example. It records clause **identifiers only, never clause text**, so it
ships even though `knowledge/spec/` cannot – which is what lets spec citation still name the
governing clause when the PDFs are absent, instead of refusing outright. Regenerating it does need
the PDFs, so it is committed and only rebuilt when the specification version changes.

### Provenance

Facts carry one of three tiers, so a reader can tell what was read from what was inferred:
`NORMATIVE` (verbatim clause-anchored spec text), `MODEL` (read from the metamodel XMI) and
`DERIVED` (computed here – closures, name-matched cross-references). The tier definitions travel
with the data, in the `provenanceTiers` block of `cross-references.json`.

## Sources

- [Systems-Modeling/SysML-v2-Pilot-Implementation](https://github.com/Systems-Modeling/SysML-v2-Pilot-Implementation) – the metamodel XMI (`sources/xmi/`, EPL-2.0).
- [Systems-Modeling/SysML-v2-Release](https://github.com/Systems-Modeling/SysML-v2-Release) – the specification PDFs (`doc/`) and the textual-notation grammar (`bnf/`) and example models.
- [STARIONGROUP/uml4net](https://github.com/STARIONGROUP/uml4net) – C# library to read XMI models.
- [STARIONGROUP/SysML2.NET](https://github.com/STARIONGROUP/SysML2.NET) – .NET implementation of the OMG SysML v2 specification, used as a reference for the generator.

Exact upstream commits and versions are recorded in [sources/README.md](sources/README.md).

## License

This repository's own code and content are licensed under **Apache-2.0** – see [LICENSE](LICENSE) and
[NOTICE](NOTICE).

### Specifications & licensing

The knowledge base is built from third-party, separately-licensed inputs:

- The **committed** knowledge (`knowledge/metamodel/`, `knowledge/textual-notation/`) is a derivative
  "special purpose specification … based upon" the OMG specifications, used for informational purposes
  as permitted by the OMG specification license; it ships under this repository's Apache-2.0 license.
  The upstream OMG attributions and the OMG license text are reproduced in [NOTICE](NOTICE).
- **OMG specification PDFs are copyrighted and intentionally not committed** – the OMG license forbids
  posting the specifications on a network, so they stay git-ignored (`sources/specs/`), and the
  spec-derived `knowledge/spec/` is git-ignored too. Obtain the PDFs from OMG:
  [KerML 1.0](https://www.omg.org/spec/KerML/1.0) (`formal/26-03-01`),
  [SysML 2.0](https://www.omg.org/spec/SysML/2.0) (`formal/26-03-02`),
  [Systems Modeling API & Services 1.0](https://www.omg.org/spec/SystemsModelingAPI/1.0) (`formal/26-03-04`).
- The **committed metamodel XMI** (`sources/xmi/`, model version `20250201`) and the **textual-notation
  sources** (`sources/textual/`) come from the SysML v2 submission team's repositories under the
  **Eclipse Public License 2.0** (see [NOTICE](NOTICE) and [sources/README.md](sources/README.md)).

## Contributing

See [CONTRIBUTING](.github/CONTRIBUTING.md). The generation tools are exercised by their tests:
`dotnet test mycelium-hypha.sln` (metamodel-gen) and `pytest` in `tools/spec-extract` (which also
write the knowledge base). CI runs both with SonarQube analysis.

## Code quality

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)

