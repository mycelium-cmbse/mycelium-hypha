---
name: sysml-validation
description: Validate SysML v2 / KerML textual notation (.sysml / .kerml, not UML) for syntactic and structural correctness against the grammar and metamodel, and explain any violations with fixes. Use when the user provides SysML v2 or KerML textual notation to check, or asks whether a snippet is valid.
---

# SysML v2 validation

Check SysML v2 / KerML textual notation against the documented grammar and metamodel, then report
findings with locations and corrections.

## Releases

The metamodel and grammar are generated **per upstream release tag** (`YYYY-MM`). Read
`knowledge/versions.json` for the installed tags and the `default` one, then substitute it for
`<tag>` below. Validate against the default release unless the user names another, and **say which
release you validated against** — a construct valid in one release may not be in another.

## Knowledge base

- `knowledge/<tag>/textual-notation/index.md` – the **keyword reference**, read from that release's
  own grammar. Start here for what is and is not a keyword.
- `knowledge/<tag>/textual-notation/examples/` – every model shipped with that release, copied
  verbatim, each linked to the metamodel elements it declares. These are the reference for what
  valid notation looks like: if a construct appears here, it is valid for this release.
- `knowledge/<tag>/metamodel/` – the combined KerML + SysML v2 metamodel: `elements/<Metaclass>.md` for
  structural constraints, `index.json` (or `index.md`) for fast name → element lookup, and
  `metamodel.json` for the structural checks below.
- `knowledge/<tag>/cross-references.json` – element → clause identifiers, BNF production and worked
  example. Committed, so it works without the PDFs.
- `knowledge/<tag>/spec/` – normative clauses to cite. **Git-ignored / generated locally** (see
  `tools/spec-extract`); if it is empty, cite the metamodel/grammar and name the governing clause
  from `cross-references.json` rather than inventing one.

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

### Grounding the structural checks in the graph

Two of these are graph traversals, not pattern matches – decide them against `metamodel.json`
instead of reasoning from prose. `jq` is recommended; without it, read the element file's
**## Inherited features** table, which carries the same effective set.

**Reachability** – is a `redefines` / `subsets` target actually inherited? A feature is reachable
from a metaclass when it appears in that metaclass's `inheritedAttributes` (or its own
`ownedAttributes`); `inheritedFrom` names the declaring type, which is what you cite:

```sh
jq -r '.classes[] | select(.name=="PartUsage")
       | (.ownedAttributes[], .inheritedAttributes[]) | select(.name=="mass") | .name' \
  knowledge/<tag>/metamodel/metamodel.json
```

No output means the feature is not on that type at all – the redefinition cannot resolve. Note the
distinction: in user notation the *declared* type may specialize nothing, in which case there is no
inherited feature to redefine regardless of what the metamodel says.

**Multiplicity bounds** – `lower` and `upper` are typed integers in the graph (`-1` means unbounded),
so `lower > upper` is a comparison rather than a judgement. Apply the same rule to bounds written in
the notation under review: `[2..1]` is ill-formed, `[0..*]` is not.

**What a construct actually populates.** `cross-references.json` carries a `features` array per
element: the metamodel feature each piece of syntax fills, and how (`=` sets, `+=` adds, `?=` sets a
boolean from a keyword's presence), with the productions responsible.

```sh
jq -r '.entries["Comment"].features[] | "\(.feature) \(.operator) via \(.productions|join(", "))"' \
  knowledge/<tag>/cross-references.json
```

Use it to explain a finding in the model's terms rather than the notation's — "this sets
`declaredName`, which is `[0..1]`" is a better explanation than "this looks wrong". An empty list
means the element adds no syntax of its own and inherits its declaration; say so rather than
treating it as unknown.

For either finding, `cross-references.json` gives the clause to point at:

```sh
jq -r '.entries["MultiplicityRange"].clauses[] | "\(.document) \(.clause)"' knowledge/<tag>/cross-references.json
```

Cite that clause as a *reference* (`DERIVED` – matched by name), and quote it only if
`knowledge/<tag>/spec/` is present.

## Procedure

1. Parse structurally: declarations, memberships, relationships, keywords (per the keyword reference).
2. Run the syntax checks, then the structural checks – reachability and multiplicity bounds against
   `metamodel.json` as above, the rest against `knowledge/<tag>/metamodel/elements/`.
3. For each issue report **location**, the **violated rule**, **why** it is wrong, and a **corrected
   snippet**; add a knowledge-base **reference** (metamodel element or spec clause) for non-obvious rulings.
4. If nothing is wrong, state that the notation is valid.

`knowledge/<tag>/textual-notation/examples/` is the reference for valid notation: every model shipped
with the release, verbatim. A construct that appears there is valid for that release; if the input
resembles one of them, compare against it rather than reasoning from the grammar alone.

## Limits

This reasons from the documented grammar and metamodel; it is **not** the pilot-implementation
compiler. When a definitive ruling needs the real parser, say so rather than guessing.

For larger inputs, delegate to the `sysml-validator` subagent.
