# Hypha.Tools: the `hypha` command

`hypha` fetches the upstream OMG **SysML v2** and **KerML** sources for a release and generates a
local knowledge base from them: the metamodel, the textual-notation grammar and its examples, the
standard model library, the specification text, and cross-references between them. The
**Systems Modeling API & Services**, the SysML v2 API, has its own versions and verbs.

It is the engine behind Hypha's [Claude Code plugin](https://github.com/mycelium-cmbse/mycelium-hypha)
and its [VS Code extension for GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=mycelium-cmbse.mycelium-hypha),
and can be used on its own from the command line or from scripts.

## Install

```sh
dotnet tool install --global Hypha.Tools
```

This needs the .NET 10 SDK. Without it, download the self-contained executable for your platform
from the [releases](https://github.com/mycelium-cmbse/mycelium-hypha/releases).

## Use

```sh
hypha discover                                   # the releases available upstream
hypha fetch --tag 2026-05                        # download one release's inputs
hypha generate --tag 2026-05                     # generate its knowledge base
hypha list                                       # installed releases and the default
hypha use --tag 2026-05                          # make it the default
hypha lookup --kind metamodel --name PartUsage   # one page of a metamodel element
hypha lookup --kind model-library --name ScalarValues::Real
hypha remove --tag 2026-05                       # remove an installed release
```

The Systems Modeling API has the same verbs under `hypha api` (`discover`, `fetch`, `generate`,
`list`, `use`, `remove`). `check`, `list`, `lookup` and `api list` take `--json`: one JSON document
on standard output, for editors, language-model tools and scripts.

Knowledge is written to `sources/` and `knowledge/` under `--repository-root`. The first generation
of the specification text downloads [`uv`](https://github.com/astral-sh/uv), which provides the Python
the extraction needs, so nothing else has to be installed by hand.

## Licensing

`hypha` is licensed under Apache-2.0. The OMG specification PDFs and the text extracted from them are
downloaded and generated on your own machine and are never redistributed: the OMG specification
license does not allow it. The metamodel XMI and the textual-notation sources come from the SysML v2
submission team's repositories under the Eclipse Public License 2.0.

## More

- [Command reference](https://github.com/mycelium-cmbse/mycelium-hypha/blob/development/tools/hypha-cli/README.md): every verb and option, lookup rules, the JSON contract and exit codes
- [Release notes](https://github.com/mycelium-cmbse/mycelium-hypha/releases)
- [Issues](https://github.com/mycelium-cmbse/mycelium-hypha/issues)
