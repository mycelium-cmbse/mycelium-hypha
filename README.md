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
├── sources/                RAW inputs (git-ignored; see sources/README.md)
│   ├── xmi/                OMG / pilot XMI metamodel + normative libraries
│   ├── specs/              OMG PDF specifications (copyrighted — not committed)
│   └── textual/            Textual-notation source material
│
└── tools/                  Generation pipelines (NOT part of the shipped plugin)
    ├── metamodel-gen/      C# (uml4net): XMI  → knowledge/{kerml,sysml2}/elements
    └── spec-extract/       Python:       PDFs → knowledge/spec
```

## The pipeline

```
sources/xmi/*.kermlx,*.sysmlx ──[ tools/metamodel-gen (uml4net) ]──▶ knowledge/{kerml,sysml2}/
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

Apache-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).
