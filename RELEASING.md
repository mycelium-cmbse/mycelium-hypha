# Releasing

Two things ship from this repository, on **independent version lines**:

| What | Tag | Released by |
| --- | --- | --- |
| the `hypha` **plugin** (skills, agents, `knowledge/`) | `vX.Y.Z` | the steps below |
| the `hypha` **tools** (the generator CLI) | `tools-vX.Y.Z` | [the Release workflow](#releasing-the-tools) |

A plugin release does not require a tool release, or the other way round – the tool is developer
tooling that is not part of the shipped plugin.

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

1. Get `development` into a releasable state (all intended PRs merged, `dotnet test` / `pytest` green).
2. On a branch off `development`:
   - `.claude-plugin/plugin.json` — bump `version` to `X.Y.Z`.
   - `.claude-plugin/marketplace.json` — set the `hypha` entry's `source.ref` to `vX.Y.Z`.
   - `CHANGELOG.md` — move the `[Unreleased]` items under a new `## [X.Y.Z] - <YYYY-MM-DD>` heading and
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

## Conventions

- `version` (no `v`) in `plugin.json` always equals the tag minus its leading `v`
  (`version: "1.0.0"` ↔ tag `v1.0.0` ↔ `source.ref: "v1.0.0"`).
- Use [Semantic Versioning](https://semver.org/) and [Keep a Changelog](https://keepachangelog.com/).
- Never point `source.ref` at a tag that does not yet exist on the remote.

## Releasing the tools

The generator CLI ships two ways from one codebase: a `dotnet tool` package for anyone with the .NET
10 SDK, and a **self-contained** executable per platform for anyone without a toolchain.

Run the **Release** workflow (`workflow_dispatch`) with a SemVer version. It builds and tests the
solution, packs `Hypha.Tools`, publishes `win-x64` / `linux-x64` / `osx-x64` / `osx-arm64`
self-contained single files, generates `THIRD-PARTY-NOTICES.txt` for the bundled dependency closure,
tags `tools-vX.Y.Z` and opens a **draft** release with the archives attached. Review the draft and
publish it.

- The NuGet push is skipped with a notice when `NUGET_API_KEY` is not configured; the package is
  still attached to the release either way.
- Pre-releases (`1.2.3-beta.1`) are never pushed to NuGet.
- The workflow runs the full test suite first. One of those tests regenerates the knowledge base and
  compares it byte for byte against the committed files, so a build that can no longer reproduce what
  ships cannot be released.
- It then **runs the published binary** over the committed sources. A single-file bundle can fail in
  ways the in-process tests cannot see – its assemblies have no location on disk – so the artifact
  that ships has to prove it still generates.
