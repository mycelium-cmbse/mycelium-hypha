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
tags. Discovery and fetching live in `tools/spec-extract` (`spec_extract.versions`, `spec_extract.fetch`).

## Where the inputs come from

| Input | Upstream | Path at the tag | License |
| --- | --- | --- | --- |
| SysML v2 metamodel | Pilot-Implementation | `org.omg.sysml/model/SysML_only_xmi.uml` | EPL-2.0, committed |
| KerML metamodel | Pilot-Implementation | `org.omg.sysml/model/KerML_only_xmi.uml` | EPL-2.0, committed |
| Primitive types | — | shared `sources/PrimitiveTypes.xmi` | OMG UML library, committed |
| Specification PDFs | Release | `doc/*.pdf` | OMG, **git-ignored** |
| Textual grammar | Release | `bnf/*.kebnf`, `*.kgbnf` | EPL-2.0, committed |
| Textual examples | Release | `kerml/`, `sysml/` | EPL-2.0, committed |

`PrimitiveTypes.xmi` is published by neither upstream: it is the OMG UML primitives library
(`…/PrimitiveTypes/20161101`), referenced through a path map and identical for every release, so it
is stored once rather than duplicated per tag.

## Installed releases

`knowledge/versions.json` is the authoritative record of which releases this checkout carries, which
one is the default, and the upstream commit each tag resolved to (traceability only — the tag is the
identifier). Fetching another release writes a new `sources/<tag>/` folder; only the window's tags are
committed.

The OMG PDFs must be obtained per release. A plugin **SessionStart** hook
(`hooks/check-spec-pdfs.py`) checks the default release and names the exact files and tagged URLs
when any are missing.

## Licensing

The XMI and textual sources are EPL-2.0 and committed; the OMG PDFs are copyrighted and git-ignored
(the OMG license forbids redistributing them — see the repository [NOTICE](../NOTICE) for the full
attribution and license terms).
