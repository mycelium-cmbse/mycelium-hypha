# Validation fixtures

A regression suite for the [`sysml-validation`](../../../skills/sysml-validation/SKILL.md) skill and
the [`sysml-validator`](../../../agents/sysml-validator.md) agent: snippets paired with the outcome the
validator should produce. **Valid** fixtures must yield no findings; each **invalid** fixture documents
the finding(s) that should be reported (location, violated rule, why, a corrected snippet, and a
knowledge-base reference).

A [dry-run report](dry-run.md) records the latest end-to-end check of the validator against this suite.

## Valid (expected: no findings)

Use the known-valid real models (EPL-2.0) already in the knowledge base as the "should pass" set,
rather than duplicating them:

- The worked examples: [packages](../examples/packages.md), [part-definitions](../examples/part-definitions.md),
  [generalization](../examples/generalization.md), [connections](../examples/connections.md),
  [ports](../examples/ports.md), [actions](../examples/actions.md), [states](../examples/states.md),
  [constraints](../examples/constraints.md), [requirements](../examples/requirements.md).
- The [reference models](../index.md) for the remaining constructs (Item, Flow, Enumeration, Interface,
  Occurrence, Use case, Analysis/Verification case, Calculation, Allocation, Metadata, View, Viewpoint,
  Rendering, Concern).

## Invalid (expected: findings)

Minimal **synthetic** fixtures authored for testing (not from the spec); each isolates one violation.

| Fixture | Category | Expected finding |
| --- | --- | --- |
| [bad-multiplicity](invalid/bad-multiplicity.md) | semantic | multiplicity lower bound greater than upper |
| [missing-semicolon](invalid/missing-semicolon.md) | syntax | missing `;` statement terminator |
| [unbalanced-braces](invalid/unbalanced-braces.md) | syntax | missing closing `}` |
| [reserved-keyword-as-name](invalid/reserved-keyword-as-name.md) | lexical | reserved keyword used as a name |
| [typing-with-equals](invalid/typing-with-equals.md) | syntax | `=` used where typing `:` is required |
| [redefines-nonexistent](invalid/redefines-nonexistent.md) | structural | `redefines` target is not inherited |
| [duplicate-member-name](invalid/duplicate-member-name.md) | structural | duplicate member name in a namespace |
