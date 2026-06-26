---
name: metamodel-lookup
description: Look up KerML / SysML v2 metamodel structure — what a metaclass is, its features, supertypes, subtypes, redefinitions, multiplicities, and constraints (plus enumerations and primitive types). Use when the user asks how a SysML v2 or KerML element is defined in the metamodel, or how elements relate.
---

# Metamodel lookup

Answer questions about the structure of the KerML and SysML v2 metamodels using Hypha's
generated knowledge base. Be exact and cite the source file.

## Knowledge base

KerML and SysML v2 are **combined** in one tree (the SysML v2 model is generated from the full
KerML + SysML metamodel, so KerML metaclasses such as `Feature`, `Element` and `Membership` live
here too):

- `knowledge/sysml2/index.md` — manifest: every metaclass grouped by package, plus
  `## Enumeration types` and `## Primitive types` sections. Each entry links to its element file
  and carries a one-line summary.
- `knowledge/sysml2/elements/<Name>.md` — one file per element. This includes metaclasses,
  enumerations (`kind: enumeration`) and primitive types (`kind: primitive`).

## Element file anatomy

YAML front matter: `name`, `package`, `fully qualified name` (e.g.
`SysML::Systems::Actions::AcceptActionUsage`), `isAbstract`, `visibility`, `generalizes`,
`specializedBy`. Then:

- **## Generalizations** / **## Specializations** — direct supertypes / subtypes, each a
  `[Name](Name.md)` link.
- **## Owned features** — one `### name` heading per feature, followed by a signature line:
  `` `+` [Type](Type.md) · `[0..1]` · *derived* `` (visibility sigil, linked type, multiplicity,
  italic modifiers). Documentation follows, then any `Redefines …` / `Subsets …` lines, each a link
  to the feature it refines.
- **## Inherited features** — a table (`Feature | Type | Multiplicity | Owner | Modifiers`) giving
  the **full effective inherited feature set**, with the declaring supertype in the `Owner` column.
  Read this table directly — you do **not** need to walk the generalization chain by hand.
- **## Constraints** — each constraint's intent text plus its OCL body in an ```ocl block.

Enumeration files list their literals under `## Literals`; primitive-type files carry just their
documentation.

## Procedure

1. Resolve the element name in `knowledge/sysml2/index.md` (or `Grep` `knowledge/sysml2/elements/`).
2. Read `knowledge/sysml2/elements/<Name>.md`. Owned features are in **## Owned features**; for the
   full inherited set read the **## Inherited features** table (the `Owner` column says where each
   comes from).
3. Report the metaclass, abstractness, supertypes/subtypes, and the relevant features (name, type,
   multiplicity, modifiers), plus redefinitions/subsettings and constraints. Follow `[Type](Type.md)`
   links to related elements when needed. Always include the source file path.
4. If the element isn't in the knowledge base, say so — never invent structure.

Answer single-element lookups inline (one index read + one element file — the inherited table
already gives the full feature set). Delegate to the `metamodel-navigator` subagent only for
**cross-cutting / fan-out** questions that must scan many element files — e.g. "which metaclasses
have a feature typed by `Expression`", comparing several metaclasses, or tracing a redefinition
across the hierarchy — so the bulk file reading stays out of this context.
