---
name: sysml-validation
description: "Validate SysML v2 / KerML textual notation (.sysml / .kerml, not UML) for syntactic and structural correctness against the grammar and metamodel, and explain any violations with fixes, from a snippet to a whole model. Use when the user provides SysML v2 or KerML textual notation to check, or asks whether a snippet is valid."
---

# SysML v2 validation

Check SysML v2 / KerML textual notation against the release's grammar and metamodel, read through the
**hypha MCP server's tools**, then report findings with locations and corrections. The tools are named
here without a client's prefix: a client may show `hypha_syntax` as, for example,
`mcp__hypha__hypha_syntax`.

## Which release

The metamodel and grammar are generated per upstream release tag (`YYYY-MM`).

- Every tool answers from the **default** release unless it is given a `tag`. Call
  `hypha_release_status` when unsure what is installed.
- **Say which release you validated against**: a construct valid in one release may not be in
  another.
- If the user names a release that is not installed, say so and name the installed ones.

## Tools

The grammar, `hypha_syntax` with the `question`:

| Question | `question` | Arguments |
| --- | --- | --- |
| Is a word a keyword or operator, of which kind, and which productions and metaclasses write it | `keyword` | `word`; `grammar: "kerml"` or `grammar: "sysml"` for one grammar |
| The elements a grammar production builds | `built-by` | `production`; `grammar` |
| Which elements' syntax fills in a metamodel feature, and through which operator | `feature-assignments` | `feature`; `operator` |
| An element's clauses, productions, the features its syntax fills in, and its worked examples | `cross-reference` | `element` |

The metamodel, `hypha_metaclass` with the `question`:

| Question | `question` | Arguments |
| --- | --- | --- |
| What a metaclass is: abstract or not, its supertypes | `element` | `name` |
| A feature on a metaclass, owned or inherited, with its type, multiplicity and owner | `features` | `name`, `feature` |
| Does one metaclass specialize another | `specializes` | `name`, `general` |
| The constraints a model of it must satisfy | `constraints` | `name` |

And `hypha_search` with `text` and `kind: "metamodel"` to find the metaclass a construct builds when
the name is not exact. For quoting the specification behind a ruling, use the `spec-citation` skill.

## What to check

**Syntax**, against the grammar:

- Every declaration is terminated by `;` or has a `{ }` body; braces, brackets and parentheses
  balance.
- A name is not a reserved keyword unless it is quoted (`'name'`): `hypha_syntax` `keyword` gives a
  word's `kind` (`reserved`, `contextual` or `operator`) per grammar, and a `null` value means it is
  no keyword or operator of the release. A keyword is written where a production that writes it allows.
- Relationship operators are correct: `:` (typing or defined by), `:>` (specializes or subsets), `:>>`
  (redefines), `::>` (references), `=>` (crosses). `=` binds a value, not a type.
- A multiplicity is well-formed: `[lower..upper]`, `[n]`, `[*]`.

**Structure**, against the metamodel:

- A usage is typed by a *definition* of the right kind (a `part` by a `part def`).
- A `redefines` / `subsets` / specialization target resolves to a feature or type reachable through a
  specialized type.
- Multiplicity bounds satisfy `lower <= upper`; member names are unique within their namespace;
  required features and parameters are present.

### Deciding the structural checks from the graph

Two of these are graph questions, not pattern matches; decide them from the tools rather than from
prose:

- **Reachability**: is a `redefines` / `subsets` target inherited? `hypha_metaclass` `features` with
  `name` and `feature` answers with the feature when the metaclass has it, owned or inherited, and an
  empty list when it does not, so the redefinition cannot resolve. Cite the owner it names
  (`inheritedFrom`, or the metaclass itself); when `redefinedBy` is set, the redefining feature is the
  one that applies. In user notation the *declared* type may specialize nothing, and then there is no
  inherited feature to redefine whatever the metamodel says.
- **Multiplicity bounds**: `lower` and `upper` are integers, `-1` meaning unbounded (write it `*`), so
  `lower > upper` is a comparison, not a judgement. Apply the same rule to bounds in the notation under
  review: `[2..1]` is ill-formed, `[0..*]` is not.

**What a construct fills in.** `hypha_syntax` `cross-reference` lists, per element, the metamodel
features its syntax fills in and how (`=` sets, `+=` adds, `?=` sets a boolean from a keyword's
presence), with the productions responsible. Use it to explain a finding in the model's terms rather
than the notation's: "this sets `declaredName`, which is `[0..1]`" explains more than "this looks
wrong". An element whose syntax fills in nothing adds no syntax of its own and inherits its
declaration: say so rather than treating it as unknown.

The same answer names the clauses that treat the element, each with its provenance: a clause the
grammar states (`MODEL`) or one matched on its title (`DERIVED`). Cite it as a *reference*, and quote
it only through `spec-citation`.

**Worked examples.** The same answer also names the release's own example models that declare the
element (their `file`). A construct that appears in one is valid for that release. To read one, call
`hypha_lookup` with `kind: "examples"` and the file's name without its folder or `.md` (for
`textual-notation/examples/sysml-src-examples-camera-example-camera.md`, the name
`sysml-src-examples-camera-example-camera`), and follow `nextLine` for a long model. `hypha_search`
with `kind: "examples"` finds one by its model's name, such as `Camera`. Quote an example only from
what the lookup returned, and name it with its upstream `source` path as evidence; never quote one
from memory.

## Large inputs

For a whole model, check each declaration in turn, but ask each question once: look up a keyword, a
production or a metaclass's features the first time it occurs, and reuse the answer for every other
declaration of the same kind. Report every finding, grouped by declaration.

## Findings format

Lead with a one-line **verdict** (`valid`, or `N issue(s) found`) and the release, then one item per
finding:

- **Location**: the offending line or snippet.
- **Rule**: the grammar or metamodel rule it breaks.
- **Why**: a one-line explanation.
- **Fix**: a corrected snippet.
- **Reference**: the metaclass, production or clause behind the ruling, with its tier, for a ruling
  that is not obvious.

## Procedure

1. Parse the notation structurally: declarations, memberships, relationships, keywords.
2. Run the syntax checks with `hypha_syntax`, then the structural checks with `hypha_metaclass`.
3. Report the findings in the format above. If nothing is wrong, say the notation is valid for the
   release.

## Limits

This reasons from the grammar and the metamodel; it is **not** the pilot-implementation compiler.
When a definitive ruling needs the real parser, say so rather than guessing. Never invent metamodel
structure or a specification clause; if the tools do not cover it, say so.
