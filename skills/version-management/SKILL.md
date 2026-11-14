---
name: version-management
description: "Manage which OMG SysML v2 / KerML release(s) hypha has installed on this machine: list what's installed vs. available upstream, install a release, switch the default, or remove one that's no longer wanted. Use when the user asks what versions are available, wants to install/update/switch/remove a release, or when a check reports nothing installed yet or a newer release available."
---

# Version management

Hypha keeps nothing generated in git: every release's metamodel, textual notation, model library,
specification text and cross-references is downloaded and generated **on this machine**, on request.
This skill is the vocabulary for doing that conversationally (check, install, switch, remove) through
the **hypha MCP server's tools**. They are named here without a client's prefix: a client may show
`hypha_install_release` as, for example, `mcp__hypha__hypha_install_release`.

Every client that starts the server shares one set of installed releases, so a release installed
here answers in every other client too.

## Always confirm before acting

- **Installing** takes minutes and downloads from GitHub. Never install a release just because a check
  said one exists: say what is available and ask.
- **Removing** deletes the release's files, with no undo. Confirm which release first.
- **Switching** the default is cheap and reversible, but still say what changed.

## Tools

| What | Tool | Arguments |
| --- | --- | --- |
| What is installed, and which release is the default | `hypha_release_status` | |
| What is installed against what is offered upstream, and whether a newer release exists | `hypha_check_releases` | `api` (default true) for the API versions too |
| Install a release: download it, generate its knowledge, make it the default | `hypha_install_release` | `tag`; `includeSpecifications`; `makeDefault`; `axis` |
| Switch the default release | `hypha_use_release` | `tag`; `axis` |
| Remove a release | `hypha_remove_release` | `tag`; `force`; `axis` |

The three release tools take `axis`: `release`, the default, for a language release, or `api` for a
Systems Modeling API version (below). Every answer says which axis it was on.

## Checking what's local vs. online

`hypha_release_status` answers from this machine. `hypha_check_releases` asks GitHub: for the
releases and for the API versions, it lists what is `installed`, the `default`, the newest few
`offered` upstream, and `newer`, the newest offered one newer than everything installed, or `null`.
Call it when the user asks about updates, or once at the start of a session when that helps; it costs
a request to GitHub, not a download.

It lists the five newest releases offered. An older one can still be installed by its tag, which is
`YYYY-MM`, sometimes with a point suffix (`YYYY-MM.N`): ask the user which, or point them to
the upstream list at https://github.com/Systems-Modeling/SysML-v2-Release/releases, and pass it to
`hypha_install_release`. A tag that is not offered upstream fails, and the error says so.

## Installing a release

`hypha_install_release` downloads the release's inputs and generates its knowledge in one call, and
reports progress while it runs:

- `tag`: the release, such as `2026-05`; without it, the newest offered upstream.
- `includeSpecifications` (default true): also download the OMG specification PDFs and extract their
  text, which quoting the specification needs (the `spec-citation` skill). Without it, a citation
  names the clause but cannot quote it.
- `makeDefault` (default true): make it the default once its knowledge is generated. Pass `false` to
  add a release alongside the current default without switching, such as an older one to compare
  against. The first release installed is always the default.

Installing a release that is already installed keeps its files and generates again. That is also how
to add the specification text to a release installed without it: install it again with
`includeSpecifications` left on, which downloads the missing PDFs and extracts their text.

The answer says what was fetched, by kind, and each artifact's outcome: generated, or skipped with a
`reason` (such as the specification text when its PDFs or the tool that extracts them could not be
had). When `refusal` is set, an artifact stopped the generation because its output would have been
wrong: report it, with its reason. Say what the default release is afterwards (`isDefault`); it is
easy to lose track of which release answers.

## Switching the default release

`hypha_use_release` with `tag` makes an installed release the default, which every tool answers from
unless given a `tag` for one answer. Install the release first if it is not installed. The answer
names the `previous` default and whether it `changed`.

## Removing a release

`hypha_remove_release` with `tag` deletes an installed release: its downloaded sources and its
generated knowledge. It refuses the default release whatever `force` says: switch with
`hypha_use_release` first. It refuses the only installed release unless `force` is set, which leaves
nothing to answer from. Confirm the release with the user before calling it.

## The Systems Modeling API is versioned separately

The Systems Modeling API & Services knowledge (the PIM, the REST/HTTP PSM and the API specification
text; see the `api-lookup` skill) is **not** part of a language release. It has its own versions and
its own default.

- `hypha_check_releases` (with `api` left on) and `hypha_release_status` report the API versions.
- The same release tools install, switch and remove an API version, with `axis: "api"`:
  `hypha_install_release` (its `tag` is the API version; without one, the newest offered;
  `includeSpecifications` fetches the API specification PDF), `hypha_use_release` and
  `hypha_remove_release`. The same rules apply as for a release: ask before installing, confirm
  before removing, and the default API version cannot be removed until another one is the default.
- An API version is a tag of the upstream API-Services repository, `2025-02` or later. That
  repository has not been tagged for every language release, so the newest API version is usually
  **older** than the newest language release. That is expected, not something to fix.
- Installing, switching or removing a language release never touches an API version, and the other
  way round.

## Typical flows

- **Nothing is installed yet**: `hypha_check_releases` to name what is offered, ask which release,
  then `hypha_install_release` it.
- **A newer release is available**: name it, ask whether to install it, and whether it should become
  the default (`makeDefault`, the default) or sit alongside the current one (`makeDefault: false`).
- **An older release for comparison**: install it by its `tag` with `makeDefault: false`, so the
  current default stays; `hypha_diff` then compares the two.
- **The user asks about the SysML v2 API and no API version is installed**: say so, name what
  `hypha_check_releases` offers, ask, then `hypha_install_release` with `axis: "api"`.
- **Cleaning up**: `hypha_release_status` for what is installed, confirm which releases to drop,
  `hypha_use_release` to move off the default if it is one of them, then `hypha_remove_release` each.
