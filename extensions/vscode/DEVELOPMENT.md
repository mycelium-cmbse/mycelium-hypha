# Developing Hypha for VS Code

For installation and everyday use, see [Hypha for VS Code](README.md).

The extension is a thin package around the hypha MCP server. It has no tools and no commands of its
own: at activation it registers one MCP server definition provider
(`vscode.lm.registerMcpServerDefinitionProvider`, declared as `mycelium-hypha.server` under
`contributes.mcpServerDefinitionProviders`), which hands the editor the bundled `hypha`, started as
`hypha serve`. Everything Copilot can ask, and installing, switching and removing releases, goes
through the server's tools, as in every other client (`tools/hypha-cli/README.md` lists them).

## Shared runtime and data

Each platform-specific VSIX includes a self-contained Hypha CLI built from this checkout. End users
do not need .NET, Python, a source checkout, a separate Agent Plugin, or a CLI on `PATH`.
Supported targets are Windows x64/arm64, Linux x64/arm64 and macOS x64/arm64; web hosts are
unsupported.
The extension runs on the workspace extension host, including WSL/SSH, in trusted, non-virtual
workspaces.

The server is started without `--repository-root` and without an environment of its own, so it
reads `HYPHA_HOME`: the per-user `hypha` data folder every client shares, or the folder the
variable names. Nothing is kept in `ExtensionContext.globalStorageUri` any more; releases an earlier
version of the extension installed there are copied into `HYPHA_HOME` once by hypha itself
(`IKnowledgeMigration`), unless they are in an older generated format.

The server's working directory is set to the extension's own folder rather than the workspace
folder the editor would choose. `hypha serve` does not look above its working directory, so this
only keeps that directory predictable. A maintainer's caveat: under **F5** the development host's
server runs the CLI built into this checkout (`out/cli/`), so unless `HYPHA_HOME` is set it answers
from the checkout's own `knowledge/`, the checkout above the running program.

The package holds the executable and the license notices, and **no specification PDFs, extracted
normative text or per-release knowledge**. It no longer carries the specification extractor or the
UML primitives either: hypha embeds both and unpacks them into `HYPHA_HOME` when it needs them.

The minimum VS Code is 1.101, the first with the MCP server definition provider API. Activation is
`onStartupFinished`, which `vsce` requires to be stated now that no command or tool implies one.

## Build and package

From this directory, with Node.js 22+ and the .NET 10 SDK:

```sh
npm ci
npm test
npm run build:cli                  # current host; required before F5 installation tests
npm run package -- linux-x64       # produces hypha-linux-x64.vsix
npm run package -- linux-x64 --pre-release   # the same, marked as a Marketplace pre-release
```

Other package targets are `linux-arm64`, `win32-x64`, `win32-arm64`, `darwin-x64` and `darwin-arm64`.
Set `DOTNET` to an absolute SDK executable if `dotnet` is not on `PATH`.
Set `HYPHA_CLI_VERSION` to the hypha tools release a VSIX is built from (for example `2.0.0`) when
it is meant for publication; the bundled CLI then reports that version (`hypha --version` prints
it with the source commit, e.g. `2.0.0+f4c0548…`). Without it the CLI is marked as a development
build, `0.0.0-dev`.
The package command builds the CLI and the extension.
The repository CI runs `npm test`, packages the extension and smoke-tests the installed VSIX on
every target, each on a runner of its own platform: Linux x64, Windows x64, Windows on Arm
(`windows-11-arm`), and macOS on Apple silicon (`macos-latest`) and on Intel (`macos-15-intel`).
Every target but Linux is tested against VS Code stable and against 1.101.0, the oldest supported
release; on Windows those are also the two archive layouts `scripts/test-host.mjs` has to find VS
Code's `cli.js` in. Linux arm64 is packaged by the publish workflow only.
Development-only packaging dependencies currently have six moderate npm audit advisories, all in
the dependency tree of `@vscode/vsce` (also through `ovsx`); `npm audit --omit=dev` is clean and no Node dependencies ship
in the VSIX.

## Publish to the Marketplace and Open VSX

