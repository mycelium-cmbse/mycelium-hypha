# tools/

Generation pipelines that build `knowledge/` from `sources/`. These are **not** part of the
shipped Claude plugin; they are developer tooling, run when the upstream OMG sources change.

```
tools/
├── hypha-cli/       C#:     the `hypha` command, the entry point that drives everything below
├── knowledge-gen/   C#:     `Hypha.Knowledge`: layout, manifests and contracts, lookup, query API, release
│                            discovery/fetching, grammar, textual notation, cross-references
├── metamodel-gen/   C# (uml4net): `Hypha.Knowledge.Metamodel`, reads XMI → writes knowledge/<tag>/metamodel/
└── spec-extract/    Python: reads PDFs → writes knowledge/<tag>/spec/ (git-ignored)
```

The dependency direction is one way: `Hypha.Knowledge.Metamodel` references `Hypha.Knowledge`, never
the reverse, so uml4net and Handlebars stay out of the knowledge library. The types every generated
JSON document is written in, and its source-generated `JsonSerializerContext`, live in
`Hypha.Knowledge`, so the generators and the code that reads their output share them.

| Artifact | Produced by | Shipped |
| --- | --- | --- |
| `knowledge/versions.json` | `Hypha.Knowledge`, namespace `Hypha.Knowledge.Releases` | **git-ignored, local only** |
| `knowledge/<tag>/metamodel/` | `Hypha.Knowledge.Metamodel` (C# / [uml4net](https://github.com/STARIONGROUP/uml4net)) | **git-ignored, local only** |
| `knowledge/<tag>/textual-notation/` | `Hypha.Knowledge`, namespaces `Hypha.Knowledge.TextualNotation` + `.Grammar` | **git-ignored, local only** |
| `knowledge/<tag>/cross-references.json` | `Hypha.Knowledge`, namespace `Hypha.Knowledge.CrossReferences` | **git-ignored, local only** |
| `knowledge/<tag>/spec/` | `tools/spec-extract` (Python) | **git-ignored, regenerate locally** |

Nothing per release is committed: every artifact above is generated on the user's own machine. See
`CLAUDE.md`'s "Committed vs git-ignored" for what the repository does carry.

**.NET alone produces everything that ships.** Python is needed only for `knowledge/<tag>/spec/`,
which is git-ignored and already requires you to have obtained the OMG PDFs yourself.

## Running them

Everything is driven by the `hypha` command, not by the test suites, which verify rather than
generate:

```sh
dotnet run --project tools/hypha-cli/Hypha.Tools -- discover
dotnet run --project tools/hypha-cli/Hypha.Tools -- fetch --tag 2026-05
dotnet run --project tools/hypha-cli/Hypha.Tools -- generate
```

Installed, that is `hypha discover` / `hypha fetch` / `hypha generate`. See
[`hypha-cli/README.md`](hypha-cli/README.md) for installation, the full verb list and exit codes.

Each tool has its own README with build and test instructions.
