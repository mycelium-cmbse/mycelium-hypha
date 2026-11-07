# Developing Hypha for VS Code

For installation and everyday use, see [Hypha for VS Code](README.md).

This prototype delivers release-aware SysML v2/KerML knowledge through one standard VS Code
extension. It contributes release-management commands and two GitHub Copilot tools:

- **Hypha Release Status** (`#hyphaReleaseStatus`) reports the default release and artifact presence.
- **Hypha Grounded Knowledge Lookup** (`#hyphaLookup`) reads generated metamodel elements and
  standard-library declarations, returning release-qualified source citations and paginated text.

The extension is not yet published to the Marketplace. Release status and knowledge lookup are read
from the bundled CLI's JSON commands; the TypeScript host owns VS Code interaction, not the release
manifest or generated-file layout. Lookups are not compiler validation or normative specification searches.

## Shared runtime and data

Each platform-specific VSIX includes a self-contained Hypha CLI built from this checkout. End users
do not need .NET, Python, a source checkout, a separate Agent Plugin, or a CLI on `PATH`.
Supported targets are Windows x64, Linux x64 and macOS x64/arm64; web hosts are unsupported.
The extension runs on the workspace extension host, including WSL/SSH, in trusted, non-virtual
workspaces.

The bundled CLI is shared through the extension installation. Mutable inputs, generated knowledge,
the release manifest, and managed toolchain caches live under `ExtensionContext.globalStorageUri`:

```text
<globalStorageUri>/
  knowledge/versions.json
  knowledge/<tag>/
  sources/PrimitiveTypes.xmi
  sources/<tag>/
  tools/spec-extract/
  toolchain/
```

This data is available across workspaces on the same extension host, not synchronized across
Windows/WSL, hosts or machines. **Hypha: Show Installed Releases** logs the actual storage URI.
The CLI always receives this root explicitly; nothing is written into the modeling workspace.

The package includes the shared primitives input, specification-extraction code and license notices,
but **no specification PDFs, extracted normative text or per-release knowledge**. After explicit user
confirmation, **Hypha: Install a SysML v2 Release** fetches upstream inputs and generates knowledge
locally, then makes the release the default only after generation succeeds. First specification
extraction may download checksum-verified `uv`, a matching Python and Python dependencies. Cancellation
or an unavailable toolchain can leave partial data; status reports presence rather than treating a
manifest entry as proof of completion. Cancellation terminates the CLI process tree.

**Hypha: Change Default Release** selects another installed release. **Hypha: Remove a Release**
removes a selected release after confirmation; change the default first when removing the current
default. Release mutations are serialized within the extension host.

## Build and package

From this directory, with Node.js 22+ and the .NET 10 SDK:

```sh
npm ci
npm test
npm run build:cli                  # current host; required before F5 installation tests
npm run package -- linux-x64       # produces hypha-linux-x64.vsix
npm run package -- linux-x64 --pre-release   # the same, marked as a Marketplace pre-release
```

Other package targets are `linux-arm64`, `win32-x64`, `darwin-x64` and `darwin-arm64`.
Set `DOTNET` to an absolute SDK executable if `dotnet` is not on `PATH`.
Set `HYPHA_CLI_VERSION` to the hypha tools release a VSIX is built from (for example `1.5.0`) when
it is meant for publication; the bundled CLI then reports that version (`hypha --version` prints
it with the source commit, e.g. `1.5.0+f4c0548…`). Without it the CLI is marked as a development
build, `0.0.0-dev`.
The package command builds the CLI and extension and includes the required runtime assets.
The repository CI runs `npm test`, packages the Linux x64 extension and smoke-tests the installed VSIX.
Development-only packaging dependencies currently have five moderate npm audit advisories, all in
`@vscode/vsce`'s own dependency tree; `npm audit --omit=dev` is clean and no Node dependencies ship
in the VSIX.

## Publish to the Marketplace

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
  targets (default `linux-x64 linux-arm64`) and whether to publish a pre-release (default yes). It
  refuses to publish when `tools/` differs from the `tools-v<cli-version>` tag, so the bundled CLI is
  always exactly that release.
- The extension version comes from `package.json`. The Marketplace accepts only
  `major.minor.patch`, never the same version twice, and by convention an odd minor for
  pre-releases and an even minor for releases.

For development, open this folder and press **F5** after compiling and building the CLI.
For packaged validation, use **Extensions: Install from VSIX…** in the intended host. In a WSL window,
ensure Hypha is installed in WSL, not only on Windows. Do not run both the packaged extension and an
F5 copy in the same host when comparing results.

## Validate

The automated installed-package smoke test uses isolated user-data/extensions directories, a
synthetic knowledge fixture and an unrelated workspace:

```sh
npm run test:host
```

It downloads desktop VS Code and needs that application's normal system libraries and a display
(`xvfb-run -a npm run test:host` on headless Linux). It checks installed-package activation, the bundled
CLI and both language-model tools without changing the personal VS Code profile.
Linux/WSL is the currently exercised package and host-test target.
The installer ignores the launching terminal's VS Code remote CLI hook so that, even from a WSL
VS Code terminal, the VSIX is installed into the isolated extensions directory.

For interactive validation:

1. Run **Hypha: Check Available Releases**. It only reads local status and upstream tag catalogs.
2. Run **Hypha: Install a SysML v2 Release** and first cancel the confirmation. No fetch or generation
   should occur. Run it again and confirm.
3. Inspect the Hypha output channel and **Hypha: Show Installed Releases**. Core readiness requires
   metamodel, textual notation, model library and cross-references; specification readiness requires
   both KerML and SysML document indexes.
4. Enable Hypha's tools in Copilot Chat and try:

   ```text
   Use #hyphaReleaseStatus to report the default release and artifact readiness.
   Do not fetch or generate anything.

   Use #hyphaLookup to explain PartUsage from the default release's metamodel.
   Cite the release and source; follow pagination as needed.

   Use #hyphaLookup to find ScalarValues::Real in the default release's model library.
   Cite the release and source, not general knowledge.
   ```

5. Open an unrelated folder on the same host. Confirm the storage URI, release and lookup results
   remain the same. Ambiguous names must return alternatives, not an arbitrary source.

If an older build failed on missing primitives or skipped specification generation because of `uv`,
rebuild/reinstall and run installation again for the same tag. Existing fetched inputs are reused;
generation refreshes cross-references after successful specification extraction. Do not delete the
global data directory or replace its manifest with a synthetic one.
