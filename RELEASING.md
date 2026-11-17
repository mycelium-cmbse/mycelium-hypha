# Releasing

Three things ship from this repository, on **independent version lines**:

| What | Tag | Released by |
| --- | --- | --- |
| the `hypha` **tools** (the CLI, and `hypha serve`, the MCP server) | `tools-vX.Y.Z` | [the Release workflow](#releasing-the-tools) |
| the `hypha` **plugin** (skills, the MCP server launcher, the client manifests) | `vX.Y.Z` | [the steps below](#releasing-the-plugin) |
| the **VS Code extension** | none | the *Publish VS Code extension* workflow; see `extensions/vscode/DEVELOPMENT.md` |

The version lines are independent, but the releases are not: **the plugin depends on the tools.** Its
launcher (`bin/hypha-launch`) starts no server of its own. On a client's first start it downloads the
`hypha` that `bin/hypha-launch.json` names (`hyphaVersion`), from the GitHub release `tools-v<hyphaVersion>`,
and hands the client over to `hypha serve`. The VS Code extension bundles that same `hypha`, built from a
`tools-v*` tag. So:

- **The tools come first.** A plugin release, or an extension, that names a tools version must not be
  published before that tools release is: a client would find nothing to download, and start with only
  `hypha_status` saying why.
- A tools release on its own reaches no plugin user until a plugin release names it; `dotnet tool` users
  get it straight away.

## The order of a release

1. **The tools**, as a full release (`tools-vX.Y.Z`): run the Release workflow, review the draft and
   publish it. Check that the five archives are attached and that the release body still holds the
   `| asset | SHA256 |` table: the launcher reads each archive's checksum from it, and refuses an archive
   whose checksum is missing or differs. Then start the **First start** workflow with that version: on
   every platform the plugin ships a launcher for, it makes the launcher's first start against the
   published release, as a client does, then an install and a query.
2. **The plugin** (`vX.Y.Z`), naming that tools version: set `hyphaVersion` in `manifest.source.json` to
   it, with the plugin's own `version`, and follow [the steps](#steps). Run **Launcher binaries** first if
   `Hypha.Launcher` changed, so the committed launchers are current.
3. **The VS Code extension**, built from the same tools tag: start *Publish VS Code extension* with that
   version (`tools/` on the run's commit has to match the tag; the workflow checks).

The first release of `hypha serve` (#197) was this order with
tools 2.0.0 and plugin 3.0.0, after the pre-release below had been verified, and nothing before it but
that pre-release.

## A pre-release to verify first

Before a tools release that clients will download for the first time, verify a pre-release in every
client (Step 9, #201, holds the list of checks):

1. Run the Release workflow with a pre-release version, such as `2.0.0-beta.1`. The workflow marks the
   GitHub release as a pre-release. Review the draft and **publish** it: the launcher asks GitHub for the
   release by its tag, which a draft does not answer to.
2. The workflow pushes the package to nuget.org as well, as a pre-release: `dotnet tool install` and
   `dotnet tool update` pass it by unless asked for it (`--prerelease`). It stays there; nuget.org can
   unlist a version but never delete it.
3. Name it in a checkout of the plugin: set `hyphaVersion` in `manifest.source.json` to `2.0.0-beta.1`
   and bless the manifests (step 2 below), on a branch. Load the plugin, and the client configurations,
   from that checkout, so each client's first start downloads the pre-release. Do not tag a plugin
   release for it.
4. Start the **First start** workflow with `2.0.0-beta.1`. It covers the platforms and the POSIX
   dispatch without a client; the clients themselves are checked by hand, each from that checkout.
5. Once verified, release the same code as the full version (`2.0.0`) in [the order above](#the-order-of-a-release).

## Releasing the plugin

The plugin is published from this repository through `.claude-plugin/marketplace.json`. The plugin
entry pins its **source to a git tag** (`source.ref`), so users install a known, reproducible commit
rather than the tip of `development`.

How Claude Code resolves a release:

- **Marketplace catalog** (`marketplace.json`) is read from the repo's default branch (`development`)
  when a user runs `/plugin marketplace add mycelium-cmbse/mycelium-hypha`.
- **Plugin content** is fetched from the tag named in the `hypha` entry's `source.ref`.
- The `version` field in `.claude-plugin/plugin.json` gates updates: existing users are only prompted
  to update when it changes (not on every `development` commit).

A release therefore means: **bump the version, point the source at a new tag, and create that tag.**

## Steps

1. Get `development` into a releasable state (all intended PRs merged, `dotnet test` / `pytest` green),
   and publish the tools release the plugin will name, if it is a new one.
2. On a branch off `development`:
   - `manifest.source.json`: bump `version` to `X.Y.Z`, and set `hyphaVersion` to the tools release the
     launcher should download. It is the one place either is written; never edit a manifest by hand.
   - Write the manifests from it:
     `dotnet test mycelium-hypha.slnx --filter "FullyQualifiedName~PackageManifestTests.Bless_the_manifests"`.
     That sets `version` in `.claude-plugin/plugin.json`, `plugin.json` and `gemini-extension.json`, the
     `hypha` entry's `source.ref` in `.claude-plugin/marketplace.json` to `vX.Y.Z`, and `bin/hypha-launch.json`.
     `dotnet test` fails while a manifest differs from the source.
   - `CHANGELOG.md`: move the `[Unreleased]` items under a new `## [X.Y.Z] - <YYYY-MM-DD>` heading and
     leave a fresh empty `[Unreleased]`.
   - Open a PR into `development` and merge it.
3. Tag the merged commit and push the tag (the actual publish):
   ```sh
   git tag -a vX.Y.Z -m "hypha vX.Y.Z" <merge-commit>
   git push origin vX.Y.Z
   ```
   Tag **after** the PR merges, so the tagged tree already carries the bumped `version`.
4. Users receive it via `/plugin marketplace update` → `/reload-plugins` (prompted because `version`
   changed).
5. Pushing the tag starts the **Claude Desktop bundle** workflow (`desktop-bundle.yml`), which packs
   `hypha-X.Y.Z.mcpb` from the tagged tree (`packaging/mcpb/manifest.json`, the launchers in `bin/` and
   an icon) and keeps it as a workflow artifact. It refuses a tag whose version the manifest does not
   carry, so bless the manifests (step 2) before tagging, and check that this run is green before
   writing the release. The launchers in the bundle are the committed ones: run **Launcher binaries**
   first if `Hypha.Launcher` changed since they were built.
6. Write the GitHub release `vX.Y.Z` and publish it. Publishing starts the same workflow again, which
   attaches `hypha-X.Y.Z.mcpb` to the release; the workflow never creates a release itself. Check that
   the asset is there: the README sends Claude Desktop users to it. To attach it again, start the
   workflow by hand with the tag.

## Conventions

- `version` (no `v`) in `manifest.source.json`, and so in every manifest, always equals the tag minus
  its leading `v` (`version: "1.0.0"` ↔ tag `v1.0.0` ↔ `source.ref: "v1.0.0"`).
- `hyphaVersion` in `manifest.source.json` is the tools release the launcher downloads
  (`bin/hypha-launch.json`), on its own version line: bump it, and bless, when a new `tools-v*`
  release should reach plugin users, and only once that release is published.
- Use [Semantic Versioning](https://semver.org/) and [Keep a Changelog](https://keepachangelog.com/).
- Never point `source.ref` at a tag that does not yet exist on the remote.

## Releasing the tools

The CLI ships two ways from one codebase: a `dotnet tool` package for anyone with the .NET 10 SDK, and
a **self-contained** executable per platform for anyone without a toolchain, which is also what the
plugin's launcher downloads.

Run the **Release** workflow (`workflow_dispatch`) with a SemVer version. It builds and tests the
solution, packs `Hypha.Tools`, publishes `win-x64` / `linux-x64` / `linux-arm64` / `osx-x64` / `osx-arm64`
self-contained single files, generates `THIRD-PARTY-NOTICES.txt` for the bundled dependency closure,
tags `tools-vX.Y.Z` and opens a **draft** release with the archives attached and their SHA-256 in the
body. Review the draft and publish it; leave the checksum table as it is.

- The NuGet push is skipped with a notice when `NUGET_API_KEY` is not configured; the package is
  still attached to the release either way.
- A pre-release version (`1.2.3-beta.1`) is pushed to NuGet too, where it is marked as one, and the
  GitHub release is marked as a pre-release; see [A pre-release to verify first](#a-pre-release-to-verify-first).
- The workflow runs the full test suite first.
- It then **runs the published binary**: the `linux-x64` build fetches release 2026-05 (without the
  specification PDFs) and generates its knowledge. A single-file bundle can fail in ways the in-process
  tests cannot see (its assemblies have no location on disk), so the artifact that ships has to prove it
  still fetches and generates.
- The self-contained builds are published **without ReadyToRun**. Measured on Windows x64 in November
  2026 (`hypha serve`, eleven runs each, the builds alternating), ReadyToRun answered the first query
  230 ms sooner (674 ms from start instead of 903 ms; `initialize` 512 ms instead of 549 ms, the first
  `hypha_metaclass` 161 ms instead of 344 ms), but made the executable 133 MB instead of 89 MB and the
  very first start of a fresh download 1.3 s slower. A quarter of a second per start does not pay for
  half again the download.
