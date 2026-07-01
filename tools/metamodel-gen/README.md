# metamodel-gen (C# / uml4net)

Reads the combined KerML / SysML v2 XMI metamodel and emits the per-element markdown knowledge base
under `knowledge/metamodel/`, plus a structured **JSON sidecar** (see below) for programmatic queries.

This tool is **never run as a CLI and is not distributed** — it is invoked exclusively from its
unit tests, which both exercise the generators and write the generated knowledge base.

## Projects

| Project | Type | Purpose |
| --- | --- | --- |
| `Hypha.MetamodelGen` | class library | XMI reading + markdown / JSON generation |
| `Hypha.MetamodelGen.Tests` | NUnit tests | drives the generators and asserts their output |

Both target **net10.0** and are part of the root solution `mycelium-hypha.sln`.
Package restore uses the repo-local `NuGet.config` (nuget.org only).

Key dependency: [`uml4net.xmi`](https://www.nuget.org/packages/uml4net.xmi) `8.2.1` (Starion Group,
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

The generators run as tests and write the committed knowledge base. After an intended format change,
re-run the `[Explicit]` `Bless_*` tests to regenerate the golden/committed files.

## JSON sidecar

Alongside the markdown, the converter emits a structured JSON form of the metamodel (same `uml4net`
pass, so the two never drift), committed under `knowledge/metamodel/`:

- **`metamodel.json`** — the full graph: every class (with **precomputed** inheritance closures
  `allAncestors` / `allDescendants`, owned + inherited attributes, operations, constraints),
  enumerations, primitive types and packages. Schema: `metamodel.schema.json`.
- **`index.json`** — a small flat name → `{kind, package, qualifiedName, …, file}` lookup for fast
  resolution before drilling into the rich file. Schema: `index.schema.json`.

Multiplicity upper bound `-1` denotes unbounded (`*`); output is deterministic (sorted, LF, no
timestamps) and golden-tested against the committed files.

### Querying with `jq`

These structural questions are one-shot `jq` queries against the precomputed graph:

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

**Requirement:** `jq` is a small standalone binary (not Python/Node) — install via `brew install jq`,
`sudo apt install jq`, or `winget install jqlang.jq`. It is an *accelerator*, not a hard requirement:
when `jq` is unavailable, read `metamodel.json` / `index.json` (or the markdown) directly. The future
MCP server will load the same JSON in memory and need neither `jq` nor a shell.

## Status

- [x] Solution, class library and test project; `uml4net.xmi` referenced and pinned.
- [x] `XmiModelReader.Read(path)` loads an XMI and exposes the model's packages.
- [x] Generate the metamodel `index.md` (issue #2).
- [x] Generate the per-metaclass element files, enumerations and primitive types (issue #3, #23).
- [x] Emit the `metamodel.json` / `index.json` JSON sidecar (issue #27).
