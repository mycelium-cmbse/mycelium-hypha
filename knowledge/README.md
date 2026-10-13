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

The Systems Modeling API & Services knowledge sits beside the language releases, not inside one: it
is versioned on its own axis, with its own manifest and default.

```
knowledge/
└── api/                                                                         (local, git-ignored)
    ├── versions.json       Installed API versions + which is the default
    └── <version>/          e.g. 2026-04 – a tag of the API-Services repository
        ├── pim/            The Platform Independent Model, in the shape of metamodel/
        ├── openapi/        The REST/HTTP PSM: one page per API schema, every endpoint, an index
        ├── spec/           API specification text, segmented by clause
        └── cross-references.json   PIM type ↔ clause ↔ PSM schema, PIM operation ↔ endpoint
```

## How it's generated

| Output | Source | Pipeline |
| --- | --- | --- |
| `<tag>/metamodel/` | `sources/<tag>/xmi/*.uml` (full KerML + SysML metamodel) | `tools/metamodel-gen` (C# / uml4net) |
| `<tag>/spec/` | `sources/<tag>/specs/*.pdf` | `tools/spec-extract` (Python), run through `uv` on demand — git-ignored |
| `<tag>/textual-notation/` | `sources/<tag>/textual/`, grammar (`bnf/`) | `tools/knowledge-gen` (C#) |
| `<tag>/cross-references.json` | the metamodel index + generated examples | `tools/knowledge-gen` (C#) |
| `api/<version>/pim/` | `sources/api/<version>/xmi/Systems-Modeling-API.xmi` | `tools/metamodel-gen` (C# / uml4net) |
| `api/<version>/openapi/` | `sources/api/<version>/openapi/openapi.json` | `tools/knowledge-gen` (C#) |
| `api/<version>/spec/` | `sources/api/<version>/specs/*.pdf` | `tools/spec-extract` (Python), run through `uv` on demand — git-ignored |
| `api/<version>/cross-references.json` | the three API artifacts above | `tools/knowledge-gen` (C#) |

The exact upstream source, version and commit of each input (and its license) are recorded in
[`sources/README.md`](../sources/README.md#provenance), so a regenerated knowledge base is traceable
to a specific specification version.

> `spec/` holds verbatim OMG specification text (OMG licensing forbids redistributing it), but like
> every other `<tag>/` subfolder it is now a plain `hypha generate --tag <tag>` away: `SpecGenerator`
> fetches and caches `uv`, which resolves or fetches a matching Python itself and runs
> `tools/spec-extract` through it - no maintainer source checkout needed. Neither is committed: see
> `CLAUDE.md`.

## The API knowledge (`api/<version>/`)

`hypha api generate` writes it; the `api-lookup` skill reads it.

- **`pim/`** has the layout and page anatomy of `metamodel/` (see below) – the PIM is a UML model
  too. Its classes are not metaclasses, so the pages say *class*; the six services are classes whose
  content is their operations; and it declares an interface (`Data`, with a **Realized by** list) and
  data types (`UUID`, `IRI`, `ISO8601DateTime`), which the metamodels do not.
- **`openapi/schemas/<Name>.md`** – one page per schema of the API itself: a `Property | Type |
  Required | Nullable | Refers to` table, and a `role` saying whether the endpoints use it in request
  bodies, in responses or both. The role is computed from the endpoints, not read off the name. The
  schemas that mirror KerML / SysML metamodel elements are indexed by name in `openapi/index.json`
  and not rendered. `openapi/endpoints.md` lists every operation.
- **`cross-references.json`** – per PIM type: the clauses that cover it, its PSM response and request
  schemas, each effective PIM attribute with whether the response and the request model carry it,
  and the PSM-only members. Per PIM service operation: its clause and the endpoints the
  specification maps it to. Every edge names the `method` that produced it, so a mapping the
  specification states can be told from one inferred from a shared name. Like the language
  cross-references it holds identifiers only, **never clause text**.

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
- **Operations** – each owned operation's signature, documentation and the OCL body of its
  `bodyCondition`;
- **Constraints** — the constraint intent text plus its OCL body.

Enumerations and primitive types referenced as feature types get their own element files too (same
folder, `kind: enumeration` / `kind: primitive` in the front matter), so those types resolve to a
real link. An enumeration file lists its literals (with documentation, in declaration order); a
primitive-type file carries its documentation.

Every entry is traceable to the source XMI.
