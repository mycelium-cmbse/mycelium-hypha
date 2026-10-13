# sources/

Raw, unprocessed inputs to the generation pipelines, **one folder per upstream release tag**.

- **`<tag>/` is entirely git-ignored** — `xmi/`, `textual/` and `specs/` alike. Nothing per-release is
  committed to this repository any more (see the root `CLAUDE.md`'s "Committed vs git-ignored"); every
  release is fetched on request, onto the user's own machine, via `hypha fetch --tag <tag>`.
- **`specs/` holds the OMG PDF specifications**, copyrighted and never committed regardless (see the
  repository `.gitignore`, which also carries a global `*.pdf` backstop).
- **`PrimitiveTypes.xmi` is shared** across every release, and stays committed at this root — it is
  tag-independent, not part of any one release's fetch.

> ⚠️ The OMG PDF specifications are copyrighted. Do **not** commit them to this repository.

## Layout

```
sources/
├── PrimitiveTypes.xmi   OMG UML primitives library, shared by every release  (committed)
└── <tag>/               e.g. 2026-05                                        (git-ignored, all of it)
    ├── xmi/             OMG / pilot XMI metamodel
    ├── specs/           OMG PDF specifications
    └── textual/         SysML v2 / KerML textual-notation source material
```

The Systems Modeling API & Services inputs sit beside the releases, under their own version:

```
sources/
└── api/                                                                     (git-ignored, all of it)
    └── <version>/       e.g. 2026-04
        ├── openapi/     the OpenAPI document (the REST/HTTP PSM)
        ├── xmi/         the PIM model, and the KerML model it refers to
        └── specs/       the OMG API specification PDF
```

## The tag is the version

A hypha version is one **tag name that resolves in both upstreams**. The tag is the only identifier
used: the model URI inside the XMI (`…/SysML/20250201`) is ignored, because it tracks neither the
release nor the content — the 2026-05 metamodel still declares a 2025 URI. The same model may
therefore appear under several tags, which is accepted.

The two repositories are not in lockstep, so the offerable versions are the **intersection** of their
tags. Discovery, commit resolution and fetching live in `tools/knowledge-gen` (`Hypha.Knowledge`,
namespace `Hypha.Knowledge.Releases`); which files make up a release is stated in `ReleaseInputs`.

## The API has its own version

The Systems Modeling API is not part of a language release. An **API version** is one tag of the
API-Services repository that is also a language release tag – so its specification PDF and the KerML
model its PIM refers to can be read from the language repositories at the same tag. The
API-Services repository is tagged far less often, so the offerable API versions are a subset of the
language releases, and the newest one is usually older than the newest release.

`hypha api fetch --tag <version>` writes `sources/api/<version>/` and records the version in
`knowledge/api/versions.json`, a manifest of its own: an API version is installed, made the default
and removed independently of any language release. Which files make it up is stated in `ApiUpstream`
and `ApiInputs` (`tools/knowledge-gen`, namespace `Hypha.Knowledge.Api`).

## Where the inputs come from

| Input | Upstream | Path at the tag | License |
| --- | --- | --- | --- |
| SysML v2 metamodel | Pilot-Implementation | `org.omg.sysml/model/SysML_only_xmi.uml` | EPL-2.0, git-ignored |
| KerML metamodel | Pilot-Implementation | `org.omg.sysml/model/KerML_only_xmi.uml` | EPL-2.0, git-ignored |
| Primitive types | — | shared `sources/PrimitiveTypes.xmi` | OMG UML library, committed |
| Specification PDFs | Release | `doc/*.pdf` | OMG, **git-ignored** |
| Textual grammar | Release | `bnf/*.kebnf`, `*.kgbnf` | EPL-2.0, git-ignored |
| Textual examples | Release | `kerml/`, `sysml/` | EPL-2.0, git-ignored |
| Standard model libraries | Release | `sysml.library/**/*.sysml`, `*.kerml` | EPL-2.0, git-ignored |
| API: OpenAPI document | API-Services | `public/docs/openapi.json` | EPL-2.0, git-ignored |
| API: specification PDF | Release | `doc/3-Systems_Modeling_API_and_Services.pdf` | OMG, **git-ignored** |
| API: KerML model the PIM refers to | Pilot-Implementation | `org.omg.sysml/model/KerML_only_xmi.uml` | EPL-2.0, git-ignored |
| API: PIM model | OMG document server | `ptc/25-02-29`, `Systems-Modeling-API.xmi` | OMG, **git-ignored** |

API-Services is [Systems-Modeling/SysML-v2-API-Services](https://github.com/Systems-Modeling/SysML-v2-API-Services).
Its OpenAPI document is byte-identical to the OMG machine-readable file `ptc/25-02-30`.

The **PIM model is the one input not read from a repository**: none of the Systems-Modeling
repositories carries it, so it is fetched from
`https://www.omg.org/spec/SystemsModelingAPI/20250201/Systems-Modeling-API.xmi`, where it is the same
file for every API version. The manifest records its OMG document identifier instead of a commit.

`PrimitiveTypes.xmi` is published by neither upstream: it is the OMG UML primitives library
(`…/PrimitiveTypes/20161101`), referenced through a path map and identical for every release, so it
is stored once rather than duplicated per tag.

## Installed releases

`knowledge/versions.json` is the authoritative record of which releases this checkout carries, which
one is the default, and the upstream commit each tag resolved to (traceability only — the tag is the
identifier). It is itself git-ignored now: it records *local* installation state, not something every
checkout starts from. Fetching a release writes a new `sources/<tag>/` folder; nothing about it is
committed.

The OMG PDFs are downloaded by default now (`hypha fetch --tag <release>`, opt out with
`--no-specs`) - a fully local install needs everything a plain HTTP call can get it. *Quoting* their
text is `hypha generate`'s `spec` artifact (see `tools/spec-extract/README.md`), which fetches and
caches `uv` to run it - no maintainer source checkout needed any more. The plugin's `SessionStart` hook
(`Hypha.Tools.Hook`, see `tools/hypha-cli/README.md`'s "Automatic version check" section) still names
the exact files and tagged URLs when they're missing for the default
release, folded into the same message that reports newer releases.

### One uniform model, entirely local

A fresh plugin install starts with nothing fetched: no committed floor means no offline-zero-setup
release either. The `SessionStart` hook only ever **compares** what's installed locally against what's
offerable upstream (`hypha check`) and reports the result - it never fetches or generates anything
itself. Getting a release, first one or another, is always something the user asks for
(conversationally, through the `version-management` skill, or directly): `hypha fetch --tag <tag>` +
`hypha generate --tag <tag>` lands it as an ordinary, git-ignored `knowledge/<tag>/` + `sources/<tag>/`
- every release is handled identically, none of them privileged by being committed. `hypha use --tag
<tag>` switches which installed release every skill answers from; `hypha remove --tag <tag>` drops one
no longer wanted (refusing on the current default or the last release without `--force`).

Two gaps are permanent, by design, rather than bugs to fix: the hook only runs on the four RIDs
`.github/workflows/release.yml` publishes (`win-x64`, `linux-x64`, `osx-x64`, `osx-arm64`), and the
plugin's dispatch shim needs a POSIX-compatible shell (present via Git Bash on essentially every
Windows development machine, but not guaranteed) to pick the right platform binary.

## Licensing

The XMI and textual sources are EPL-2.0; the OMG PDFs are copyrighted. Neither is committed to this
repository (the OMG license forbids redistributing the PDFs; the XMI/textual sources are simply no
longer part of what ships in git at all — see the repository [NOTICE](../NOTICE) for the full
attribution and license terms). One release's real XMI is the exception: it is committed as a test
fixture under `tools/metamodel-gen/Hypha.MetamodelGen.Tests/Fixtures/xmi/`, which never ships with the
plugin (see `CLAUDE.md`'s "What this repo is").
