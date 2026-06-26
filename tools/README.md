# tools/

Generation pipelines that build `knowledge/` from `sources/`. These are **not** part of the
shipped Claude plugin — they are developer tooling, run when the upstream OMG sources change.

```
tools/
├── metamodel-gen/   C# (uml4net): reads XMI → writes knowledge/sysml2/elements + index.md
└── spec-extract/    Python:       reads PDFs → writes knowledge/spec/{kerml,sysml2} by clause
```

| Pipeline | Language | Input | Output |
| --- | --- | --- | --- |
| `metamodel-gen` | C# / [uml4net](https://github.com/STARIONGROUP/uml4net) | `sources/xmi/*.uml` | `knowledge/sysml2/` |
| `spec-extract` | Python | `sources/specs/*.pdf` | `knowledge/spec/` |

Each tool has its own README with build/run instructions.
