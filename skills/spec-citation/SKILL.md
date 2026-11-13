---
name: spec-citation
description: "Quote the normative OMG SysML v2 / KerML specification text, or the Systems Modeling API & Services specification text, with an exact clause reference: one clause, or what several clauses say about a topic. Covers the SysML v2, KerML and Systems Modeling API specifications, not the OMG UML specification. Use when the user wants the authoritative wording of one of those specifications, or asks 'what does the SysML v2 / KerML / API spec say about ...'."
---

# Spec citation

Back claims with the literal text of the OMG KerML and SysML v2 specifications, always with a clause
reference, read through the **hypha MCP server's tools**. They are named here without a client's
prefix: a client may show `hypha_spec` as, for example, `mcp__hypha__hypha_spec`. Never paraphrase a
normative ("shall"/"must") statement.

## Which release

Clause text is generated per upstream release tag (`YYYY-MM`).

- Every tool answers from the **default** release unless it is given a `tag`. Call
  `hypha_release_status` when unsure what is installed.
- **Say which release each citation is from**: clause numbering shifts between releases, so a bare
  clause reference can be wrong for another one.
- If the user asks about a release that is not installed, say so and name the installed ones.

## Tools

The two language specifications are the `document` values `kerml` (KerML 1.0) and `sysml2`
(SysML v2.0).

- `hypha_spec` with the `question`:

  | Question | `question` | Arguments |
  | --- | --- | --- |
  | A clause's verbatim text, title, pages and whether it is normative | `clause` | `document`, `clause` (such as `7.4.2`) |
  | The elements a clause treats | `elements-in-clause` | `document`, `clause`; `includeSubclauses` |

- `hypha_search_spec` with `text` (and optionally `document`): the clauses whose text says a word or
  phrase, those that say it most first, each with an excerpt. An excerpt is **not a quotation**: read
  the clause with `hypha_spec` before quoting it.
- `hypha_search` with `text` and `kind: "kerml"` or `kind: "sysml2"`: a clause by its number or title.
- `hypha_syntax` with `question: "cross-reference"` and `element`: the clauses that treat a metamodel
  element, such as `PartUsage`. Each clause names its `document`, its `clause` number, its
  `provenance` and the `method` that found it.
- `hypha_resolve` with an `address` a search returned, such as `hypha://2026-05/kerml/7.4.2`: the
  clause's whole generated page. The same page is that `hypha://` resource.

## Reading an answer

A `clause` answer carries a `value` with `document`, `clause`, `title`, `pages`, `normative` and
`text`, its `grounding` (`NORMATIVE`, the release and the file it was read from) and, when the
specification text was never generated, `unavailable`.

- A `null` value with no `unavailable` means the release has no clause of that number: say so, and
  find the intended one with `hypha_search` or `hypha_search_spec`.
- A `null` value with `unavailable` means the text is not on this machine: see below.

## When the clause text is missing

The specification text is generated locally from the user's own copy of the OMG PDFs and is never
shipped (OMG licensing forbids redistributing it). Do not simply refuse. Degrade in this order:

1. **Name the governing clause** from `hypha_syntax` `cross-reference` for the element. That works
   without the text, since a cross-reference holds clause numbers only.
2. Say plainly that you are giving a clause **reference**, not a quotation, and give its provenance:
   a clause the grammar states (`MODEL`, method `grammar-clause`) or one matched on its title
   (`DERIVED`, method `exact-title-match`).
3. Tell the user how to unlock the verbatim text: installing the release again with
   `hypha_install_release` (`includeSpecifications` left on) downloads the PDFs and extracts their
   text; ask first, as it takes minutes (the `version-management` skill). Outside a client with that
   tool, pass on what `unavailable` says, with its commands as written.

Never invent or paraphrase the wording of a clause you cannot read. A pointer to the right clause is
useful; a fabricated quotation is not.

## Systems Modeling API and Services

The API specification is the third document, versioned **apart from the language releases**.
`hypha_api` with `question: "clause"` and `clause` (and optionally `version`) answers one of its
clauses, with the same `value` as a language clause; `hypha_release_status` names the installed API
versions. State the **API version** with every citation from it, and cite it as
*Systems Modeling API 1.0 §<clause> <title> (p. <pages>)*. For what the PIM and the REST/HTTP PSM
themselves say, use the `api-lookup` skill.

## Citing

Quote the `text` verbatim and attribute it as **`<document> <version> §<clause> <title> (p. <pages>)`**,
such as *KerML 1.0 §7.4.2 Concrete Syntax (p. 45)* or *SysML 2.0 §2 Conformance (pp. 35-36)*. The
text keeps the specification's own formatting: `*italic*`, `**bold**`, inline `` `code` `` and fenced
code blocks for textual-notation examples. Keep or drop the markers, but never change the words.

## Normative vs informative

- **Normative**: sentences using "shall"/"must" (and "shall not"). The `normative` flag says a clause
  contains such statements. Quote these exactly; never paraphrase.
- **Informative**: notes and examples. Treat content inside `<!-- informative:... -->` markers **or**
  any paragraph that begins "NOTE"/"EXAMPLE" as informative, even when the marker is absent: the
  markers are sparse.

## Procedure

1. Find the clause:
   - a clause number the user gives: read it with `hypha_spec` `clause`;
   - a metamodel element: its clauses from `hypha_syntax` `cross-reference`;
   - a topic: `hypha_search_spec` for what the text says, or `hypha_search` for a clause title.
2. Read each clause you quote with `hypha_spec` `clause`. For a topic several clauses treat, read the
   few that say it most, not every hit.
3. Quote verbatim, attributed in the format above, with the release, and flag each quote as normative
   or informative.
4. If the specification does not cover it, say so; never fabricate. If the text is unavailable,
   follow [When the clause text is missing](#when-the-clause-text-is-missing) instead of stopping.
