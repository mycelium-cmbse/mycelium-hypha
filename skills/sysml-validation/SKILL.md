---
name: sysml-validation
description: Validate SysML v2 (or KerML) textual notation for syntactic and structural correctness, and explain any violations with fixes. Use when the user provides SysML v2 textual notation to check, or asks whether a snippet is valid.
---

# SysML v2 validation

Check SysML v2 / KerML textual notation against the documented grammar and metamodel, then report
findings with locations and corrections.

## Knowledge base

- `knowledge/textual-notation/index.md` – the **grammar summary** (declaration forms, membership &
  visibility, relationship operators) and **keyword reference**. Start here for syntax rules.
- `knowledge/textual-notation/examples/` – worked valid examples (each links the metamodel elements it
  uses); `knowledge/textual-notation/fixtures/` – the valid/invalid regression suite.
- `knowledge/sysml2/` – the combined KerML + SysML v2 metamodel: `elements/<Metaclass>.md` for
  structural constraints, `index.json` (or `index.md`) for fast name → element lookup.
- `knowledge/spec/` – normative clauses to cite. **Git-ignored / generated locally** (see
  `tools/spec-extract`); if it is empty, cite the metamodel/grammar instead and note the spec text is
  unavailable rather than inventing a clause.

## What to check

**Syntax** (against the grammar reference)
- Every declaration is terminated by `;` or has a `{ }` body; braces/brackets/parentheses balance.
- Names are not reserved keywords unless quoted (`'name'`); keywords are spelled and placed per the reference.
- Relationship operators are correct: `:` (typing / defined by), `:>` (specializes / subsets), `:>>`
  (redefines), `::>` (references), `=>` (crosses). `=` binds a value, not a type.
- Multiplicity is well-formed (`[lower..upper]`, `[n]`, `[*]`).

**Structural / semantic** (against the metamodel)
- A usage is typed by a *definition* of the right kind (e.g. a `part` by a `part def`).
- `redefines` / `subsets` / specialization targets resolve to a feature or type reachable through a
  specialized type; multiplicity bounds satisfy `lower <= upper`.
- Member names are unique within their namespace; required features/parameters are present.

## Procedure

1. Parse structurally: declarations, memberships, relationships, keywords (per the grammar summary).
2. Run the syntax checks, then the structural checks against `knowledge/sysml2/elements/`.
3. For each issue report **location**, the **violated rule**, **why** it is wrong, and a **corrected
   snippet**; add a knowledge-base **reference** (metamodel element or spec clause) for non-obvious rulings.
4. If nothing is wrong, state that the notation is valid.

The `knowledge/textual-notation/fixtures/` suite is the reference for expected outcomes: valid fixtures
should yield no findings; each invalid fixture documents the finding to produce.

## Limits

This reasons from the documented grammar and metamodel; it is **not** the pilot-implementation
compiler. When a definitive ruling needs the real parser, say so rather than guessing.

For larger inputs, delegate to the `sysml-validator` subagent.
