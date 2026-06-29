# Releasing the `hypha` plugin

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
