---
name: metamodel-lookup
description: Look up the structure of the OMG SysML v2 / KerML metamodel (not OMG UML 2.x) – a metaclass's features, supertypes, subtypes, redefinitions, multiplicities and constraints, plus enumerations and primitive types. Use when the user asks how a SysML v2 or KerML element (e.g. PartUsage, ConnectionDefinition, Feature) is defined in the metamodel, or how elements relate.
---

# Metamodel lookup

Answer questions about the structure of the KerML and SysML v2 metamodels using Hypha's
generated knowledge base. Be exact and cite the source file.

## Releases

The knowledge base is generated **per upstream release tag** (`YYYY-MM`, e.g. `2026-05`), one folder
per release under `knowledge/`. Read `knowledge/versions.json` first: it lists the installed tags
newest first and names the `default` one to answer from.

```sh
jq -r '.default, (.versions[].tag)' knowledge/versions.json
```

Then substitute that tag for `<tag>` in every path below.

- Answer from the **default** release unless the user names one.
- **Say which release you answered from** — e.g. "in 2026-05, `PartUsage` …". The metamodel changes
  between releases, so an unqualified structural claim is incomplete.
- If the user asks about a release that is not installed, say so and list the ones that are, rather
  than answering from a different release silently.
- The tag is the version. Ignore the model URI inside the XMI (`…/SysML/20250201`) — it tracks
  neither the release nor the content, and the same model can appear under several tags.

## Knowledge base

KerML and SysML v2 are **combined** in one tree (the SysML v2 model is generated from the full
KerML + SysML metamodel, so KerML metaclasses such as `Feature`, `Element` and `Membership` live
here too):

- `knowledge/<tag>/metamodel/index.md` — manifest: every metaclass grouped by package, plus
  `## Enumeration types` and `## Primitive types` sections. Each entry links to its element file
  and carries a one-line summary.
- `knowledge/<tag>/metamodel/elements/<Name>.md` — one file per element. This includes metaclasses,
  enumerations (`kind: enumeration`) and primitive types (`kind: primitive`).
