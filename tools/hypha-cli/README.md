# hypha-cli (C#)

The command-line tool that fetches the upstream OMG sources and generates the knowledge base from
them. It is the **only** driver of generation: the test suites verify what it produces, they no
longer produce it.

```
tools/hypha-cli/
├── Hypha.Tools/        the `hypha` command
└── Hypha.Tools.Tests/  the verbs, and the golden regeneration check
```

## Installing

Two deliveries, from one codebase. Pick whichever matches what you already have:

| You have | Install |
| --- | --- |
| the .NET 10 SDK | `dotnet tool install --global Hypha.Tools` |
| no toolchain at all | download the self-contained archive for your platform from the [releases](https://github.com/mycelium-cmbse/mycelium-hypha/releases) |

The self-contained build bundles the .NET runtime, so nothing else needs installing. It is larger
(~80 MB) for exactly that reason.

Working inside a checkout, you can also skip installation entirely:

```sh
dotnet run --project tools/hypha-cli/Hypha.Tools -- generate
```

## Verbs

Every verb accepts `--repository-root` (the folder holding `sources/` and `knowledge/`, discovered by
walking up from the working directory when omitted), `--token`, `--log-level` and `--no-logo`.

| Verb | What it does |
| --- | --- |
| `hypha discover` | Lists the releases both upstreams offer. |
| `hypha fetch --tag <release>` | Downloads that release's inputs into `sources/<tag>/` and records it in `knowledge/versions.json`. |
| `hypha generate [artifact]` | Generates the knowledge base. Every artifact for every installed release when nothing is narrowed. |
| `hypha list` | Lists the installed releases and which one answers by default. |
| `hypha move-window --tag <release> [--keep N]` | Fetches, regenerates, re-extracts specifications, re-blesses fixtures, verifies, then evicts releases outside the window (default `--keep`: 2). |

```sh
hypha discover                              # what can be installed
hypha fetch --tag 2026-05                   # sources/2026-05/{xmi,textual}
hypha generate                              # every artifact, every installed release
hypha generate metamodel --tag 2026-05      # one artifact, one release
hypha generate --output /tmp/knowledge      # generate without touching the repository
```

The artifact names are not a fixed list: they are whatever `IKnowledgeGenerator` implementations are
registered, so `hypha generate nonsense` reports the ones that exist. Today they are `metamodel`,
`grammar-references`, `textual-notation`, `model-library` and `cross-references`, run in that order
because the cross-references read what the others write.

### Moving the release window

`hypha move-window --tag <release>` is the one command that advances hypha to a newer upstream
release: fetch → regenerate → re-extract specification text → re-bless the metamodel-gen test
fixtures that follow the default release → evict whatever falls outside the window → verify the
result regenerates byte-identical. `--keep` (default 2) sets how many releases the window holds;
whichever fall outside it have their `sources/<tag>/` and `knowledge/<tag>/` deleted **immediately**,
in the same run — nothing is left for later manual cleanup. Nothing is committed automatically:
everything the command does, including the deletions, is still a `git checkout` away from undone
until you commit.

It reports which step it is on as it goes and can take several minutes — specification extraction is
the dominant cost. Unlike every other verb, **it needs a full source checkout with the .NET SDK and a
provisioned `tools/spec-extract/.venv`** (`python -m venv .venv && pip install -e .[dev]`, from
`tools/spec-extract/`): its re-bless and verify steps shell out to `dotnet test`, and specification
extraction shells out to `pytest`. It cannot run from the standalone distributed binary.

### Fetching the specification PDFs

`hypha fetch --tag <release> --include-specs` also downloads the OMG specification PDFs. They are
**OMG-copyrighted**, land in the git-ignored `sources/<tag>/specs/`, and must never be committed. You
only need them for spec citation – see [`tools/spec-extract`](../spec-extract/README.md).

### Exit codes

| Code | Meaning |
| --- | --- |
| 0 | Success. |
| 1 | Nothing to do, or a generator refused because its output would have been wrong. |
| 2 | A usage error: an artifact no generator produces, a tag that is not a release. |
| 130 | Cancelled. |

## Rate limits

Every upstream read is public, so a token is optional – but the anonymous GitHub API allows 60
requests an hour, which `discover` alone can exhaust. Set `--token`, `GITHUB_TOKEN` or `GH_TOKEN`.

## Why the tests no longer generate

Generation used to be driven by the test suites: running `dotnet test` rewrote `knowledge/`, and "is
the working tree clean afterwards?" doubled as the determinism check. That made every test run a
write to the repository, and made the determinism check something you had to remember to perform.

The split now is that **the CLI generates and the tests verify**. `KnowledgeRegenerationTests`
regenerates the whole knowledge base into a scratch folder and compares it byte for byte against the
committed one, which proves the same property without touching a tracked file. The seam that allows
it is `HyphaKnowledgeOptions.OutputRoot`: inputs are still read from the repository, only the output
moves.

## Building

```sh
dotnet build mycelium-hypha.sln
dotnet test tools/hypha-cli/Hypha.Tools.Tests/Hypha.Tools.Tests.csproj
```

Publishing a self-contained build by hand:

```sh
dotnet publish tools/hypha-cli/Hypha.Tools -c Release -r win-x64 \
  --self-contained true -p:PublishSingleFile=true -o publish/win-x64
```

`linux-x64`, `osx-x64` and `osx-arm64` work the same way; the release workflow does all four.
**NativeAOT is deliberately out of scope**: Handlebars.Net compiles its templates at runtime through
expression trees, which AOT forbids, and uml4net is reflection-heavy over XMI. Trimming carries a
milder version of the same risk and is left off until someone verifies it.
