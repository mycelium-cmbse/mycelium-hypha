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
walking up from the working directory when omitted), `--toolchain-cache-root`, `--token`,
`--log-level` and `--no-logo`.

`--toolchain-cache-root <folder>` keeps the managed toolchain – the `uv` executable, uv's package
cache (`uv-cache/`) and the Python uv installs (`python/`) – under that folder. Without it, `uv` is
cached in the per-user application data folder and uv keeps its own default locations, which other uv
users on the machine share. A host that keeps everything it provisions in its own storage, such as an
editor extension, passes this option.

| Verb | What it does |
| --- | --- |
| `hypha discover` | Lists the releases both upstreams offer. |
| `hypha fetch --tag <release>` | Downloads that release's inputs into `sources/<tag>/` and records it in `knowledge/versions.json`. |
| `hypha generate [artifact]` | Generates the knowledge base. Every artifact for every installed release when nothing is narrowed. |
| `hypha list [--json]` | Lists the installed releases, which one answers by default, and which of their artifacts have been generated. |
| `hypha lookup --kind <metamodel\|model-library> --name <name> [--tag <release>] [--start-line N] [--max-lines N] [--json]` | Prints one page of a metamodel element's or model-library declaration's generated page. Read-only. |
| `hypha move-window --tag <release> [--keep N]` | Maintainer-only: fetches, regenerates, re-extracts specifications, re-blesses the metamodel-gen fixtures, verifies, then prunes locally-installed releases beyond `--keep` (default 2). |
| `hypha use --tag <release>` | Switches which installed release every skill answers from by default. |
| `hypha remove --tag <release> [--force]` | Removes one installed release's `sources/`/`knowledge/`; refuses on the current default or the last remaining release without `--force`. |
| `hypha check [--json]` | Compares installed releases against what is offerable upstream - no fetching, no generating; the one verb the plugin's `SessionStart` hook drives. |

```sh
hypha discover                              # what can be installed
hypha fetch --tag 2026-05                   # sources/2026-05/{xmi,textual}
hypha generate                              # every artifact, every installed release
hypha generate metamodel --tag 2026-05      # one artifact, one release
hypha generate --output /tmp/knowledge      # generate without touching the repository
```

### Looking things up

`hypha lookup` resolves a name against a release's generated index and prints one page of the
generated file, so a host never has to read the index format itself:

```sh
hypha lookup --kind metamodel --name PartUsage                 # the default release
hypha lookup --kind model-library --name Real --tag 2026-05    # ScalarValues::Real, from its declaration
hypha lookup --kind metamodel --name PartUsage --start-line 161
```

A name resolves in tiers, and the first tier with any match decides: the exact key or qualified name,
then the same ignoring case, then a trailing part of the qualified name (`Real` for
`ScalarValues::Real`, `Actions::ActionUsage`), then that ignoring case. One match is the answer;
several are `ambiguous`. Only when no tier matches are names that merely contain the request offered,
as `candidates` – never as an answer, even when there is only one.

A model-library page holds a whole upstream file, so its lookup starts at the declaration rather than
at the top of the page. The declaration is found by following the qualified name down the scopes of
the source: each enclosing name has to be declared with a body, not merely mentioned in a comment or
used as a type (`: LengthValue`, `:> Base`), and each member is looked for directly inside the body
of the one before it, so a member is not confused with a same-named member of another scope. When
the declaration cannot be found, the lookup starts at the top of the page and `declarationLine` is
`null`.

An index path that leaves the release folder – absolute, through `..`, or through a symbolic link
anywhere on the way – is refused, and so is an index that is not what the generators write; both ask
for `hypha generate`.

### JSON output

`check`, `list` and `lookup` take `--json`, for editors, language-model tools and scripts. Every
`--json` document follows the same rules:

- standard output holds exactly one line with one JSON document – no banner, no wrapping – and every
  log line goes to standard error;
- every property of the document is always present, `null` when it has no value;
- a failure is a document too, `{"error": "..."}`, with a non-zero exit code: 1, or 130 when
  cancelled. That includes a failure before the verb runs, such as a folder that is not a hypha
  repository or an upstream that cannot be reached. A lookup whose name does not resolve is not a
  failure: it exits 0 and says so in its `status`.

The property names are a contract, pinned by the tests in `Hypha.Tools.Tests`; renaming one is a
breaking change for every host that reads it.

The artifact names are not a fixed list: they are whatever `IKnowledgeGenerator` implementations are
registered, so `hypha generate nonsense` reports the ones that exist. Today they are `metamodel`,
`grammar-references`, `textual-notation`, `model-library`, `spec` and `cross-references`, run in that
order because the cross-references read what the others write.

