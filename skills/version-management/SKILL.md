---
name: version-management
description: Manage which OMG SysML v2 / KerML release(s) hypha has fetched and generated locally – list what's installed vs. available upstream, fetch a release, switch the default, or remove one that's no longer wanted. Use when the user asks what versions are available, wants to fetch/update/switch/remove a release, or when a SessionStart hook message reports nothing installed yet or a newer release available.
---

# Version management

Hypha keeps nothing generated in git: every release's metamodel, textual notation and
cross-references is fetched and generated **on this machine**, on request. This skill is the
vocabulary for doing that conversationally - list, fetch, switch, remove - all through the `hypha`
CLI (self-contained; no toolchain needed).

## Always confirm before acting

- **Fetching** costs real time (minutes for a first fetch) and a network round-trip. Never fetch a
  release just because a hook mentioned one exists - say what's available and ask.
- **Removing** is destructive (the release's files are deleted). Confirm which tag before running it.
- Switching the default is cheap and reversible - still worth naming what changed, not silent.

## Listing what's local vs. online

```sh
hypha check
```

Prints installed releases (with which is `default`) next to what's newest online, and tells you if a
newer release exists. This is the same comparison the `SessionStart` hook already ran once at session
start - re-run it if the user asks "what versions do I have" later in a long conversation, since the
hook only checks once per session.

For the full upstream list rather than just the newest few, use `hypha discover` (`--limit 0` for
everything, newest first).

## Fetching a release

Run both steps in the foreground - the user sees real progress as they stream, there is nothing to
poll for:

```sh
hypha fetch --tag <tag>
hypha generate --tag <tag>
```

`fetch` downloads that release's metamodel XMI, textual sources, and (by default) the OMG
specification PDFs into `sources/<tag>/` - all git-ignored, all local only. `generate` builds
`knowledge/<tag>/` from what was fetched. **`fetch` switches the default to whatever it just fetched,
every time** - if the user wants to add a release alongside what's already installed *without*
switching (e.g. "also get me an older one for comparison"), pass `--no-default`:

```sh
hypha fetch --tag <tag> --no-default
hypha generate --tag <tag>
```

Either way, say what the default is afterward - it's easy for the user to lose track of which release
they're now getting answers from.

Quoting normative specification text (`knowledge/<tag>/spec/`) is `generate`'s `spec` artifact: it
fetches and caches `uv`, which resolves or fetches a matching Python itself and runs
`tools/spec-extract` through it - no maintainer source checkout or pre-existing Python needed. The one
way this still degrades to naming a clause rather than quoting it is `uv` itself being unprovisionable
(offline the first time it's needed, or an unsupported platform); `generate` reports that as a `Skipped`
result with a reason, the same as any other artifact missing its inputs. Say so if the user specifically
asked for spec citation and it came back skipped.

## Switching the default release

```sh
hypha use --tag <tag>
```

Only works on an already-installed tag (fetch it first if it isn't). Every skill reads this default
unless the user names a different tag in their prompt for one answer.

## Removing a release

```sh
hypha remove --tag <tag>
```

Refuses on the current default (switch with `hypha use` first) and on the only installed release
(pass `--force` if that's really what's wanted - it leaves nothing installed). Confirm the tag with
the user before running this; there's no undo once the files are deleted.

## The Systems Modeling API is versioned separately

The Systems Modeling API & Services knowledge (the PIM, the REST/HTTP PSM and the API specification
text – see the `api-lookup` skill) is **not** part of a language release. It has its own versions,
its own default and its own manifest (`knowledge/api/versions.json`), driven by the same verbs under
`hypha api`:

```sh
hypha api discover                 # the API versions the upstreams offer
hypha api list                     # what is installed, and which is the default
hypha api fetch --tag <version>    # into sources/api/<version>/  (--no-default, --no-specs, --force)
hypha api generate --tag <version> # into knowledge/api/<version>/
hypha api use --tag <version>
hypha api remove --tag <version>
```

- An API version is a tag of the upstream API-Services repository, `2025-02` or later. That
  repository has not been tagged for every language release since, so the newest API version is
  usually **older** than the newest language release – that is expected, not something to fix.
- Installing, switching or removing an API version never touches an installed language release, and
  the other way round. `hypha check` and the `SessionStart` hook report on the language releases
  only: for the API, run `hypha api list` and `hypha api discover`.
- The same confirmations apply: ask before fetching, confirm the version before removing, and say
  what the default API version is afterward.

## Typical flows

- **Fresh install, hook says nothing is local yet**: name the releases the hook reported, ask which
  one, then `fetch` + `generate` it.
- **Hook says a newer release is available**: name it, ask whether to fetch it - clarify whether they
  want it to become the new default (plain `fetch`) or sit alongside the current one (`--no-default`).
- **User wants an older release for comparison**: `hypha discover --limit 0` to find the tag, then
  `fetch --no-default` + `generate` it so it joins what's already installed without switching away
  from the current default.
- **User asks about the SysML v2 API and nothing is installed under `knowledge/api/`**: say so,
  run `hypha api discover` to name what is on offer, ask which one, then `hypha api fetch` +
  `hypha api generate` it.
- **Cleaning up**: `hypha check` to see what's installed, confirm which tag(s) to drop, `hypha use` to
  move off any that's currently default, then `hypha remove` each.
