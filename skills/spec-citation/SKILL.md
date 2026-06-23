---
name: spec-citation
description: Quote the normative KerML / SysML v2 specification text with an exact clause reference. Use when the user wants the authoritative wording of the OMG specification, or asks "what does the spec say about ...".
---

# Spec citation

Back claims with the literal text of the OMG KerML and SysML v2 specifications, always with a
clause reference. Never paraphrase a normative ("shall"/"must") statement.

## Knowledge base

- `knowledge/spec/kerml/` — KerML 1.0 spec text, segmented by clause/section.
- `knowledge/spec/sysml2/` — SysML v2 (Parts 1–2) spec text, segmented by clause/section.

Each file records its clause number, title, and source document/page.

## Procedure

1. Search the relevant `knowledge/spec/` tree for the concept and read the matching clause file(s).
2. Quote verbatim. Attribute every quote: specification name + version, clause number and title.
3. Distinguish normative statements from informative notes and examples.
4. If the spec text doesn't cover it, say so — do not fabricate a citation.

For multi-clause gathering or cross-referencing, delegate to the `spec-citation` subagent.
