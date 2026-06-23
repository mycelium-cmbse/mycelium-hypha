# metamodel-gen (C# / uml4net)

Reads the KerML / SysML v2 XMI metamodel and emits the per-element markdown knowledge base.

- **Input:** `sources/xmi/` (`.kermlx`, `.sysmlx`, or metamodel XMI)
- **Output:** `knowledge/kerml/` and `knowledge/sysml2/` — one file per metaclass, plus `index.md`
- **Library:** [`uml4net.xmi`](https://www.nuget.org/packages/uml4net.xmi) (Starion Group, Apache-2.0)

## Status

Not yet implemented. Planned as a .NET console tool that:

1. Deserialises the XMI with uml4net.
2. Walks the metaclasses (features, generalizations, redefinitions, constraints).
3. Renders one markdown file per element, following the convention in `knowledge/README.md`.
4. Writes the `index.md` manifest for each metamodel.

> Confirm the current `uml4net.xmi` package version and target framework before adding the
> `.csproj` (the API and TFM may have moved since this scaffolding).