The extension is published under the `mycelium-cmbse` publisher by the **Publish VS Code
extension** workflow (`.github/workflows/publish-vscode.yml`), started by hand from the Actions tab.
It uses Marketplace **Trusted Publishing**: GitHub Actions proves its identity through OIDC
(`vsce publish --oidc`, vsce 4.1.0 or later), so no Marketplace token is stored anywhere.

- The publisher's Trusted Publishing policy names the repository owner `mycelium-cmbse`, the
  repository `mycelium-hypha`, the workflow `publish-vscode.yml` and the environment
  `vscode-marketplace`. Renaming the workflow file or the environment breaks publishing until the
  policy is updated.
- The `vscode-marketplace` GitHub environment is where approval is enforced: give it required
  reviewers, so a run waits for a maintainer before anything is published.
- The run takes the hypha tools version the bundled CLI is labelled with (`cli-version`), the
  targets (default `win32-x64 linux-x64 linux-arm64`), whether to publish a pre-release (default
  yes), and where to: the Marketplace (default yes), Open VSX (default no), or both, from the same
  VSIXs. Publish a new version for every supported target: users of a target left out stay on the
  previous version. A target can be added to an already published version by a later run with only
  that target. It
  refuses to publish when `tools/` or `sources/PrimitiveTypes.xmi` differs from the
  `tools-v<cli-version>` tag, so the bundled CLI is always exactly that release.
- **Open VSX** (open-vsx.org) is where VSCodium, Cursor and other VS Code-compatible editors install
  from. The run publishes there with `ovsx publish --trusted-publishing`, Open VSX's Trusted
  Publishing, so no Open VSX token is stored either. Before the first run, a maintainer:
  1. signs in to open-vsx.org with GitHub and signs the Eclipse Foundation Open VSX Publisher
     Agreement;
  2. creates the `mycelium-cmbse` namespace and claims its ownership (an issue on
     `EclipseFdn/open-vsx.org`), so the namespace is verified;
  3. adds a trusted publisher to the namespace naming the repository owner `mycelium-cmbse`, the
     repository `mycelium-hypha`, the workflow `publish-vscode.yml` and the environment
     `vscode-marketplace`, the same names the Marketplace policy has.
  A version Open VSX already has is refused, as on the Marketplace: to add Open VSX to a version
  already on the Marketplace, run with only Open VSX chosen.
- The extension version comes from `package.json`. The Marketplace accepts only
  `major.minor.patch`, never the same version twice, and by convention an odd minor for
  pre-releases and an even minor for releases.

For development, open this folder and press **F5** after compiling and building the CLI.
For packaged validation, use **Extensions: Install from VSIX…** in the intended host. In a WSL window,
ensure Hypha is installed in WSL, not only on Windows. Do not run both the packaged extension and an
F5 copy in the same host when comparing results.

## Validate

The automated installed-package smoke test uses isolated user-data and extensions directories, a
scratch `HYPHA_HOME` and an unrelated workspace:

```sh
npm run test:host
```

It downloads desktop VS Code and needs that application's normal system libraries and a display
(`xvfb-run -a npm run test:host` on headless Linux). It checks that the installed package activates
and registers its provider, that the bundled CLI is there and no copy of the extractor is, and then
starts the bundled `hypha serve` itself, over stdio, to check that it offers hypha's tools and
answers `hypha_release_status`. The editor only starts a registered MCP server once a chat needs
it, which the test host cannot trigger, so the test drives the same executable, arguments and
transport directly. It never touches the personal VS Code profile or the per-user `HYPHA_HOME`.
The installer ignores the launching terminal's VS Code remote CLI hook so that, even from a WSL
VS Code terminal, the VSIX is installed into the isolated extensions directory.

For interactive validation, which is the part the automated test cannot cover:

1. Install the VSIX, open Copilot Chat in Agent mode and check under **Configure Tools** that the
   **hypha** server and its tools are listed. **MCP: List Servers** shows the server and its log.
2. Ask which releases are installed and which are offered. Nothing should be downloaded.
3. Ask Copilot to install a release, and decline the confirmation first: nothing should be fetched.
   Ask again and confirm. Progress is reported while it runs.
4. Try a metamodel question, a model-library lookup and a specification quotation, and check that
   each answer names its release and grounding.
5. Open an unrelated folder on the same host and confirm the same release answers. If another Hypha
   client is installed on the machine, confirm it sees the release too.
