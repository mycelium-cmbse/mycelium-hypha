---
name: sysml-validation
description: Validate SysML v2 (or KerML) textual notation for syntactic and structural correctness, and explain any violations with fixes. Use when the user provides SysML v2 textual notation to check, or asks whether a snippet is valid.
---

# SysML v2 validation

Check SysML v2 / KerML textual notation against the grammar and metamodel constraints, then
explain findings clearly with suggested corrections.

## Knowledge base

- `knowledge/textual-notation/` — grammar summary, keyword reference, and worked examples.
- `knowledge/{sysml2,kerml}/` — metamodel constraints the notation must satisfy.
- `knowledge/spec/` — normative rules to cite for a violation.

## Procedure

1. Parse the notation structurally (declarations, memberships, relationships, keywords).
2. Check syntax against the grammar reference, then structural/semantic rules against the metamodel.
3. For each issue report: location, the violated rule, why it's wrong, and a corrected snippet.
4. Cite a metamodel element or spec clause for non-obvious rulings.

## Limits

This reasons from the documented grammar and metamodel; it is not the pilot-implementation
compiler. When a definitive ruling requires the actual parser, say so.

For larger inputs, delegate to the `sysml-validator` subagent.
