---
name: metamodel-lookup
description: Look up KerML / SysML v2 metamodel structure — what a metaclass is, its features, supertypes, redefinitions, multiplicities, and constraints. Use when the user asks how a SysML v2 or KerML element is defined in the metamodel, or how elements relate.
---

# Metamodel lookup

Answer questions about the structure of the KerML and SysML v2 metamodels using Hypha's
generated knowledge base. Be exact and cite the source file.

## Knowledge base

- `knowledge/kerml/index.md` — index of all KerML metaclasses.
- `knowledge/sysml2/index.md` — index of all SysML v2 metaclasses.
- `knowledge/{kerml,sysml2}/elements/<Name>.md` — one file per metaclass.

## Procedure

1. Resolve the element name from the relevant `index.md` (handle KerML vs SysML v2 — SysML v2
   metaclasses often specialise KerML ones).
2. Read the element file. For "all features including inherited", walk the generalization chain.
3. Report metaclass, abstractness, supertypes, and the relevant features (name : type [mult]),
   plus any redefinitions/subsettings and constraints. Always include the source file path.
4. If the element isn't in the knowledge base, say so — never invent structure.

For deep or multi-element traversals, delegate to the `metamodel-navigator` subagent.
