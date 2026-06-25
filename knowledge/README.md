# knowledge/

The **generated knowledge base** that the Hypha plugin reads at runtime. This directory is
committed so the plugin works without running the generation pipelines; regenerate it whenever
the upstream OMG sources change.

> Do not hand-edit generated files. Change the pipeline (`tools/`) or the source (`sources/`)
> and regenerate. Curated content (currently only parts of `textual-notation/`) is the exception
> and is marked as such.

## Layout

```
knowledge/
├── kerml/
│   ├── index.md            Manifest: every KerML metaclass → its element file
│   └── elements/           One markdown file per KerML metaclass
├── sysml2/
│   ├── index.md            Manifest: every SysML v2 metaclass → its element file
│   └── elements/           One markdown file per SysML v2 metaclass
├── spec/
│   ├── kerml/              KerML 1.0 spec text, segmented by clause
│   └── sysml2/             SysML v2 (Parts 1–2) spec text, segmented by clause
└── textual-notation/
    ├── index.md            Grammar summary + entry point
    └── examples/           Worked SysML v2 / KerML textual-notation examples
```

## How it's generated

| Output | Source | Pipeline |
| --- | --- | --- |
| `kerml/`, `sysml2/` | `sources/xmi/*.kermlx`, `*.sysmlx` | `tools/metamodel-gen` (C# / uml4net) |
| `spec/` | `sources/specs/*.pdf` | `tools/spec-extract` (Python) |
| `textual-notation/` | `sources/textual/`, grammar (`bnf/`) | curated + scripted |

## Element file convention (`kerml/elements`, `sysml2/elements`)

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

Every entry is traceable to the source XMI.
