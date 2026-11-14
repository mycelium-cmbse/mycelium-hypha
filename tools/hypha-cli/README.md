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
| the .NET 10 SDK | `dotnet tool install --global Hypha.Tools` ([nuget.org](https://www.nuget.org/packages/Hypha.Tools)) |
| no toolchain at all | download the self-contained archive for your platform from the [releases](https://github.com/mycelium-cmbse/mycelium-hypha/releases) |

The self-contained build bundles the .NET runtime, so nothing else needs installing. It is larger
(~80 MB) for exactly that reason.

Working inside a checkout, you can also skip installation entirely:

```sh
dotnet run --project tools/hypha-cli/Hypha.Tools -- generate
```

## Verbs

Every verb accepts `--repository-root`, `--toolchain-cache-root`, `--token`, `--log-level` and
`--no-logo`.

`--repository-root <folder>` is the folder holding `sources/` and `knowledge/`. When it is omitted,
`hypha` uses the first of:

1. the source checkout above the working directory, which is how a maintainer works;
2. the `HYPHA_HOME` folder, when that environment variable is set;
3. the source checkout above the `hypha` program itself, such as a build run with `dotnet run`
   from another folder;
4. `hypha` in the per-user application data folder (`%LOCALAPPDATA%\hypha` on Windows,
   `$XDG_DATA_HOME/hypha` or `~/.local/share/hypha` on Linux, `~/Library/Application Support/hypha`
   on macOS), the default `HYPHA_HOME`.

Every hypha client shares the `HYPHA_HOME` folder.

`--toolchain-cache-root <folder>` keeps the managed toolchain under that folder: the `uv` executable,
uv's package cache (`uv-cache/`) and the Python uv installs (`python/`). Without it, they are kept in
`toolchain/` in the `HYPHA_HOME` folder, never in uv's own per-user locations. A host that keeps
everything it provisions in its own storage, such as an editor extension, passes this option.

| Verb | What it does |
| --- | --- |
| `hypha discover` | Lists the releases both upstreams offer. |
| `hypha fetch --tag <release>` | Downloads that release's inputs into `sources/<tag>/` and records it in `knowledge/versions.json`. |
| `hypha generate [artifact]` | Generates the knowledge base. Every artifact for every installed release when nothing is narrowed. |
| `hypha list [--json]` | Lists the installed releases, which one answers by default, and which of their artifacts have been generated. |
| `hypha lookup --kind <metamodel\|model-library\|examples> --name <name> [--tag <release>] [--start-line N] [--max-lines N] [--json]` | Prints one page of a metamodel element's, model-library declaration's or worked example's generated page. Read-only. |
| `hypha query <question> [options] [--tag <release>] [--json]` | Answers a structural question about a release from its generated knowledge, one verb per question (`subclasses`, `features`, `keyword`, `diff`, ...), and `hypha query api <question> [--tag <version>]` about an API version. Read-only. |
| `hypha move-window --tag <release> [--keep N]` | Maintainer-only: fetches, regenerates, re-extracts specifications, re-blesses the metamodel-gen fixtures, verifies, then prunes locally-installed releases beyond `--keep` (default 2). |
| `hypha use --tag <release>` | Switches which installed release every skill answers from by default. |
| `hypha remove --tag <release> [--force]` | Removes one installed release's `sources/`/`knowledge/`; refuses on the current default or the last remaining release without `--force`. |
| `hypha check [--json]` | Compares installed releases against what is offerable upstream, with no fetching and no generating; the MCP server's `hypha_check_releases` runs the same check. |
| `hypha serve` | Runs the hypha MCP server over standard input and output, for an AI client to start (see "MCP server" below). |
| `hypha migrate [--from <folder>]` | Copies the releases the Claude plugin and the VS Code extension installed in their own folders into `HYPHA_HOME` (see "Moving to HYPHA_HOME" below). |

```sh
hypha discover                              # what can be installed
hypha fetch --tag 2026-05                   # sources/2026-05/{xmi,textual}
hypha generate                              # every artifact, every installed release
hypha generate metamodel --tag 2026-05      # one artifact, one release
hypha generate --output /tmp/knowledge      # generate without touching the repository
```

### Progress

`fetch`, `api fetch`, `generate` and `api generate` show a live progress display when their output
is a terminal: one bar per step, each with its latest message (`textual: 140 of 309 files`,
`2026-05: keywords: 1 files`). Log lines are written above the bars, and the HTTP request and retry
logging is held to warnings and errors, since the bars already count every request; pass
`--log-level Debug` to see it. When the output is redirected (an editor extension, a script, CI)
nothing is shown beyond the verb's own lines and the log, both on standard output as before (on
standard error under `--json`, see below).

### Looking things up

`hypha lookup` resolves a name against a release's generated index and prints one page of the
generated file, so a host never has to read the index format itself:

```sh
hypha lookup --kind metamodel --name PartUsage                 # the default release
hypha lookup --kind model-library --name Real --tag 2026-05    # ScalarValues::Real, from its declaration
hypha lookup --kind metamodel --name PartUsage --start-line 161
hypha lookup --kind examples --name Camera                     # several examples are called Camera: lists their pages
```

A name resolves in tiers, and the first tier with any match decides: the exact key or qualified name,
then the same ignoring case, then a trailing part of the qualified name (`Real` for
`ScalarValues::Real`, `Actions::ActionUsage`), then that ignoring case. One match is the answer;
several are `ambiguous`. Only when no tier matches are names that merely contain the request offered,
as `candidates`, never as an answer, even when there is only one.

A model-library page holds a whole upstream file, so its lookup starts at the declaration rather than
at the top of the page. The declaration is found by following the qualified name down the scopes of
the source: each enclosing name has to be declared with a body and the keyword the index recorded for
it, not merely mentioned in a comment, an import or a reference (`: LengthValue`, `:> Base`,
`specializes Usage`), and each member is looked for directly inside the body of the one before it, so
a member is not confused with a same-named member of another scope. `all` and a short name between
a keyword and its name (`feature all x`, `attribute <m> mass`) are allowed. When
the declaration cannot be found, the lookup starts at the top of the page and `declarationLine` is
`null`.

An index path that leaves the release folder (absolute, through `..`, or through a symbolic link
anywhere on the way) is refused, and so is an index that is not what the generators write; both ask
for `hypha generate`.

### Asking a question

`hypha query` answers the questions of the query API (`IKnowledgeQuery`, `IApiQuery`), the same
facts the MCP server's tools answer, one verb per question. `hypha query --help` lists them; each verb's
`--help` lists its options.

```sh
hypha query subclasses --of Usage --concrete-only
hypha query features --class PartUsage --name name
hypha query keyword --word ':>>' --grammar sysml
hypha query constraints --class PartUsage --own-only
hypha query diff --from 2026-02 --to 2026-05 --element PartUsage
hypha query search --text Feature --kind metamodel
hypha query search-specification --text "is typed by" --document kerml
hypha query resolve --address hypha://2026-05/metamodel/PartUsage
hypha query api type --name Commit
hypha query api schema --name CommitRequest
```

Every answer says where it comes from: its tier (`NORMATIVE` for specification text, `MODEL` for a
fact read from the metamodel, `DERIVED` for a closure, a cross-reference or a match), the release and
the generated file it was read from. Without `--json` that is the first line, followed by the answer
as indented JSON. With `--json` the whole result is one line: `value` (the answer, `null` when the
release has no such element), `grounding`, `unavailable` (why an optional artifact, such as the
specification text, cannot answer, and how to generate it; otherwise `null`), `found` and
`available`. An answer that finds nothing exits 0; a release that is not installed or generated, or
an argument a question refuses, exits 1.

`lookup` and `query` read the indexes through the same typed contracts and cache, so an index the
generators could not have written is refused by both.

### JSON output

`check`, `list`, `lookup`, `query` and `api list` take `--json`, for editors, language-model tools and scripts. Every
`--json` document follows the same rules:

- standard output holds exactly one line with one JSON document, with no banner and no wrapping, and every
  log line goes to standard error. HTTP request and retry logging is held to warnings and errors there,
  so a caller that never reads standard error (a script, say) cannot fill the pipe and stall
  the command;
- every property of the document is always present, `null` when it has no value;
- a failure is a document too, `{"error": "..."}`, with a non-zero exit code: 1, or 130 when
  cancelled. That includes a command line that does not parse and a failure around the verb, such as
  a folder that is not a hypha repository, an upstream that cannot be reached or a corrupt
  `knowledge/versions.json`. A lookup whose name does not resolve is not a
  failure: it exits 0 and says so in its `status`.

The property names are a contract, pinned by the tests in `Hypha.Tools.Tests`; renaming one is a
breaking change for every host that reads it.

The artifact names are not a fixed list: they are whatever `IKnowledgeGenerator` implementations are
registered, so `hypha generate nonsense` reports the ones that exist. Today they are `metamodel`,
`grammar-references`, `textual-notation`, `keywords`, `model-library`, `spec` and `cross-references`, run in that
order because the cross-references read what the others write.

`spec` is unlike the rest: it needs the OMG spec PDFs (`hypha fetch` gets them by default; pass
`--no-specs` to skip) and, the first time it runs, fetches and caches its own copy of
[`uv`](https://github.com/astral-sh/uv), which resolves or fetches a matching Python itself and
installs `tools/spec-extract`'s dependencies into a managed venv on demand, so it needs neither a
pre-existing Python nor a provisioned `.venv`. Outside a source checkout, `hypha` unpacks the extractor
it carries into `toolchain/spec-extract/`, next to `uv`. Either input being unavailable (no PDFs fetched, offline
on first `uv` provisioning) is a `Skipped` result, the same as any other generator missing its inputs;
it never blocks the rest of a `hypha generate` run.

### The Systems Modeling API: `hypha api`

The Systems Modeling API & Services knowledge is versioned apart from the language releases, so it
has the same verbs under a group of its own. They act on `sources/api/<version>/`,
`knowledge/api/<version>/` and the manifest `knowledge/api/versions.json`, and never touch an
installed language release.

| Verb | What it does |
| --- | --- |
| `hypha api discover` | Lists the API versions the upstreams offer: the API-Services tags that are also a language release, from `2025-02` on. |
| `hypha api fetch --tag <version>` | Downloads that version's inputs into `sources/api/<version>/` and records it. `--no-specs`, `--force` and `--no-default` as for `hypha fetch`. |
| `hypha api generate [artifact]` | Generates the API knowledge. Every artifact for every installed API version when nothing is narrowed; `--tag` and `--output` as for `hypha generate`. |
| `hypha api list [--json]` | Lists the installed API versions, which one answers by default, and which of their artifacts have been generated. `--json` prints the same shape as `hypha list --json`, with `versions` in place of `releases` and `pim`, `openApi`, `crossReferences` and `specification` as the artifacts. |
| `hypha api use --tag <version>` | Switches the default API version. |
| `hypha api remove --tag <version> [--force]` | Removes one installed API version, under the same rules as `hypha remove`. |

```sh
hypha api discover                          # what can be installed
hypha api fetch --tag 2026-04               # sources/api/2026-04/{openapi,xmi,specs}
hypha api generate                          # every artifact, every installed API version
hypha api generate pim --tag 2026-04        # one artifact, one API version
```

The API artifacts are the `IApiKnowledgeGenerator` implementations: the same contract as
`IKnowledgeGenerator`, registered as a separate collection so a run over the language releases never
picks them up. Today they are `pim`, `openapi`, `spec` and `cross-references`, run in that order
because the cross-references read what the others write. `spec` behaves as it does for a language
release: it needs the PDF and `uv`, and is a `Skipped` result without either.

A fetch reads four files from four places: the OpenAPI document from the API-Services repository,
the specification PDF from the Release repository and the KerML model the PIM refers to from the
Pilot-Implementation repository (all at the same tag), and the PIM model from the OMG document
server, because no repository carries it. That last request goes through a client of its own that
carries no credentials: `--token` is for GitHub and is never sent anywhere else.

`hypha check` and `hypha move-window` cover the language releases only, and so do the MCP server's
release tools.

### Moving the release window

`hypha move-window --tag <release>` is a maintainer-only convenience for advancing to a newer upstream
release in a source checkout: fetch → regenerate → re-extract specification text → re-bless
`Hypha.Knowledge.Metamodel.Tests`' `Expected/` golden files against its committed XMI fixture (a fixed
regression fixture, unrelated to the release just fetched; see CLAUDE.md's "Committed vs
git-ignored") → prune whatever falls outside the local `--keep` count → verify the result regenerates
self-consistently. `--keep` (default 2) is just how many releases stay installed locally; whichever
fall outside it have their `sources/<tag>/` and `knowledge/<tag>/` deleted **immediately**, in the
same run; nothing is left for later manual cleanup. Nothing is committed automatically, and nothing
per-release is committed at all (see CLAUDE.md): the deletions only ever touch the local, git-ignored
`sources/`/`knowledge/` folders.

It reports which step it is on as it goes and can take several minutes; specification extraction is
the dominant cost. Unlike every other verb, **it needs a full source checkout with the .NET SDK and a
provisioned `tools/spec-extract/.venv`** (`uv sync --extra dev`, or `python -m venv .venv && pip install
-e .[dev]`, from `tools/spec-extract/`): its re-bless and verify steps shell out to `dotnet test`, and specification
extraction shells out to `pytest`. It cannot run from the standalone distributed binary.

### Checking for newer releases

Nothing generated is committed to git (see the repository root `CLAUDE.md`): the plugin fetches and
generates every release entirely on the user's machine, on request, and holds none of that content in
git. `hypha check` compares what is installed locally against what is offerable upstream, and *only*
compares; the MCP server's `hypha_check_releases` runs the same check (`IReleaseCheck`), which the
`version-management` skill calls when the user asks about updates. Neither fetches or generates
anything; deciding whether to act on what they report is left entirely to the user.

That is a deliberate difference from `move-window`, not a smaller version of it:

- It **never shells out to `dotnet test`/`pytest`** (no re-bless, no verify, no specification
  extraction), so it works from the downloaded self-contained binary a plugin install actually has,
  not just from a full source checkout.
- It is **read-only**: a couple of GitHub API calls, nothing written to disk. There is nothing here for
  `IReleaseWindowEvictor` (which deletes indiscriminately by tag age) to get wrong, because nothing is
  ever deleted or fetched by this verb: only `hypha remove` (or `hypha_remove_release`), run because a
  user asked for exactly that tag to go, ever deletes a release.

### Fetching the specification PDFs

`hypha fetch --tag <release>` downloads the OMG specification PDFs **by default** now: a fully local
install needs everything, and downloading them is a plain HTTP call with no toolchain implication.
Pass `--no-specs` to skip them. They land in the git-ignored `sources/<tag>/specs/` and must never be
committed. *Quoting* their text (`knowledge/<tag>/spec/`) is `hypha generate`'s `spec` artifact (see
[`tools/spec-extract`](../spec-extract/README.md)), which needs neither Python nor a maintainer source
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


## MCP server

`hypha serve` runs the hypha MCP server, with the official `ModelContextProtocol` C# SDK, over
standard input and output: an AI client starts it, one server per client, and talks to it until it
disconnects. Standard output carries the protocol and nothing else; the log and any failure go to
standard error. The global options apply, so a client can pass `--repository-root`, and without it
the server answers from the same folder every other verb uses (`HYPHA_HOME` by default).

```json
{ "mcpServers": { "hypha": { "command": "hypha", "args": ["serve"] } } }
```

Its query tools are read-only, and each groups related questions behind a `question` argument whose
values are the `hypha query` verbs that answer the same thing:

| Tool | Answers |
| --- | --- |
| `hypha_release_status` | The installed releases and API versions, the defaults, and what each has generated. |
| `hypha_check_releases` | The installed releases and API versions against those offered upstream, naming a newer one when there is: what `hypha check` answers, and the one read tool that asks GitHub. |
| `hypha_metaclass` | `element`, `ancestors`, `subclasses`, `specializes`, `neighbours`, `constraints`, `operations`, `literals`, `features`, `effective-features`. |
| `hypha_feature` | `redefinitions`, `subsettings`, `opposite`, `feature-constraints`, `contributors`. |
| `hypha_typed_by` | `classes-with-feature-typed`, `features-accepting`, `containers`. |
| `hypha_package` | A metamodel package's metaclasses and nested packages. |
| `hypha_syntax` | `cross-reference`, `feature-assignments`, `built-by`, `keyword`. |
| `hypha_spec` | `clause` (verbatim text), `elements-in-clause`. |
| `hypha_search` | Metaclasses, library declarations, clauses and worked examples by name, each with a `hypha://` address. |
| `hypha_search_spec` | The clauses whose text says a word or phrase, the ones that say it most first, each with its `hypha://` address and an excerpt. |
| `hypha_resolve` | The page a `hypha://` address names. |
| `hypha_lookup` | An element page, a model-library declaration or a worked example, a page at a time. |
| `hypha_diff` | What changed between two installed releases. |
| `hypha_api` | `pim-class`, `type`, `operation`, `schema`, `clause` of a Systems Modeling API version. |

Every answer is the JSON document `hypha query --json` prints: the value and its grounding (tier,
release, source file). A question that cannot be answered, such as one about a release that is not
installed, is a tool error whose message says how to fix it. The server's `instructions` explain the
grounding tiers and which tool answers what, for clients that load no skills.

Three more tools change the installed releases, in the server's own process:

| Tool | Does |
| --- | --- |
| `hypha_install_release` | Fetches a release (the newest one offered upstream when no `tag` is given) and generates its knowledge; `includeSpecifications` and `makeDefault` default to `true`, and the release becomes the default only once its knowledge is generated with no generator refusing. It sends MCP progress notifications while it runs, one per file fetched and per artifact generated, and answers with what each artifact did. With `axis: "api"` it installs a Systems Modeling API version instead. |
| `hypha_use_release` | Makes an installed release, or with `axis: "api"` an API version, the default. |
| `hypha_remove_release` | Deletes an installed release, or with `axis: "api"` an API version, that is not the default (`force` for the last one). |

Each takes the release-operation lock, so two servers, or a server and the VS Code extension, change
the releases one at a time; under `serve` a tool waits up to ten minutes for another's change to
finish. The query tools need no telling afterwards: they read the version manifest on every call and
read a generated file again once it has changed on disk.

Every `hypha://` address in an answer (its `uri`) is also a resource, through the one template
`hypha://{tag}/{kind}/{name}`: reading it returns the page as Markdown, the whole metaclass, clause or
worked-example page, or a model-library declaration from where it is declared on. No resource is listed, since a
release has thousands. And five prompts give a client's prompt menu the guided flows: `overview`,
`explain_metaclass`, `cite_specification`, `validate_notation` and `api_lookup`. Each is a request
naming the tools to use, starting from what is installed; everything they do is also reachable through
the tools, for clients that support neither.

## MCP server launcher

Every client package starts the MCP server through one command, `bin/hypha-launch`: a package is a
git repository or a zip, which can carry a small launcher per platform but not a `hypha` of tens of MB.
The Claude plugin registers it in `.claude-plugin/plugin.json` (`mcpServers`); the other packages
follow (Step 7 of `hypha-mcp-refactor-plan.md`).

- **One path for every platform.** On Windows a client finds `bin/hypha-launch.exe` from the
  extensionless `bin/hypha-launch`; on macOS and Linux that path is a POSIX script that execs
  `bin/<rid>/hypha-launch`. A Node spawn and `cross-spawn`, which the MCP TypeScript SDK uses, both
  start the `.exe` from the extensionless path on Windows.
- **The matching `hypha`.** `bin/hypha-launch.json` names the `hypha` version the package starts. The
  launcher downloads `hypha-<version>-<rid>.zip` from the `tools-v<version>` release once, checks it
  against the checksum table in the release notes, keeps it in `HYPHA_HOME/bin/<version>/<rid>/`, and
  runs `hypha serve` on the same standard input and output, passing on any arguments it was given.
  Each download goes into a folder of its own and is renamed into place once verified, so two clients
  starting at once neither write into one folder together nor overwrite a `hypha` that is running.
- **The first start does not keep the client waiting.** A client gives a server a limited time to
  answer, and the first download can take longer, so the launcher answers at once, downloads
  meanwhile (`hypha_status` says so), then starts `hypha serve`, replays the client's `initialize` to
  it and tells the client that its tools, resources and prompts changed. From then on it passes every
  message through, so the first session gets the real tools without reconnecting.
  It resolves the package from its own location and `HYPHA_HOME` itself, never from a client's path
  variables, and the download honours `HTTPS_PROXY` and `GITHUB_TOKEN`.
- **`HYPHA_BIN`** names a `hypha` to start instead, for an offline or locked-down machine. It must be
  `hypha` 1.6.0 or later (an older one has no `serve`, and prints its help on standard output), given
  by its absolute path: a relative one would be resolved against whatever folder the client starts
  the server in, so the launcher refuses it.
- **A failure the model can see.** When there is no `hypha` to start (the download is blocked or
  cannot be written, the package is damaged, `HYPHA_HOME` cannot be used, the `hypha` found cannot be
  started), the launcher serves a minimal MCP server itself.
  Its `instructions` and its one tool, `hypha_status`, say why and how to fix it, so the model tells
  the user rather than the server failing to start.

The launcher is NativeAOT (`tools/hypha-cli/Hypha.Launcher`) and references nothing else in the
repository. `.github/workflows/launcher-binaries.yml` builds it on each platform's own runner and commits it
into `bin/`.

## Moving to HYPHA_HOME

Before `HYPHA_HOME`, each client kept its knowledge in a folder of its own: the Claude plugin in its
plugin folder (one per plugin version, under `plugins/cache/<marketplace>/hypha/<version>/` of the
Claude configuration folder, `CLAUDE_CONFIG_DIR` or `~/.claude`), and the VS Code extension in its
global storage (`<product>/User/globalStorage/mycelium-cmbse.mycelium-hypha/` of `%APPDATA%`,
`~/Library/Application Support` or `$XDG_CONFIG_HOME`, for VS Code, VS Code Insiders and VSCodium).

The first time `hypha` reads an empty `HYPHA_HOME`, it copies every release and API version those
folders installed, with their sources, so nothing is fetched or generated again. Where two clients
installed the same release, the one changed last wins, and so does its default. It copies rather
than moves, so the clients keep answering from their own folders until they use `HYPHA_HOME`
themselves. It holds the release-operation lock of `HYPHA_HOME` while it copies, and each client
folder's own lock while it copies from it, without waiting: a client installing a release at that
moment has listed it before generating it, so the migration stops, records nothing and is done on a
later run, rather than copy a half-built release. Each folder is copied under a
temporary name and renamed into place, and the manifests are written last, so an interrupted copy is
simply done again on the next run. A marker, `.migrated-from-clients`, then records that it ran and
from where, so it never runs again on its own, even after every release is removed. `hypha serve`
copies beside answering, so a client's handshake does not wait for it; every other verb copies first.
Nothing is copied when the knowledge is read from anywhere else (a checkout or `--repository-root`).

`hypha migrate` does it on request: it copies whatever `HYPHA_HOME` still lacks, keeping its default,
from the clients' folders or from the folders named with `--from`, each of which must hold a
`knowledge/versions.json`. A `HYPHA_HOME` that cannot be used is never reported by the automatic run,
only by the verbs that read from it.

## Rate limits

Every upstream read is public, so a token is optional, but the anonymous GitHub API allows 60
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

`linux-x64`, `linux-arm64`, `osx-x64` and `osx-arm64` work the same way; the release workflow does all
five.
**NativeAOT is deliberately out of scope for `Hypha.Tools` itself**: Handlebars.Net compiles its
templates at runtime through expression trees, which AOT forbids, and uml4net is reflection-heavy over
XMI. Trimming carries a milder version of the same risk and is left off until someone verifies it.

`Hypha.Launcher` (see "MCP server launcher" above) is the exception: it carries neither dependency,
so it *is* NativeAOT-published, and the result is committed to `bin/` rather than downloaded on
demand:

```sh
dotnet publish tools/hypha-cli/Hypha.Launcher -c Release -r win-x64 -p:PublishAot=true \
  -o publish/launch/win-x64
dotnet test tools/hypha-cli/Hypha.Launcher.Tests/Hypha.Launcher.Tests.csproj
```

NativeAOT publishing needs a platform linker (`Desktop development with C++` on Windows, `clang` on
Linux/macOS) that a plain `dotnet build`/`dotnet test` does not; see
[the NativeAOT prerequisites](https://aka.ms/nativeaot-prerequisites). `.github/workflows/launcher-binaries.yml`
runs the four platforms on their native runners and commits the results; that workflow, not a local
publish, is the normal way `bin/` gets updated.
