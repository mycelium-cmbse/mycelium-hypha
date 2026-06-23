# sources/

Raw, unprocessed inputs to the generation pipelines. **These files are git-ignored** (see the
repository `.gitignore`) and must be obtained separately — only this README and `.gitkeep`
placeholders are tracked.

> ⚠️ The OMG PDF specifications are copyrighted. Do **not** commit them to this repository.

## Layout

```
sources/
├── xmi/        OMG / pilot XMI metamodel + normative model libraries
├── specs/      OMG PDF specifications
└── textual/    SysML v2 / KerML textual-notation source material
```

## Where to get the inputs

Most inputs come from the SysML v2 submission team's release repository:
[Systems-Modeling/SysML-v2-Release](https://github.com/Systems-Modeling/SysML-v2-Release).

### `xmi/` — metamodel & libraries (→ `tools/metamodel-gen`)
- Normative model libraries in Eclipse XMI: `sysml.library.xmi/` (`.kermlx`, `.sysmlx`).
- The variant with derived properties/implied relationships: `sysml.library.xmi.implied/`.
- Place the `.kermlx` / `.sysmlx` files (or the metamodel XMI) here.

### `specs/` — PDF specifications (→ `tools/spec-extract`)
- From `doc/` in the release repo: KerML 1.0, SysML v2 (Parts 1–2), Systems Modeling API & Services 1.0.
- Drop the PDFs here. (Kept out of git for copyright reasons.)

### `textual/` — textual-notation material (→ `knowledge/textual-notation`)
- Grammar from `bnf/`, and textual examples from `kerml/` and `sysml/` in the release repo.

## Reproducibility

Record exactly which upstream release/tag each input came from (e.g. in a commit message or a
small manifest) so a regenerated `knowledge/` base is traceable to a specific spec version.
