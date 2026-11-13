---
name: metamodel-lookup
description: "Look up the structure of the OMG SysML v2 / KerML metamodel (not OMG UML 2.x): a metaclass's features, supertypes, subtypes, redefinitions, multiplicities and constraints, plus enumerations and primitive types, and cross-cutting questions over many metaclasses (which metaclasses have a feature typed by Expression, every concrete subclass of Usage, who redefines a feature). Use when the user asks how a SysML v2 or KerML element (e.g. PartUsage, ConnectionDefinition, Feature) is defined in the metamodel, or how elements relate."
---

# Metamodel lookup

Answer questions about the structure of the KerML and SysML v2 metamodels from the **hypha MCP
server's tools**. They are named here without a client's prefix: a client may show `hypha_metaclass`
as, for example, `mcp__hypha__hypha_metaclass`. Be exact, and say where each fact comes from.

KerML and SysML v2 are one combined metamodel here: the SysML v2 model is generated from the full
KerML + SysML metamodel, so KerML metaclasses such as `Feature`, `Element` and `Membership` are
answered too.

## Which release

The knowledge is generated per upstream release tag (`YYYY-MM`, such as `2026-05`).

- Every tool answers from the **default** release unless it is given a `tag`. Call
  `hypha_release_status` when unsure what is installed or which release is the default.
- **Say which release answered**, as every result's grounding names it: "in 2026-05, `PartUsage` ...".
  The metamodel changes between releases, so an unqualified structural claim is incomplete.
- If the user asks about a release that is not installed, say so and name the installed ones rather
  than answering from another release. Offer `hypha_install_release` only when the user wants it: it
  downloads from GitHub and takes minutes.
- The tag is the version. Ignore the model URI inside the XMI (`.../SysML/20250201`): it tracks neither
  the release nor the content.

## Which tool answers what

One metaclass, `hypha_metaclass` with the `question`:

| Question | `question` | Arguments |
| --- | --- | --- |
| What it is: kind, package, supertypes | `element` | `name` |
| Every supertype | `ancestors` | `name` |
| What specializes it | `subclasses` | `name`; `concreteOnly` to leave abstract ones out |
| Does it specialize another, and through what | `specializes` | `name`, `general` |
| Its features, owned and inherited, each with its owner | `features` | `name`; `feature` for one |
| Its features without the inherited ones it redefines | `effective-features` | `name` |
| Its constraints, or operations, each with where it is declared | `constraints`, `operations` | `name`; `includeInherited` (default true) |
| An enumeration's literals | `literals` | `name`, such as `VisibilityKind` |
| The metaclasses its features are typed by, and the features typed by it | `neighbours` | `name` |

One feature, `hypha_feature` with the `question`:

| Question | `question` | Arguments |
| --- | --- | --- |
| What redefines it, or subsets it | `redefinitions`, `subsettings` | `feature`; `class` for the owner's own feature only |
| The other end of its association | `opposite` | `feature`, `class` |
| The constraints that name it | `feature-constraints` | `feature`, `class` |
| Everything that redefines or subsets it, directly or not | `contributors` | `feature`, `class` |

A type across the whole metamodel, `hypha_typed_by` with the `question`:

| Question | `question` | Arguments |
| --- | --- | --- |
| Which metaclasses have a feature typed by it | `classes-with-feature-typed` | `type`; `compositeOnly` |
| Every feature that can hold a value of it | `features-accepting` | `type`; `class` for one metaclass's features |
| The composite features through which an element of it can be owned | `containers` | `type`; `class` |

And:

- `hypha_package` with `name`: a package's metaclasses, nested packages and everything below it.
- `hypha_search` with `text` and `kind: "metamodel"`: find a metaclass by name or qualified name
  when the user's spelling is not exact.
- `hypha_lookup` with `kind: "metamodel"` and `name`: the metaclass's whole generated page, a page at
  a time, for documentation wording to quote. When `nextLine` is set, call again with it as
  `startLine`. The same page is the resource `hypha://<tag>/metamodel/<Name>`.