`spec` is unlike the rest: it needs the OMG spec PDFs (`hypha fetch` gets them by default; pass
`--no-specs` to skip) and, the first time it runs, fetches and caches its own copy of
[`uv`](https://github.com/astral-sh/uv), which resolves or fetches a matching Python itself and
installs `tools/spec-extract`'s dependencies into a managed venv on demand - so it needs neither a
pre-existing Python nor a provisioned `.venv`. Either input being unavailable (no PDFs fetched, offline
on first `uv` provisioning) is a `Skipped` result, the same as any other generator missing its inputs;
it never blocks the rest of a `hypha generate` run.

### Moving the release window

`hypha move-window --tag <release>` is a maintainer-only convenience for advancing to a newer upstream
release in a source checkout: fetch → regenerate → re-extract specification text → re-bless
`Hypha.MetamodelGen.Tests`' `Expected/` golden files against its committed XMI fixture (a fixed
regression fixture, unrelated to the release just fetched - see CLAUDE.md's "Committed vs
git-ignored") → prune whatever falls outside the local `--keep` count → verify the result regenerates
self-consistently. `--keep` (default 2) is just how many releases stay installed locally; whichever
fall outside it have their `sources/<tag>/` and `knowledge/<tag>/` deleted **immediately**, in the
same run — nothing is left for later manual cleanup. Nothing is committed automatically, and nothing
per-release is committed at all (see CLAUDE.md) — the deletions only ever touch the local, git-ignored
`sources/`/`knowledge/` folders.

It reports which step it is on as it goes and can take several minutes — specification extraction is
the dominant cost. Unlike every other verb, **it needs a full source checkout with the .NET SDK and a
provisioned `tools/spec-extract/.venv`** (`python -m venv .venv && pip install -e .[dev]`, from
`tools/spec-extract/`): its re-bless and verify steps shell out to `dotnet test`, and specification
extraction shells out to `pytest`. It cannot run from the standalone distributed binary.

### Automatic version check

Nothing generated is committed to git (see the repository root `CLAUDE.md`): the plugin fetches and
generates every release entirely on the user's machine, on request, and holds none of that content in
git. `hypha check` is what the plugin's `SessionStart` hook runs, every session, to compare what is
installed locally against what is offerable upstream - and *only* to compare. It never fetches or
generates anything itself; deciding whether to act on what it reports is left entirely to the user,
through the `version-management` skill (`skills/version-management/SKILL.md`).

That is a deliberate difference from `move-window`, not a smaller version of it:

- It **never shells out to `dotnet test`/`pytest`** - no re-bless, no verify, no specification
  extraction - so it works from the downloaded self-contained binary a plugin install actually has,
  not just from a full source checkout.
- It is **read-only**: a couple of GitHub API calls, nothing written to disk. There is nothing here for
  `IReleaseWindowEvictor` (which deletes indiscriminately by tag age) to get wrong, because nothing is
  ever deleted or fetched by this verb - only `hypha remove`, run because a user asked for exactly that
  tag to go, ever deletes a release.

It is driven by a small companion project, `Hypha.Tools.Hook` (`tools/hypha-cli/Hypha.Tools.Hook`),
**not** part of `Hypha.Tools` itself. `Hypha.Tools.Hook` is the one thing in this repository that *is*
NativeAOT-published and committed to git, at `hooks/native/<rid>/hypha-hook(.exe)`: unlike the main
CLI, it never generates anything, only downloads and launches the CLI that does, so it carries none of
the reflection-heavy dependencies (Handlebars.Net, uml4net) that keep `Hypha.Tools` off AOT - which is
what makes it small enough (single-digit MB per platform) to commit rather than download on demand.
`.claude-plugin/plugin.json` registers a `SessionStart` hook whose command is a one-line POSIX shell
dispatch shim - pure glue, no logic - that `exec`s the right platform binary for the current OS and
architecture; see `.github/workflows/hook-binaries.yml` for how those binaries are built.

Each session start, that binary:

1. Downloads and checksum-verifies the pinned `hypha` CLI version (`plugin.json`'s
   `hyphaCliVersion`) from GitHub Releases the first time it is needed, caching it outside the
   plugin's own git-managed folder (under the OS's local application data directory) so a plugin
   update/reinstall can never disturb it mid-run.
2. Runs `hypha check --json`, **synchronously** - cheap enough to wait for inline (SessionStart's
   default timeout is 10 minutes), with nothing to poll for later the way a background fetch would
   need.
3. Reports the result as one line of `additionalContext`: nothing installed yet (names what is
   available online), a newer release exists (names it), or nothing to report - silent, matching the
   tone of the OMG-PDF-presence check this hook also folds in (see `CheckResultSummarizer`).

### Fetching the specification PDFs

`hypha fetch --tag <release>` downloads the OMG specification PDFs **by default** now - a fully local
install needs everything, and downloading them is a plain HTTP call with no toolchain implication.
Pass `--no-specs` to skip them. They land in the git-ignored `sources/<tag>/specs/` and must never be
committed. *Quoting* their text (`knowledge/<tag>/spec/`) is `hypha generate`'s `spec` artifact - see
[`tools/spec-extract`](../spec-extract/README.md) - which needs neither Python nor a maintainer source
checkout of its own any more: it fetches and caches `uv`, which resolves or fetches a matching Python
itself. The one remaining way spec-citation degrades to naming a clause rather than quoting it is `uv`
itself being unprovisionable (offline on its first use, or an unsupported platform).

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
**NativeAOT is deliberately out of scope for `Hypha.Tools` itself**: Handlebars.Net compiles its
templates at runtime through expression trees, which AOT forbids, and uml4net is reflection-heavy over
XMI. Trimming carries a milder version of the same risk and is left off until someone verifies it.

`Hypha.Tools.Hook` (see "Automatic version check" above) is the exception: it carries neither dependency, so it
*is* NativeAOT-published, and the result is committed to `hooks/native/<rid>/` rather than downloaded
on demand:

```sh
dotnet publish tools/hypha-cli/Hypha.Tools.Hook -c Release -r win-x64 -p:PublishAot=true \
  -o publish/hook/win-x64
dotnet test tools/hypha-cli/Hypha.Tools.Hook.Tests/Hypha.Tools.Hook.Tests.csproj
```

NativeAOT publishing needs a platform linker (`Desktop development with C++` on Windows, `clang` on
Linux/macOS) that a plain `dotnet build`/`dotnet test` does not - see
[the NativeAOT prerequisites](https://aka.ms/nativeaot-prerequisites). `.github/workflows/hook-binaries.yml`
runs the four platforms on their native runners and commits the results; that workflow, not a local
publish, is the normal way `hooks/native/` gets updated.
