# tools/

Generation pipelines that build `knowledge/` from `sources/`. These are **not** part of the
shipped Claude plugin — they are developer tooling, run when the upstream OMG sources change.

```
tools/
├── hypha-cli/       C#:     the `hypha` command — the entry point that drives everything below
├── knowledge-gen/   C#:     release discovery/fetching, grammar, textual notation, cross-references
├── metamodel-gen/   C# (uml4net): reads XMI → writes knowledge/<tag>/metamodel/
└── spec-extract/    Python: reads PDFs → writes knowledge/<tag>/spec/ (git-ignored)
```

| Artifact | Produced by | Shipped |
| --- | --- | --- |
| `knowledge/versions.json` | `Hypha.Knowledge.Releases` | committed |
| `knowledge/<tag>/metamodel/` | `Hypha.MetamodelGen` (C# / [uml4net](https://github.com/STARIONGROUP/uml4net)) | committed |
| `knowledge/<tag>/textual-notation/` | `Hypha.Knowledge.TextualNotation` + `.Grammar` | committed |
| `knowledge/<tag>/cross-references.json` | `Hypha.Knowledge.CrossReferences` | committed |
| `knowledge/<tag>/spec/` | `tools/spec-extract` (Python) | **git-ignored — regenerate locally** |

**.NET alone produces everything that ships.** Python is needed only for `knowledge/<tag>/spec/`,
which is git-ignored and already requires you to have obtained the OMG PDFs yourself.

## Running them

Everything is driven by the `hypha` command — not by the test suites, which verify rather than
generate:

```sh
dotnet run --project tools/hypha-cli/Hypha.Tools -- discover
dotnet run --project tools/hypha-cli/Hypha.Tools -- fetch --tag 2026-05
dotnet run --project tools/hypha-cli/Hypha.Tools -- generate
```

Installed, that is `hypha discover` / `hypha fetch` / `hypha generate`. See
[`hypha-cli/README.md`](hypha-cli/README.md) for installation, the full verb list and exit codes.

Each tool has its own README with build and test instructions.
