# knowledge/

The **generated knowledge base** that the Hypha plugin reads at runtime, one folder per release tag.
Nothing per-release here is committed to this repository any more — a plugin install fetches and
generates it on the user's own machine, on request. See the root [`CLAUDE.md`](../CLAUDE.md)'s
"Committed vs git-ignored" for the authoritative list of what *is* committed (schemas and shared
inputs only) and [`sources/README.md`](../sources/README.md) for how a release is fetched.

> Do not hand-edit generated files. Change the pipeline (`tools/`) or the source (`sources/`)
> and regenerate. Curated content (currently only parts of `textual-notation/`) is the exception
> and is marked as such.

## Layout

```
knowledge/
├── versions.json           Installed release tags + which is the default        (local, git-ignored)
├── cross-references.schema.json, model-library.schema.json                      (committed)
└── <tag>/                  e.g. 2026-05                                         (local, git-ignored)
    ├── metamodel/          Combined KerML + SysML v2 metamodel (one tree)
    │   ├── index.md        Manifest: every metaclass (+ enumeration, primitive type) → its element file
    │   └── elements/       One markdown file per element (metaclasses, enumerations, primitive types)
    ├── spec/
    │   ├── kerml/          KerML 1.0 spec text, segmented by clause
    │   └── sysml2/         SysML v2 (Parts 1–2) spec text, segmented by clause
    ├── textual-notation/
    │   ├── index.md        Grammar summary + entry point
    │   └── examples/       Worked SysML v2 / KerML textual-notation examples
    └── cross-references.json
```

## How it's generated

| Output | Source | Pipeline |
| --- | --- | --- |
| `<tag>/metamodel/` | `sources/<tag>/xmi/*.uml` (full KerML + SysML metamodel) | `tools/metamodel-gen` (C# / uml4net) |
| `<tag>/spec/` | `sources/<tag>/specs/*.pdf` | `tools/spec-extract` (Python) — git-ignored, needs a maintainer checkout |
| `<tag>/textual-notation/` | `sources/<tag>/textual/`, grammar (`bnf/`) | `tools/knowledge-gen` (C#) |
| `<tag>/cross-references.json` | the metamodel index + generated examples | `tools/knowledge-gen` (C#) |

The exact upstream source, version and commit of each input (and its license) are recorded in
[`sources/README.md`](../sources/README.md#provenance), so a regenerated knowledge base is traceable
to a specific specification version.

> `spec/` holds verbatim OMG specification text (OMG licensing forbids redistributing it) and needs a
> maintainer source checkout of `tools/spec-extract` to generate even locally — every other `<tag>/`
> subfolder is a plain `hypha generate --tag <tag>` away. Neither is committed: see `CLAUDE.md`.

## Element file convention (`metamodel/elements`)

One file per metaclass, named `<MetaclassName>.md`. Each file opens with YAML front matter
(`name`, `package`, `fully qualified name`, `isAbstract`, `visibility`, `generalizes`,
`specializedBy`) for machine-readable lookup, followed by:

- the metaclass documentation;
- **Generalizations** (direct supertypes) and **Specializations** (direct subtypes), cross-linked;
- **Owned features** — one heading per feature (a stable anchor), followed by a signature line
  giving the visibility sigil (`+`/`-`/`#`/`~`), the type (linked to its element file when the type
  is a generated metaclass), the multiplicity, and italic modifiers (`derived`, `composite`,
  `ordered`); then the documentation and any redefinitions/subsettings, each rendered as a link to
  the feature it refines (a same-file `#anchor` when owned locally, `Owner.md#feature` when
  inherited);
- **Inherited features** — a compact table of the effective feature set inherited from supertypes,
  each row giving the (linked) type, multiplicity, declaring owner, and modifiers, so the reader
  sees the whole feature set without walking the generalization chain;
- **Constraints** — the constraint intent text plus its OCL body.

Enumerations and primitive types referenced as feature types get their own element files too (same
folder, `kind: enumeration` / `kind: primitive` in the front matter), so those types resolve to a
real link. An enumeration file lists its literals (with documentation, in declaration order); a
primitive-type file carries its documentation.

Every entry is traceable to the source XMI.
