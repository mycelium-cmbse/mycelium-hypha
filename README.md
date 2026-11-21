# mycelium-hypha

Hypha is the AI agent of the Mycelium ecosystem, a connecting filament between engineers and the SysML v2 specification. It is packaged as a Claude Code plugin with skills and an MCP server for KerML / SysML v2 metamodel lookup, standard model library lookup, Systems Modeling API lookup, normative spec citation, and SysML v2 textual-notation validation; as a VS Code extension for GitHub Copilot; and as the `hypha` command-line tool both are built on.

## What's in here

mycelium-hypha ships three things, each with a version and a release of its own. The plugin and the extension both run the tools.

### The tools: the `hypha` program

`hypha` is where everything that knows about SysML v2 and KerML lives. It fetches the OMG sources of a release from GitHub, generates the knowledge from them (the metamodel, the grammar, the
standard model library, the specification text and the cross-references) into the `HYPHA_HOME` folder, and answers from it. Its verbs (`hypha query`, `hypha lookup`, `hypha list`, ...) answer on the command line; `hypha serve` runs it as an MCP server, whose tools an AI client calls.

- Code: [`tools/`](tools/README.md): the CLI (`hypha-cli`), the MCP server (`hypha-mcp`) and the generation (`knowledge-gen`, `metamodel-gen`, `spec-extract`).
- Released as a `tools-vX.Y.Z` tag: a `dotnet tool` on [NuGet](https://www.nuget.org/packages/Hypha.Tools), and a self-contained executable per platform attached to the GitHub release.
- Used directly by developers, scripts and CI; everyone else gets it through the plugin or the extension.

### The plugin: what an AI client installs

The plugin holds almost no logic of its own:

- `skills/`: how to answer SysML v2 questions with hypha's tools.
- `bin/hypha-launch`: a small native launcher, committed per platform.
- The manifests every kind of client reads, all rendered from `manifest.source.json`: `.claude-plugin/` (Claude Code), `plugin.json` and `mcp.json` (Agent Plugins: Copilot, Cursor, Codex), `gemini-extension.json` (Gemini CLI) and `packaging/mcpb/` (the Claude Desktop bundle).

When a client starts it, the launcher reads the tools version `bin/hypha-launch.json` names, downloads that `hypha` from its `tools-v` release once, checks it against the release's checksums,
keeps it in `HYPHA_HOME` and hands the client over to `hypha serve`. The plugin is the tools, delivered and started for you. It is released as a `vX.Y.Z` tag, through the marketplace in
`.claude-plugin/marketplace.json`, with the `.mcpb` bundle attached to its GitHub release.

### The VS Code extension: the same server for GitHub Copilot

The extension ([`extensions/vscode/`](extensions/vscode/README.md)) registers `hypha serve` as an MCP server for Copilot, and has no tools, commands or storage of its own. Unlike the plugin it does not download `hypha`: each platform's VSIX bundles one, built from a `tools-v` tag. It is published to the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=mycelium-cmbse.mycelium-hypha) and to [Open VSX](https://open-vsx.org/extension/mycelium-cmbse/mycelium-hypha), marked as a preview, for Windows x64 and Linux x64/arm64; see its
[development guide](extensions/vscode/DEVELOPMENT.md).

### How they fit together

```
                  tools: hypha (CLI, MCP server, knowledge generation)
                 /                                                  \
 plugin: downloads it on first start                  extension: bundles it in its VSIX
 (launcher, skills, manifests)                        (registers it for Copilot)
        |                                                           |
 Claude Code, Claude Desktop, Copilot, Cursor,                VS Code with Copilot
 Codex, Gemini CLI, Junie, Zed, ...
                 \                                                  /
         one shared HYPHA_HOME: a release installed once answers in every client
```

- **The tools are released first.** A plugin or an extension that names a tools version cannot start before that version is published; [`RELEASING.md`](RELEASING.md) gives the order.
- **Their versions move on their own.** The tools take a major version when what they generate or read changes incompatibly; the plugin when what it gives a client is removed or changed; the
  extension by its own rule (an odd minor is a Marketplace pre-release).

```
mycelium-hypha/
├── .claude-plugin/   Plugin + marketplace manifests
├── bin/              The MCP server's launcher: starts the hypha server, downloading it the first time
├── skills/           Conversational workflows (metamodel lookup, API lookup, spec citation, validation, ...)
├── extensions/vscode/ VS Code GitHub Copilot extension
├── knowledge/        Knowledge base, fetched and generated locally, one folder per release tag
├── sources/          Raw OMG inputs, fetched locally, one folder per release tag
└── tools/            The hypha tools: the CLI, the MCP server and the knowledge generation
```

## Install

### VS Code GitHub Copilot (preview)

Install **Hypha – SysML v2 and KerML** from the Extensions view, or from its [Marketplace page](https://marketplace.visualstudio.com/items?itemName=mycelium-cmbse.mycelium-hypha).
It is published for Windows x64 and for Linux x64 and arm64, which also covers WSL and SSH remotes; on macOS, build a VSIX with the [development guide](extensions/vscode/DEVELOPMENT.md).
It gives Copilot the hypha MCP server, and shares its release data with every other Hypha client on the machine. See its [user guide](extensions/vscode/README.md).

### Claude Desktop

Download `hypha-<version>.mcpb` from the plugin's [release](https://github.com/mycelium-cmbse/mycelium-hypha/releases) and open it with Claude Desktop, which shows an installation dialog. The one bundle serves Windows, macOS and Linux. It holds the same launcher as the plugin, so it downloads `hypha` on its first start and shares its releases with every other Hypha client on the machine. Claude Desktop has no skills, so it works from the server's own instructions.

### Other MCP clients

JetBrains Junie, Devin Desktop (formerly Windsurf), Zed and Cline start the server from a configuration file you write once, and read the skills from a folder you copy them to. See [Hypha in other MCP clients](docs/other-clients.md) for each one's snippet and skills folder.

### Command line

```sh
dotnet tool install --global Hypha.Tools
```

[`Hypha.Tools`](https://www.nuget.org/packages/Hypha.Tools) needs the .NET 10 SDK; without it, download the self-contained executable for your platform from the [releases](https://github.com/mycelium-cmbse/mycelium-hypha/releases). See [tools/hypha-cli](tools/hypha-cli/README.md) for every verb.

### Claude Code

1. In Claude Code, add the marketplace and install the plugin:

   ```
   /plugin marketplace add mycelium-cmbse/mycelium-hypha
   /plugin install hypha@mycelium
   ```

   `/plugin list` confirms it's installed. The plugin starts the hypha MCP server through its launcher (`bin/hypha-launch`), which downloads the `hypha` version the plugin names the first time and keeps it in the per-user `hypha` data folder, so nothing else needs installing.

2. Ask it, for example, "what SysML v2 releases are available?" Claude will report that nothing is installed yet, name the releases available upstream, and ask which one to install. Confirm a  release, and it downloads and generates it for you (see [Releases](#releases)).

3. Ask your real question. Metamodel lookup and validation now work from what's installed; **spec citation additionally needs the specification text generated locally** (see [The knowledge base](#the-knowledge-base)).

Releases installed through the plugin are shared with every other client that starts the hypha server on the same machine.

Example prompts for step 3 above:

- *Metamodel lookup*: "What features does `PartUsage` own and inherit?", "How does `ConnectionUsage` relate to `ConnectionDefinition`?", or "Which metaclasses specialize `Feature`?"
- *Spec citation*: "What does the SysML v2 spec say about conformance?" or "Quote the normative rule for redefinition."
- *Validation*: "Is this valid SysML v2? `part def Vehicle { attribute mass : Real[2..1]; }`"

## Releases

KerML and SysML v2 are released on a rolling tag cadence (`YYYY-MM`), and hypha generates its knowledge base **per release tag**. Nothing is pre-installed: the first thing a fresh install does is ask which release to fetch.

Manage this conversationally, in plain language ("what releases are available", "install the latest", "switch to 2026-04", "remove 2026-03"), and Claude uses the hypha server's release tools for you (check, install, use, remove; the `hypha` CLI's `check` / `fetch` / `generate` / `use` / `remove` do the same, see [tools/hypha-cli](tools/hypha-cli/README.md) if you want to run it yourself). It always says what the current default release is, and asks before installing (real time, real bandwidth) or removing (deletes that release's files for good).

The skills state which release an answer came from, and answer from the default unless you name one.

## The knowledge base

Once a release is fetched and generated, hypha answers from:

- **the metamodel**: every KerML / SysML v2 metaclass, its features, generalizations and constraints
- **the textual notation**: the full keyword reference, plus every example model the release ships
- **the model library**: the standard libraries (ISQ, ScalarValues, SysML.sysml, ...)
- **cross-references**: links from each metamodel element to the specification clause, grammar production and worked example that treat it, so an answer can point back to *why*

### The Systems Modeling API

The **Systems Modeling API & Services**, the API layer of SysML v2, is a separate body of knowledge with a version of its own. Ask for it to be installed, as for a release (the release tools install an API version too), or run `hypha api fetch --tag <version>` and `hypha api generate --tag <version>` (`hypha api discover` lists the versions on offer):

- **the PIM**: every type of the Platform Independent Model with its attributes, multiplicities, derivation, subsetting and redefinition, and the six services with their operations
- **the REST/HTTP PSM**: every API schema of the OpenAPI document with its required and nullable members and whether it is a request or a response model, and every endpoint
- **the API specification text**, clause by clause, quotable on the same terms as the KerML and SysML texts
- **cross-references**: PIM type ↔ specification clause ↔ PSM schema, which PIM members a PSM model leaves out, and which endpoint each PIM service operation maps to

An API version is installed, switched and removed independently of the language releases, and the two do not have to match: the upstream API repository is not tagged for every language release.

**Spec citation needs the specification text generated locally.** Quoting the normative specification text verbatim needs the OMG PDFs, which installing a release downloads by default, and the clause text extracted from them, which the install generates. The first time, it downloads [`uv`](https://github.com/astral-sh/uv), which provides the Python the extraction needs, so this needs network access once but no Python or source checkout of your own. Without the specification text, metamodel lookup and validation still work fully, and spec citation can still name the governing clause, just not quote the text itself.

### Provenance

Facts carry one of three tiers, so a reader can tell what was read from what was inferred: `NORMATIVE` (verbatim clause-anchored spec text), `MODEL` (read from the metamodel XMI) and `DERIVED` (computed here: closures, name-matched cross-references). The tier definitions travel with the data, in the `provenanceTiers` block of `cross-references.json`.

## Sources

The knowledge base comes from two upstream OMG repositories, one release tag at a time:

- [Systems-Modeling/SysML-v2-Pilot-Implementation](https://github.com/Systems-Modeling/SysML-v2-Pilot-Implementation): the metamodel XMI.
- [Systems-Modeling/SysML-v2-Release](https://github.com/Systems-Modeling/SysML-v2-Release): the specification PDFs, the textual-notation grammar, and the example models.

The Systems Modeling API knowledge adds two more, one API version at a time:

- [Systems-Modeling/SysML-v2-API-Services](https://github.com/Systems-Modeling/SysML-v2-API-Services): the OpenAPI document of the REST/HTTP PSM.
- The [OMG document server](https://www.omg.org/spec/SystemsModelingAPI/1.0): the PIM model (`ptc/25-02-29`), which no repository carries.

Exact upstream commits and versions are recorded in [sources/README.md](sources/README.md).

## License

This repository's own code and content are licensed under **Apache-2.0**; see [LICENSE](LICENSE) and [NOTICE](NOTICE).

### Specifications & licensing

The knowledge base is built from third-party, separately-licensed inputs:

- The generated knowledge (`knowledge/<tag>/metamodel/`, `knowledge/<tag>/textual-notation/`) is a derivative "special purpose specification … based upon" the OMG specifications, used for
  informational purposes as permitted by the OMG specification license, and ships under this repository's Apache-2.0 license once generated. Nothing per-release is committed to this repository any more (see #106); it is fetched and generated on your own machine instead. The upstream OMG attributions and the OMG license text still apply, though, and are reproduced in [NOTICE](NOTICE).
- **OMG specification PDFs are copyrighted and never committed**: the OMG license forbids posting the specifications on a network, so they stay git-ignored (`sources/<tag>/specs/`), and the spec-derived `knowledge/<tag>/spec/` is git-ignored too. Obtain the PDFs from OMG: [KerML 1.0](https://www.omg.org/spec/KerML/1.0) (`formal/26-03-01`), [SysML 2.0](https://www.omg.org/spec/SysML/2.0) (`formal/26-03-02`), [Systems Modeling API & Services 1.0](https://www.omg.org/spec/SystemsModelingAPI/1.0) (`formal/26-03-04`).
- The **metamodel XMI** (`sources/<tag>/xmi/`) and the **textual-notation sources** (`sources/<tag>/textual/`), both git-ignored and fetched per release, come from the SysML v2 submission team's repositories under the **Eclipse Public License 2.0** (see [NOTICE](NOTICE) and [sources/README.md](sources/README.md)). The one exception is `sources/PrimitiveTypes.xmi` (the OMG UML primitives library, shared and tag-independent), which is committed.

## Security

Report a vulnerability privately, as the [security policy](.github/SECURITY.md) describes. The [security and deployment profile](docs/security-profile/README.md) describes, per version, what the MCP server starts, reads, writes and downloads, for administrators who need to approve it.

## Contributing

See [CONTRIBUTING](.github/CONTRIBUTING.md). `dotnet test mycelium-hypha.slnx` and `pytest` in `tools/spec-extract` exercise the generators; CI runs both with SonarQube analysis. The .NET tests **verify** the knowledge base rather than writing it: `KnowledgeRegenerationTests` fetches a release fresh and asserts two independent regenerations are byte-identical to *each other*, proving the generators are deterministic without a committed baseline to diff against (nothing per-release is committed any more; see #106). Regenerating for real is `hypha generate`'s job (see [tools/hypha-cli](tools/hypha-cli/README.md)), so a test run leaves the working tree clean.

## Code quality

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
