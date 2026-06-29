---
name: spec-citation
description: Quote the normative OMG SysML v2 / KerML specification text with an exact clause reference. Covers the SysML v2 and KerML specifications, not the OMG UML specification. Use when the user wants the authoritative wording of the SysML v2 or KerML spec, or asks 'what does the SysML v2 / KerML spec say about ...'.
---

# Spec citation

Back claims with the literal text of the OMG KerML and SysML v2 specifications, always with a
clause reference. Never paraphrase a normative ("shall"/"must") statement.

## Knowledge base

- `knowledge/spec/kerml/` — KerML 1.0, one markdown file per clause.
- `knowledge/spec/sysml2/` — SysML v2.0, one markdown file per clause.

Each tree has an `index.md` (a human-readable table of every clause → title → pages → file) and an
`index.json` (the same catalog, machine-readable, with `entries` keyed by clause number →
`{title, pages, normative, file}`) — use either as the entry point. `index.json` is handy for exact
lookup ("clause 7.4.2") or filtering (e.g. `normative` clauses) before reading a file; it carries
metadata only, never clause text. Clause files are named by zero-padded clause number + slug, e.g.
`07.04.02-concrete-syntax.md`, so a clause number sorts and greps directly.

> `knowledge/spec/` is **generated locally** by `tools/spec-extract` from your own copies of the OMG
> PDFs and is **not shipped** with the plugin (OMG licensing forbids redistributing the spec text). If
> the tree is empty, tell the user to regenerate it (see `tools/spec-extract`) rather than guessing a
> citation.

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

1. Find the clause: look it up in `index.json` / `index.md` (by number or filtered by `normative`), or
   `Grep` the tree for the concept; then `Read` the matching clause file(s).
2. Quote verbatim, attributed in the format above.
3. Flag whether each quote is normative or informative.
4. If the spec text doesn't cover it (or the tree isn't generated), say so — do not fabricate.

For multi-clause gathering or cross-referencing, delegate to the `spec-citation` subagent.
