# Changelog

All notable changes to the Hypha VS Code extension are documented in this file. The hypha CLI it
bundles has its own changelog, in the
[mycelium-hypha repository](https://github.com/mycelium-cmbse/mycelium-hypha/blob/development/CHANGELOG.md).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Versions with an
odd minor number are Marketplace pre-releases; an even minor number marks a release.

## [0.1.1] – pre-release

Bundles hypha tools `1.5.0`.

### Added
- Published for **Windows x64**, validated end to end on Windows 11: installing a release, the
  first specification generation through `uv` (with an installed Python and with a managed one),
  cancelling during extraction (no `hypha`, `uv` or `python` process is left), the lock between two
  processes, and both Copilot tools.

### Changed
- The README's troubleshooting explains paths that go past Windows' 260-character limit for long
  user names or VS Code Insiders, and how to turn on long path support.

## [0.1.0] – first pre-release

Bundles hypha tools `1.5.0`.

### Added
- **Copilot tools** – `#hyphaReleaseStatus` reports the installed SysML v2 / KerML releases and
  which of their artifacts are present; `#hyphaLookup` resolves a metamodel or model-library name
  and answers with a release-qualified source link, the candidates when a name is ambiguous, and
  one page at a time.
- **Commands** – *Show Installed Releases*, *Check Available Releases*, *Install a SysML v2
  Release*, *Change Default Release* and *Remove a Release*. Installing asks for consent first,
  reports progress and can be cancelled; a release becomes the default only once it has been
  generated.
- Releases, generated knowledge and the toolchain live in the extension's global storage, shared
  by every workspace on the same host, and a lock file keeps two VS Code windows from changing
  releases at the same time.
- Nothing has to be installed by hand: the VSIX bundles a self-contained `hypha` CLI, which
  downloads `uv` on first use for the Python that specification extraction needs. Specification
  PDFs and extracted text are never packaged; they are downloaded and generated locally.
- Published for Linux x64 and arm64, which also covers WSL and SSH remotes. Windows and macOS
  packages build but have not yet been validated end to end.