- `hypha_syntax` for how an element is written: see [Notation](#notation) below.
- `hypha_diff` with `from`, `to` and optionally `element`: what changed between two installed releases.

## Questions about many metaclasses

A question about a set, a closure or a comparison is still **one call**: the tools return closures and
inherited sets the generator already computed, so never collect them by hand from several pages.

- "Which metaclasses have a feature typed by `Expression`?" `hypha_typed_by`,
  `classes-with-feature-typed`, `type: "Expression"`.
- "Every concrete subclass of `Usage`?" `hypha_metaclass`, `subclasses`, `name: "Usage"`,
  `concreteOnly: true`.
- "Who redefines `Feature::type`?" `hypha_feature`, `redefinitions`, `feature: "type"`,
  `class: "Feature"`. Without `class` the answer covers every feature named `type`, such as
  `FeatureTyping::type` too.
- "Where does `PartUsage`'s `name` come from?" `hypha_metaclass`, `features`, `name: "PartUsage"`,
  `feature: "name"`: the answer names its owner.
- "Compare `PartUsage` and `ItemUsage`": one `hypha_metaclass` call per metaclass, then compare the
  facts asked for; report only those.

## Reading an answer

Every result carries a `value`, a `grounding` (tier, release and the generated file it was read from)
and, when part of the knowledge was never generated, `unavailable` saying why and how to generate it.

- A `null` value means the release has no such element: say so, and look the intended name up with
  `hypha_search`. Never invent structure.
- A reference to another feature names its owner (a `class`, or an `association` for an end the
  association owns), because several metaclasses declare features of the same name. Report the owner.
- Multiplicities use `-1` for an unbounded upper bound: write it as `*`.
- A tool error names what is missing (a release that is not installed, a missing argument) and how to
  fix it. Act on it, or tell the user.

## Notation

`hypha_syntax` answers how an element is written, from the grammar itself rather than from names:

- `cross-reference` with `element`: the specification clauses that treat it, the grammar productions
  that build it, the features its syntax fills in, and worked examples.
- `feature-assignments` with `feature` (and optionally `operator`): which elements' syntax fills in a
  feature, through which operator and productions.
- `built-by` with `production`: the elements a production builds. `keyword` with `word`: the
  productions that write a keyword or operator, such as `ref` or `:>>`.

Read the operators literally: `=` sets a value, `+=` adds to a collection, and `?=` sets a boolean
from the **presence of a keyword**, so `isStandard ?=` means the flag is true when the keyword is
written at all. An element whose syntax fills in no features adds no syntax of its own (`PartUsage`'s
declaration is inherited from `Usage`): say that, rather than that nothing is known. The SysML grammar
re-declares many KerML productions, so the same assignment can appear under both grammars; report it
once.

## Provenance

Every fact belongs to one of three tiers; the grounding says which. Say it when it matters, and never
blur them:

| Tier | Means |
| --- | --- |
| `NORMATIVE` | verbatim specification text, with its clause |
| `MODEL` | read directly from the metamodel: a metaclass's own features, its constraints and operations |
| `DERIVED` | computed or matched: inherited feature sets, closures, and every cross-reference edge |

The distinction that matters most: a metaclass's own features are `MODEL`, its inherited set is
`DERIVED`, and a clause found through a cross-reference is `DERIVED`. It says where to look, but it is
not a citation. Quoting the normative wording is the `spec-citation` skill's job.

## Procedure

1. Pick the tool and `question` from the tables above; pass `tag` only when the user names a release.
2. For one metaclass, start with `element`, then the question asked (`features`, `constraints`, ...).
   For a set or closure, make the one call that answers it.
3. Report the metaclass, abstractness, supertypes and the relevant features (name, type, multiplicity,
   modifiers, owner), redefinitions and constraints, with the release and the tier.
4. Quote documentation only from `hypha_lookup`'s page, never from memory.
5. If the element is not in the release, say so; never invent structure.
