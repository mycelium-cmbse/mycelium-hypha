# mycelium-hypha

Hypha is the AI agent of the Mycelium ecosystem, a connecting filament between engineers and the SysML v2 specification. Packaged as a Claude plugin with skills and subagents for metamodel lookup, spec citation, and SysML v2 validation.

## What's in here

mycelium-hypha is two things in one repository:

1. **A Claude plugin** (`hypha`) — the agent itself: skills + subagents that an engineer
   installs to get authoritative answers about KerML and SysML v2.
2. **The generation pipelines** that build the plugin's knowledge base from the
   normative OMG sources (XMI metamodel + PDF specifications).

```
mycelium-hypha/
├── .claude-plugin/         Plugin + marketplace manifests
│   ├── plugin.json
│   └── marketplace.json
│
├── skills/                 Skills (user- and model-invocable workflows)
│   ├── metamodel-lookup/
│   ├── spec-citation/
│   └── sysml-validation/
│
├── agents/                 Subagents (focused, isolated workers)
│   ├── metamodel-navigator.md
│   ├── spec-citation.md
│   └── sysml-validator.md
│
├── knowledge/              GENERATED knowledge base the plugin reads (committed)
│   ├── sysml2/             Combined KerML + SysML v2 metamodel — one markdown file per element
│   ├── spec/               Normative spec excerpts extracted from the PDFs, by clause
│   └── textual-notation/   SysML v2 textual-notation reference & examples
│
├── sources/                RAW inputs (see sources/README.md)
│   ├── xmi/                OMG / pilot XMI metamodel (EPL-2.0, committed)
│   ├── specs/              OMG PDF specifications (copyrighted — not committed)
│   └── textual/            Textual-notation grammar + examples (EPL-2.0, committed)
│
└── tools/                  Generation pipelines (NOT part of the shipped plugin)
    ├── metamodel-gen/      C# (uml4net): XMI  → knowledge/sysml2/elements
    └── spec-extract/       Python:       PDFs → knowledge/spec
```

## The pipeline

```
sources/xmi/*.uml             ──[ tools/metamodel-gen (uml4net) ]──▶ knowledge/sysml2/
sources/specs/*.pdf           ──[ tools/spec-extract (Python)   ]──▶ knowledge/spec/
sources/textual/, bnf/        ──[ curated / scripted            ]──▶ knowledge/textual-notation/
```

`knowledge/` is committed so the plugin works without running the pipelines. Regenerate
it whenever the upstream OMG sources change.

## Sources

The normative inputs come from the OMG SysML v2 submission team and Starion Group tooling:

- [Systems-Modeling/SysML-v2-Release](https://github.com/Systems-Modeling/SysML-v2-Release) — PDF specs (`doc/`), normative XMI libraries (`sysml.library.xmi/`), grammar (`bnf/`), and textual examples.
- [STARIONGROUP/uml4net](https://github.com/STARIONGROUP/uml4net) — C# library to read XMI models.
- [STARIONGROUP/SysML2.NET](https://github.com/STARIONGROUP/SysML2.NET) — .NET implementation of the OMG SysML v2 specification.

## License

This repository's own code and content are licensed under **Apache-2.0** — see
[LICENSE](LICENSE) and [NOTICE](NOTICE).

### Specifications & licensing

The generated knowledge base is built from third-party, separately-licensed inputs:

- **Generated `knowledge/`** is a derivative "special purpose specification … based upon" the OMG
  specifications, used for informational purposes as permitted by the OMG specification license. It
  ships under this repository's Apache-2.0 license; the upstream OMG attributions and the OMG license
  text are reproduced in [NOTICE](NOTICE).
- **OMG specification PDFs are copyrighted and intentionally NOT committed** here — the OMG license
  forbids posting the specifications on a network. They stay git-ignored (`sources/specs/`). Obtain
  them from OMG:
  [KerML 1.0](https://www.omg.org/spec/KerML/1.0) (`formal/26-03-01`),
  [SysML 2.0](https://www.omg.org/spec/SysML/2.0) (`formal/26-03-02`),
  [Systems Modeling API & Services 1.0](https://www.omg.org/spec/SystemsModelingAPI/1.0)
  (`formal/26-03-04`).
- **Committed metamodel XMI** (`sources/xmi/*.uml`, `PrimitiveTypes.xmi`, model version `20250201`)
  comes from [Systems-Modeling/SysML-v2-Pilot-Implementation](https://github.com/Systems-Modeling/SysML-v2-Pilot-Implementation)
  under the **Eclipse Public License 2.0**.

See [sources/README.md](sources/README.md) for exact input provenance (upstream source + version).

## Code Quality

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)