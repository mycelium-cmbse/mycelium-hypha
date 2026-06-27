# sources/

Raw, unprocessed inputs to the generation pipelines.

- **`xmi/` is committed** — the XMI / UML metamodel files are version-controlled so the
  `metamodel-gen` tests are reproducible.
- **`specs/` and `textual/` are git-ignored** (see the repository `.gitignore`) and must be
  obtained separately; only this README and `.gitkeep` placeholders are tracked there.

> ⚠️ The OMG PDF specifications are copyrighted. Do **not** commit them to this repository.

## Layout

```
sources/
├── xmi/        OMG / pilot XMI metamodel + normative model libraries
├── specs/      OMG PDF specifications
└── textual/    SysML v2 / KerML textual-notation source material
```

## Where to get the inputs

Inputs come from the SysML v2 submission team's repositories — the metamodel XMI from
[Systems-Modeling/SysML-v2-Pilot-Implementation](https://github.com/Systems-Modeling/SysML-v2-Pilot-Implementation)
and the PDF specifications, grammar and textual examples from
[Systems-Modeling/SysML-v2-Release](https://github.com/Systems-Modeling/SysML-v2-Release).
See **Provenance** below for exact commits.

### `xmi/` — metamodel (→ `tools/metamodel-gen`) — committed
- The SysML v2 metamodel (`SysML_only_xmi.uml`), the KerML metamodel it imports
  (`KerML_only_xmi.uml`) and the referenced primitive types (`PrimitiveTypes.xmi`), committed so the
  generator tests run reproducibly (incl. in CI).
- These are EPL-2.0 artifacts from the release repo (see **Provenance** below); committing them is
  permitted. The model version is `20250201` (per each file's `URI` attribute).

### `specs/` — PDF specifications (→ `tools/spec-extract`)
- From `doc/` in the release repo: KerML 1.0, SysML v2 (Parts 1–2), Systems Modeling API & Services 1.0.
- Drop the PDFs here. (Kept out of git for copyright reasons.)

### `textual/` — textual-notation material (→ `knowledge/textual-notation`)
- Grammar from `bnf/`, and textual examples from `kerml/` and `sysml/` in the release repo.

## Provenance

Each input is traceable to a specific upstream snapshot, so a regenerated `knowledge/` base maps to a
specific specification version. There are two upstreams:

> **XMI** — [`Systems-Modeling/SysML-v2-Pilot-Implementation`](https://github.com/Systems-Modeling/SysML-v2-Pilot-Implementation)
> @ commit `22dbbc30d13e5cde39a2d69cbd68cab895d6c9e1` (EPL-2.0)
>
> **PDFs** — [`Systems-Modeling/SysML-v2-Release`](https://github.com/Systems-Modeling/SysML-v2-Release)
> @ commit `cd99f7ca70b96abb38f09dfd25725e3cf259baa3` (OMG-copyrighted; not committed)

| Input | File(s) | Version | OMG document | Upstream / License |
| --- | --- | --- | --- | --- |
| SysML v2 metamodel | `xmi/SysML_only_xmi.uml` | model URI `…/SysML/20250201` | — | Pilot-Implementation, EPL-2.0 |
| KerML metamodel | `xmi/KerML_only_xmi.uml` | model URI `…/KerML/20250201` | — | Pilot-Implementation, EPL-2.0 |
| Primitive types | `xmi/PrimitiveTypes.xmi` | (imported by the above) | — | Pilot-Implementation, EPL-2.0 |
| KerML spec PDF | `specs/1-Kernel_Modeling_Language.pdf` | 1.0 | `formal/26-03-01` | Release, OMG (not committed) |
| SysML spec PDF | `specs/2a-OMG_Systems_Modeling_Language.pdf` | 2.0 | `formal/26-03-02` | Release, OMG (not committed) |
| API & Services PDF | `specs/3-Systems_Modeling_API_and_Services.pdf` | 1.0 | `formal/26-03-04` | Release, OMG (not committed) |

The XMI is EPL-2.0 and committed; the OMG PDFs are copyrighted and git-ignored (the OMG license
forbids redistributing them — see the repository [NOTICE](../NOTICE) for the full attribution and
license terms). When updating any input, bump the relevant upstream commit above and the affected
version/document numbers.
