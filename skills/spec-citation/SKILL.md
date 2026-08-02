---
name: spec-citation
description: Quote the normative OMG SysML v2 / KerML specification text with an exact clause reference. Covers the SysML v2 and KerML specifications, not the OMG UML specification. Use when the user wants the authoritative wording of the SysML v2 or KerML spec, or asks 'what does the SysML v2 / KerML spec say about ...'.
---

# Spec citation

Back claims with the literal text of the OMG KerML and SysML v2 specifications, always with a
clause reference. Never paraphrase a normative ("shall"/"must") statement.

## Releases

Clause text is generated **per upstream release tag** (`YYYY-MM`). Read `knowledge/versions.json`
for the installed tags and the `default` one, then substitute it for `<tag>` below. Quote from the
default release unless the user names another, and **state which release the citation is from** —
clause numbering shifts between releases, so a bare clause reference can be wrong for another one.

## Knowledge base

- `knowledge/<tag>/spec/kerml/` — KerML 1.0, one markdown file per clause.
- `knowledge/<tag>/spec/sysml2/` — SysML v2.0, one markdown file per clause.

Each tree has an `index.md` (a human-readable table of every clause → title → pages → file) and an
`index.json` (the same catalog, machine-readable, with `entries` keyed by clause number →
`{title, pages, normative, file}`) — use either as the entry point. `index.json` is handy for exact
lookup ("clause 7.4.2") or filtering (e.g. `normative` clauses) before reading a file; it carries
metadata only, never clause text. Clause files are named by zero-padded clause number + slug, e.g.
`07.04.02-concrete-syntax.md`, so a clause number sorts and greps directly.

> `knowledge/<tag>/spec/` is **generated locally** by `tools/spec-extract` from your own copies of the OMG
> PDFs and is **not shipped** with the plugin (OMG licensing forbids redistributing the spec text).

- `knowledge/<tag>/cross-references.json` — **committed**, and available even when the clause text is not.
  It maps each metamodel element to the clause identifiers that treat it. It holds clause *numbers*
  only, never wording, which is exactly why it can ship.

## When the clause text is missing

Do not simply refuse. Degrade in this order:

1. Look the element up in `knowledge/<tag>/cross-references.json` and **name the governing clause**:
   `jq '.entries["PartUsage"].clauses' knowledge/<tag>/cross-references.json`.
2. Say plainly that you are giving a clause *reference*, not a quotation, and that the reference is
   `DERIVED` (matched by name) rather than read from the specification.
3. Tell the user how to unlock verbatim text — obtain the PDFs and regenerate with
   `tools/spec-extract`.

Never invent or paraphrase the wording of a clause you cannot read. A pointer to the right clause is
useful; a fabricated quotation is not.

## Clause file structure

YAML front matter, then a `# <number> <title>` heading and the clause body:

```
---
clause: "2"
title: "Conformance"
document: "SysML"      # "KerML" or "SysML"
version: "2.0"         # "1.0" for KerML, "2.0" for SysML
pages: "35-36"         # source PDF page range
normative: true        # the clause contains "shall"/"must" wording
---
```

In the body, the spec's own formatting is preserved: `*italic*`, `**bold**`, inline `` `code` `` and
fenced ` ``` ` code blocks for textual-notation examples. Informative asides, when detected, are
wrapped in `<!-- informative:note -->` / `<!-- informative:example -->` markers (these are sparse —
do not rely on them; see below).

## Citing

Quote the wording verbatim and attribute it as: **`<document> <version> §<clause> <title> (p. <pages>)`**
— e.g. *KerML 1.0 §7.4.2 Concrete Syntax (p. 45)* or *SysML 2.0 §2 Conformance (pp. 35–36)*. The
inline `*…*` / `` `…` `` markers reflect the spec's italics / monospace; keep or drop them, but never
change the words.

## Normative vs informative

- **Normative** = sentences using "shall"/"must" (and "shall not"). The front-matter `normative: true`
  flag tells you a clause contains such statements. Quote these exactly; never paraphrase.
- **Informative** = notes and examples. Treat content inside `<!-- informative:… -->` markers **or**
  any paragraph that begins "NOTE"/"EXAMPLE" as informative, even when the marker is absent.

## Procedure

1. Find the clause: when the question names a metamodel element, `knowledge/<tag>/cross-references.json`
   resolves it to clause numbers directly. Otherwise look it up in `index.json` / `index.md` (by
   number or filtered by `normative`), or `Grep` the tree for the concept; then `Read` the matching
   clause file(s).
2. Quote verbatim, attributed in the format above.
3. Flag whether each quote is normative or informative.
4. If the spec text doesn't cover it, say so — do not fabricate. If the tree isn't generated, follow
   [When the clause text is missing](#when-the-clause-text-is-missing) instead of stopping.

For multi-clause gathering or cross-referencing, delegate to the `spec-citation` subagent.
