# metamodel-gen (C# / uml4net)

Reads the KerML / SysML v2 XMI metamodel and emits the per-element markdown knowledge base under
`knowledge/{kerml,sysml2}/`.

This tool is **never run as a CLI and is not distributed** — it is invoked exclusively from its
unit tests, which both exercise the generator and write the generated knowledge base.

## Projects

| Project | Type | Purpose |
| --- | --- | --- |
| `Hypha.MetamodelGen` | class library | XMI reading + (later) markdown generation |
| `Hypha.MetamodelGen.Tests` | NUnit tests | drives the generator and asserts its output |

Both target **net10.0** and are part of the root solution `mycelium-hypha.sln`.
Package restore uses the repo-local `NuGet.config` (nuget.org only).

Key dependency: [`uml4net.xmi`](https://www.nuget.org/packages/uml4net.xmi) `8.1.2` (Starion Group,
Apache-2.0), which reads UML 2.5.1 XMI via `XmiReaderBuilder.Create().Build()`.

## Inputs

Place the metamodel XMI under [`sources/xmi/`](../../sources/README.md) (git-ignored). The tests
locate the first `*.xmi` / `*.uml` file found there by walking up from the test output directory to
the repository root. With no XMI present the smoke test **skips** (the suite stays green).

## Build & test

```sh
dotnet build mycelium-hypha.sln
dotnet test  mycelium-hypha.sln
```

## Status

- [x] Solution, class library and test project; `uml4net.xmi` referenced and pinned.
- [x] `XmiModelReader.Read(path)` loads an XMI and exposes the model's top-level packages.
- [x] Smoke test loads the XMI and enumerates top-level packages (skips when no XMI is present).
- [ ] Generate the metamodel `index.md` files (issue #2).
- [ ] Generate the per-metaclass element files (issue #3).