- `knowledge/<tag>/metamodel/metamodel.json` — the same metamodel as a **structural graph**, with the
  inheritance closures already computed. Use it for set-shaped and cross-cutting questions; see
  [Querying the graph](#querying-the-graph) below.
- `knowledge/<tag>/cross-references.json` — links each element to the spec clauses that treat it, its BNF
  grammar production, and any worked example. See [Cross-references](#cross-references).

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

## Querying the graph

`knowledge/<tag>/metamodel/metamodel.json` holds every element as a node with its closures precomputed —
`allAncestors`, `allDescendants`, `directSubclasses`, and `inheritedAttributes` carrying the
declaring type in `inheritedFrom`. Owned attributes carry `type`, `lower`/`upper` (`-1` is
unbounded), `isDerived`, `isComposite`, `isOrdered`, `redefines` and `subsets`; classes also carry
`ownedOperations` and `constraints` with their OCL.

It is ~8 MB, so **never read it whole** — query it. `jq` is *recommended* for this; when it is not
installed, fall back to the markdown route described under [Procedure](#procedure), which is slower
and reads more files but always works.

```sh
# Every concrete subclass of Usage, anywhere in the hierarchy
jq -r '.classes[] | select(.allAncestors | index("Usage")) | select(.isAbstract|not) | .name' \
  knowledge/<tag>/metamodel/metamodel.json

# Which metaclasses have a feature typed by Expression (exact, no prose false positives)
jq -r '.classes[] | select(.ownedAttributes[]? | .type=="Expression") | .name' \
  knowledge/<tag>/metamodel/metamodel.json

# Where does PartUsage's `name` come from?
jq -r '.classes[] | select(.name=="PartUsage") | .inheritedAttributes[] | select(.name=="name") | .inheritedFrom' \
  knowledge/<tag>/metamodel/metamodel.json
```

Prefer the graph whenever the question is a *set* ("which metaclasses…", "every subclass of…"),
a *closure* ("all ancestors", "the full feature set"), or a *comparison* across several elements.
Prefer the markdown when the question is about **one** element and the answer should be quoted and
cited.

## Cross-references

`knowledge/<tag>/cross-references.json` maps each element name to `clauses` (specification clause
identifiers), `grammar` (BNF production) and `examples` (worked notation), plus the `element`
markdown path. It records clause **numbers only, never clause text**, which is why it can be
committed while `knowledge/<tag>/spec/` cannot.

```sh
# Which clauses treat PartUsage, and is there a worked example?
jq '.entries["PartUsage"] | {clauses: [.clauses[] | "\(.document) \(.clause)"], examples: [.examples[].file]}' \
  knowledge/<tag>/cross-references.json
```

Use it to point at the governing clause or a worked example after answering a structural question.

## Notation questions — how an element is written

`cross-references.json` also carries `grammar` (the productions that build an element) and
`features` (which metamodel feature a piece of syntax populates, and how). Together they answer
questions the metamodel alone cannot:

```sh
# How is a PartUsage written? -> the productions that build it
jq -r '.entries["PartUsage"].grammar[] | "\(.grammar) \(.production)"' knowledge/<tag>/cross-references.json

# What does the syntax of a Comment fill in?
jq -r '.entries["Comment"].features[] | "\(.grammar): \(.feature) \(.operator) via \(.productions|join(", "))"' \
  knowledge/<tag>/cross-references.json

# Which element's syntax sets `declaredName`, and through which production?
jq -r '.entries | to_entries[] | . as $e | $e.value.features[]
       | select(.feature=="declaredName") | "\($e.key): \(.productions|join(", "))"' \
  knowledge/<tag>/cross-references.json
```

Read the operator literally: `=` sets a value, `+=` adds to a collection, and `?=` sets a boolean
from the *presence of a keyword* — so `isStandard ?=` means the flag is true when the keyword is
written at all, with no value to supply.

The full production text, grouped by clause, is in
`knowledge/<tag>/textual-notation/grammar-{kerml,sysml}.md`; the graphical notation is in
`grammar-graphical.md`.

**An empty `features` list is meaningful, not missing data.** It means the element adds no syntax of
its own — `PartUsage` has none, because its declaration is inherited from `Usage`. Say that, rather
than reporting that nothing is known.

**The same feature often appears under both grammars.** The SysML grammar re-declares many KerML
productions, so `Comment` lists `body =` once for `kerml` and once for `sysml`. That is one fact, not
two: report it once and mention both grammars declare it only if the user asks.

## Provenance

Every fact you report belongs to one of three tiers — say which when it matters, and never blur them:

| Tier | Means | Where it comes from |
| --- | --- | --- |
| `NORMATIVE` | verbatim spec text, clause-anchored | `knowledge/<tag>/spec/` (git-ignored; may be absent) |
| `MODEL` | read directly from the XMI | element files, and `metamodel.json` fields other than the closures |
| `DERIVED` | computed or asserted here | `allAncestors` / `allDescendants` / `inheritedAttributes`, and every edge in `cross-references.json` |

The distinction that matters most in practice: a metaclass's own features are `MODEL`, its inherited
set is `DERIVED`, and a clause reference obtained by name matching is `DERIVED` — it tells you where
to look, it is not itself a citation. Quoting normative wording requires `spec-citation`.

## Procedure

1. Resolve the element name in `knowledge/<tag>/metamodel/index.md` (or `Grep` `knowledge/<tag>/metamodel/elements/`).
2. Read `knowledge/<tag>/metamodel/elements/<Name>.md`. Owned features are in **## Owned features**; for the
   full inherited set read the **## Inherited features** table (the `Owner` column says where each
   comes from).
3. Report the metaclass, abstractness, supertypes/subtypes, and the relevant features (name, type,
   multiplicity, modifiers), plus redefinitions/subsettings and constraints. Follow `[Type](Type.md)`
   links to related elements when needed. Always include the source file path.
4. If the element isn't in the knowledge base, say so — never invent structure.

Answer single-element lookups inline (one index read + one element file — the inherited table
already gives the full feature set). For a **set or closure** question, query `metamodel.json` as
above instead of reading files. Delegate to the `metamodel-navigator` subagent only when the answer
still needs many element files read — e.g. comparing the documentation of several metaclasses — so
the bulk reading stays out of this context.
