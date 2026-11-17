# tools/

Generation pipelines that build `knowledge/` from `sources/`. These are **not** part of the
shipped Claude plugin; they build the `hypha` command and its MCP server, which run them on the
user's own machine when a release is installed.

```
tools/
├── hypha-cli/       C#:     the `hypha` command, the entry point that drives everything below
├── hypha-launcher/  C#:     `hypha-launch`, the MCP server launcher the client packages start (NativeAOT,
│                            committed in `bin/`)
├── hypha-mcp/       C#:     `Hypha.Mcp`, the MCP server `hypha serve` hosts
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

**.NET alone produces everything but the specification text.** Python is needed only for
`knowledge/<tag>/spec/`, from the OMG PDFs that `hypha fetch` downloads by default: `hypha generate
spec` fetches [`uv`](https://github.com/astral-sh/uv), which provides the Python, and runs
`spec-extract` through it.

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

`hypha-cli`, `hypha-launcher`, `metamodel-gen` and `spec-extract` each have a README of their own;
`hypha-mcp` and `knowledge-gen` build and test with the solution (`dotnet test mycelium-hypha.slnx`).
