# metamodel-gen (C# / uml4net)

Reads the combined KerML / SysML v2 XMI metamodel and emits the per-element markdown knowledge base
under `knowledge/metamodel/`, plus a structured **JSON sidecar** (see below) for programmatic queries.

This is a class library with no entry point of its own. It registers a `MetamodelGenerator` into the
same `IKnowledgeGenerator` collection the other artifacts use, and
[`hypha generate metamodel`](../hypha-cli/README.md) resolves and runs it. The tests exercise the
generators and assert on scratch output; they no longer write the committed knowledge base.

## Projects

| Project | Type | Purpose |
| --- | --- | --- |
| `Hypha.Knowledge.Metamodel` | class library | XMI reading + markdown / JSON generation |
| `Hypha.Knowledge.Metamodel.Tests` | NUnit tests | exercises the generators and asserts their output |

Both target **net10.0** and are part of the root solution `mycelium-hypha.sln`.
Package restore uses the repo-local `NuGet.config` (nuget.org only).

Key dependency: [`uml4net.xmi`](https://www.nuget.org/packages/uml4net.xmi) `9.2.0` (Starion Group,
Apache-2.0), which reads UML 2.5.1 XMI via `XmiReaderBuilder.Create().Build()`.

## Inputs

The metamodel XMI lives under [`sources/xmi/`](../../sources/README.md) and **is committed**
(`SysML_only_xmi.uml`, `KerML_only_xmi.uml`, `PrimitiveTypes.xmi`). The tests locate it by walking up
from the test output directory to the repository root. With no XMI present the tests **skip** (the
suite stays green).

## Build & test

```sh
dotnet build mycelium-hypha.sln
dotnet test  mycelium-hypha.sln
```

Tests never write the committed knowledge base; that is the job of
`dotnet run --project tools/hypha-cli/Hypha.Tools -- generate metamodel`. After an intended format
change, regenerate, review the diff, then re-run the `[Explicit]` `Bless_*` tests to bring the golden
expectations under `Expected/` in line.

## JSON sidecar

Alongside the markdown, the converter emits a structured JSON form of the metamodel (same `uml4net`
pass, so the two never drift), committed under `knowledge/metamodel/`:

- **`metamodel.json`**: the full graph, every class (with **precomputed** inheritance closures
  `allAncestors` / `allDescendants`, owned + inherited attributes, operations, constraints),
  enumerations, primitive types and packages. Schema: `metamodel.schema.json`.
- **`index.json`**: a small flat name → `{kind, package, qualifiedName, …, file}` lookup for fast
  resolution before drilling into the rich file. It has no committed schema of its own.

Multiplicity upper bound `-1` denotes unbounded (`*`); output is deterministic (sorted, LF, no
timestamps) and golden-tested against the committed files.

### Querying the graph

The MCP server's tools and `hypha query` answer structural questions from this graph through
`IKnowledgeQuery`, without re-deriving anything. For a quick look by hand, the same questions are
one-shot `jq` queries against it (the query API's tests compare its answers with `jq` queries like these):

```sh
# Concrete subclasses of Usage (anywhere in the hierarchy)
jq -r '.classes[] | select(.allAncestors | index("Usage")) | select(.isAbstract|not) | .name' \
  knowledge/metamodel/metamodel.json

# All properties of PartUsage, owned + inherited
jq -r '.classes[] | select(.name=="PartUsage") | (.ownedAttributes + .inheritedAttributes)[].name' \
  knowledge/metamodel/metamodel.json

# Classes with a composite attribute typed by Expression
jq -r '.classes[] | select(.ownedAttributes[]? | .isComposite and .type=="Expression") | .name' \
  knowledge/metamodel/metamodel.json

# Resolve one element from the index
jq '.entries["PartUsage"]' knowledge/metamodel/index.json
```

Neither the plugin nor the MCP server needs `jq`; this file is not shipped with the plugin.

## Status

- [x] Solution, class library and test project; `uml4net.xmi` referenced and pinned.
- [x] `XmiModelReader.Read(path)` loads an XMI and exposes the model's packages.
- [x] Generate the metamodel `index.md` (issue #2).
- [x] Generate the per-metaclass element files, enumerations and primitive types (issue #3, #23).
- [x] Emit the `metamodel.json` / `index.json` JSON sidecar (issue #27).
- [x] Emit a Mermaid class diagram per package under `knowledge/metamodel/diagrams/` (issue #69).
