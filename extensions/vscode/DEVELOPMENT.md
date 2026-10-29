# Developing Hypha for VS Code

For installation and everyday use, see [Hypha for VS Code](README.md).

This prototype delivers release-aware SysML v2/KerML knowledge through one standard VS Code
extension. It contributes release-management commands and two GitHub Copilot tools:

- **Hypha Release Status** (`#hyphaReleaseStatus`) reports the default release and artifact presence.
- **Hypha Grounded Knowledge Lookup** (`#hyphaLookup`) reads generated metamodel elements and
  standard-library declarations, returning release-qualified source citations and paginated text.

The extension is not yet published to the Marketplace. Switching and removing releases are not yet
exposed as extension commands. Lookups are not compiler validation or normative specification searches.

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
confirmation, **Hypha: Install a SysML v2 Release** fetches upstream inputs, makes the release the
default and generates knowledge locally. First specification extraction may download checksum-verified
`uv`, a matching Python and Python dependencies. Cancellation or an unavailable toolchain can leave
partial data; status reports presence rather than treating a manifest entry as proof of completion.

## Build and package

From this directory, with Node.js 22+ and the .NET 10 SDK:

```sh
npm ci
npm test
npm run build:cli                  # current host; required before F5 installation tests
npm run package -- linux-x64       # produces hypha-linux-x64.vsix
```

Other package targets are `win32-x64`, `darwin-x64` and `darwin-arm64`.
Set `DOTNET` to an absolute SDK executable if `dotnet` is not on `PATH`.
The package command builds the CLI and extension and includes the required runtime assets.
Development-only packaging dependencies currently have four moderate npm audit advisories;
`npm audit --omit=dev` is clean and no Node dependencies ship in the VSIX.

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
