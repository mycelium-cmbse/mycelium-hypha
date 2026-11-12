---
name: version-management
description: "Manage which OMG SysML v2 / KerML release(s) hypha has fetched and generated locally: list what's installed vs. available upstream, fetch a release, switch the default, or remove one that's no longer wanted. Use when the user asks what versions are available, wants to fetch/update/switch/remove a release, or when a SessionStart hook message reports nothing installed yet or a newer release available."
---

# Version management

Hypha keeps nothing generated in git: every release's metamodel, textual notation and
cross-references is fetched and generated **on this machine**, on request. This skill is the
vocabulary for doing that conversationally (list, fetch, switch, remove), all through the `hypha`
CLI (self-contained; no toolchain needed).

## Always point `hypha` at the plugin's folder

Every `hypha` command in this skill runs with `--repository-root "${CLAUDE_PLUGIN_ROOT}"`: the
plugin's own folder, which holds the `knowledge/` the other skills read and is the folder the
`SessionStart` hook checks. Without it, `hypha` uses the per-user `HYPHA_HOME` folder instead, and a
release fetched there is invisible to this plugin. Keep the option on every command, including the
ones you compose yourself.

## Always confirm before acting

- **Fetching** costs real time (minutes for a first fetch) and a network round-trip. Never fetch a
  release just because a hook mentioned one exists; say what's available and ask.
- **Removing** is destructive (the release's files are deleted). Confirm which tag before running it.
- Switching the default is cheap and reversible, but still worth naming what changed, not silent.

## Listing what's local vs. online

```sh
hypha check --repository-root "${CLAUDE_PLUGIN_ROOT}"
```

Prints installed releases (with which is `default`) next to what's newest online, and tells you if a
newer release exists. This is the same comparison the `SessionStart` hook already ran once at session
start. Re-run it if the user asks "what versions do I have" later in a long conversation, since the
hook only checks once per session.

For the full upstream list rather than just the newest few, use `hypha discover` (`--limit 0` for
everything, newest first).

## Fetching a release

Run both steps in the foreground: the user sees real progress as they stream, there is nothing to
poll for:

```sh
hypha fetch --tag <tag> --repository-root "${CLAUDE_PLUGIN_ROOT}"
hypha generate --tag <tag> --repository-root "${CLAUDE_PLUGIN_ROOT}"
```

`fetch` downloads that release's metamodel XMI, textual sources, and (by default) the OMG
specification PDFs into `sources/<tag>/`, all git-ignored, all local only. `generate` builds
`knowledge/<tag>/` from what was fetched. **`fetch` switches the default to whatever it just fetched,
every time**. If the user wants to add a release alongside what's already installed *without*
switching (e.g. "also get me an older one for comparison"), pass `--no-default`:

```sh
hypha fetch --tag <tag> --no-default --repository-root "${CLAUDE_PLUGIN_ROOT}"
hypha generate --tag <tag> --repository-root "${CLAUDE_PLUGIN_ROOT}"
```

Either way, say what the default is afterward; it's easy for the user to lose track of which release
they're now getting answers from.

Quoting normative specification text (`knowledge/<tag>/spec/`) is `generate`'s `spec` artifact: it
fetches and caches `uv`, which resolves or fetches a matching Python itself and runs
`tools/spec-extract` through it, so no maintainer source checkout or pre-existing Python needed. The one
way this still degrades to naming a clause rather than quoting it is `uv` itself being unprovisionable
(offline the first time it's needed, or an unsupported platform); `generate` reports that as a `Skipped`
result with a reason, the same as any other artifact missing its inputs. Say so if the user specifically
asked for spec citation and it came back skipped.

## Switching the default release

```sh
hypha use --tag <tag> --repository-root "${CLAUDE_PLUGIN_ROOT}"
```

Only works on an already-installed tag (fetch it first if it isn't). Every skill reads this default
unless the user names a different tag in their prompt for one answer.

## Removing a release

```sh
hypha remove --tag <tag> --repository-root "${CLAUDE_PLUGIN_ROOT}"
```

Refuses on the current default (switch with `hypha use` first) and on the only installed release
(pass `--force` if that's really what's wanted, since it leaves nothing installed). Confirm the tag with
the user before running this; there's no undo once the files are deleted.

## The Systems Modeling API is versioned separately

The Systems Modeling API & Services knowledge (the PIM, the REST/HTTP PSM and the API specification
text; see the `api-lookup` skill) is **not** part of a language release. It has its own versions,
its own default and its own manifest (`knowledge/api/versions.json`), driven by the same verbs under
`hypha api`:

```sh
hypha api discover --repository-root "${CLAUDE_PLUGIN_ROOT}"  # the API versions the upstreams offer
hypha api list --repository-root "${CLAUDE_PLUGIN_ROOT}"  # what is installed, and which is the default
hypha api fetch --tag <version> --repository-root "${CLAUDE_PLUGIN_ROOT}"  # into sources/api/<version>/  (--no-default, --no-specs, --force)
hypha api generate --tag <version> --repository-root "${CLAUDE_PLUGIN_ROOT}"  # into knowledge/api/<version>/
hypha api use --tag <version> --repository-root "${CLAUDE_PLUGIN_ROOT}"
hypha api remove --tag <version> --repository-root "${CLAUDE_PLUGIN_ROOT}"
```

- An API version is a tag of the upstream API-Services repository, `2025-02` or later. That
  repository has not been tagged for every language release since, so the newest API version is
  usually **older** than the newest language release. That is expected, not something to fix.
- Installing, switching or removing an API version never touches an installed language release, and
  the other way round. `hypha check` and the `SessionStart` hook report on the language releases
  only: for the API, run `hypha api list` and `hypha api discover`.
- The same confirmations apply: ask before fetching, confirm the version before removing, and say
  what the default API version is afterward.

## Typical flows

- **Fresh install, hook says nothing is local yet**: name the releases the hook reported, ask which
  one, then `fetch` + `generate` it.
- **Hook says a newer release is available**: name it, ask whether to fetch it, and clarify whether they
  want it to become the new default (plain `fetch`) or sit alongside the current one (`--no-default`).
- **User wants an older release for comparison**: `hypha discover --limit 0` to find the tag, then
  `fetch --no-default` + `generate` it so it joins what's already installed without switching away
  from the current default.
- **User asks about the SysML v2 API and nothing is installed under `knowledge/api/`**: say so,
  run `hypha api discover` to name what is on offer, ask which one, then `hypha api fetch` +
  `hypha api generate` it.
- **Cleaning up**: `hypha check` to see what's installed, confirm which tag(s) to drop, `hypha use` to
  move off any that's currently default, then `hypha remove` each.
