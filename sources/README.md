# sources/

Raw, unprocessed inputs to the generation pipelines, **one folder per upstream release tag**.

- **`<tag>/xmi/` and `<tag>/textual/` are committed** for the releases in the rolling window, so the
  pipelines are reproducible.
- **`<tag>/specs/` is git-ignored** (the OMG PDF specifications are copyrighted — see the repository
  `.gitignore`, which also carries a global `*.pdf` backstop) and must be obtained separately.
- **`PrimitiveTypes.xmi` is shared** across every release.

> ⚠️ The OMG PDF specifications are copyrighted. Do **not** commit them to this repository.

## Layout

```
sources/
├── PrimitiveTypes.xmi   OMG UML primitives library, shared by every release
└── <tag>/               e.g. 2026-05
    ├── xmi/             OMG / pilot XMI metamodel
    ├── specs/           OMG PDF specifications          (git-ignored)
    └── textual/         SysML v2 / KerML textual-notation source material
```

## The tag is the version

A hypha version is one **tag name that resolves in both upstreams**. The tag is the only identifier
used: the model URI inside the XMI (`…/SysML/20250201`) is ignored, because it tracks neither the
release nor the content — the 2026-05 metamodel still declares a 2025 URI. The same model may
therefore appear under several tags, which is accepted.

The two repositories are not in lockstep, so the offerable versions are the **intersection** of their
tags. Discovery, commit resolution and fetching live in `tools/knowledge-gen` (`Hypha.Knowledge`,
namespace `Hypha.Knowledge.Releases`); which files make up a release is stated in `ReleaseInputs`.

## Where the inputs come from

| Input | Upstream | Path at the tag | License |
| --- | --- | --- | --- |
| SysML v2 metamodel | Pilot-Implementation | `org.omg.sysml/model/SysML_only_xmi.uml` | EPL-2.0, committed |
| KerML metamodel | Pilot-Implementation | `org.omg.sysml/model/KerML_only_xmi.uml` | EPL-2.0, committed |
| Primitive types | — | shared `sources/PrimitiveTypes.xmi` | OMG UML library, committed |
| Specification PDFs | Release | `doc/*.pdf` | OMG, **git-ignored** |
| Textual grammar | Release | `bnf/*.kebnf`, `*.kgbnf` | EPL-2.0, committed |
| Textual examples | Release | `kerml/`, `sysml/` | EPL-2.0, committed |
| Standard model libraries | Release | `sysml.library/**/*.sysml`, `*.kerml` | EPL-2.0, committed |

`PrimitiveTypes.xmi` is published by neither upstream: it is the OMG UML primitives library
(`…/PrimitiveTypes/20161101`), referenced through a path map and identical for every release, so it
is stored once rather than duplicated per tag.

## Installed releases

`knowledge/versions.json` is the authoritative record of which releases this checkout carries, which
one is the default, and the upstream commit each tag resolved to (traceability only — the tag is the
identifier). Fetching another release writes a new `sources/<tag>/` folder; only the window's tags are
committed.

The OMG PDFs are downloaded by default now (`hypha fetch --tag <release>`, opt out with
`--no-specs`) - a fully local install needs everything a plain HTTP call can get it, though *quoting*
their text still needs a maintainer source checkout (see `tools/spec-extract/README.md`). The
plugin's `SessionStart` hook (`Hypha.Tools.Hook`, see `tools/hypha-cli/README.md`'s "Automatic version
check" section) still names the exact files and tagged URLs when they're missing for the default
release, folded into the same message that reports newer releases.

### What the committed window is for, now that `hypha use`/`remove` let the user manage more

The two releases committed to this repository are the **permanent, offline-working floor**: a fresh
plugin install works immediately, with zero setup and no network access, because they are already
here.

Beyond that floor, the plugin's `SessionStart` hook only ever **compares** what's installed against
what's offerable upstream (`hypha check`) and reports the result - it never fetches or generates
anything itself. Getting an additional release, older or newer, is always something the user asks for
(conversationally, through the `version-management` skill, or directly): `hypha fetch --tag <tag>` +
`hypha generate --tag <tag>` lands it as an ordinary, **untracked** `knowledge/<tag>/` +
`sources/<tag>/` - visible in `git status`, never gitignored - because it is not part of the committed
floor. `hypha use --tag <tag>` switches which installed release every skill answers from;
`hypha remove --tag <tag>` drops one no longer wanted (refusing on the current default or the last
release without `--force`). A maintainer can `git add` an untracked release to promote it into the
committed window through the normal `hypha move-window` flow.

Two gaps are permanent, by design, rather than bugs to fix: the hook only runs on the four RIDs
`.github/workflows/release.yml` publishes (`win-x64`, `linux-x64`, `osx-x64`, `osx-arm64`), and the
plugin's dispatch shim needs a POSIX-compatible shell (present via Git Bash on essentially every
Windows development machine, but not guaranteed) to pick the right platform binary.

## Licensing

The XMI and textual sources are EPL-2.0 and committed; the OMG PDFs are copyrighted and git-ignored
(the OMG license forbids redistributing them — see the repository [NOTICE](../NOTICE) for the full
attribution and license terms).
