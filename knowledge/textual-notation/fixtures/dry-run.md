# Validation dry-run

End-to-end check of the `sysml-validation` skill / `sysml-validator` agent against the
[fixtures](index.md). An independent run was given the fixture snippets **blind** (no expected
findings, no hint of which are valid) and asked to validate each using the skill procedure and the
knowledge base; its output was then compared with each fixture's documented expectation.

## Result – 9 / 9 matched

7 invalid fixtures + 2 valid examples (part-definitions, packages).

| Snippet | Expected | Validator verdict | Match |
| --- | --- | --- | --- |
| bad-multiplicity | issue: lower > upper | issue, cited `MultiplicityRange` | yes |
| missing-semicolon | issue: missing `;` | issue, grammar (declaration forms) | yes |
| unbalanced-braces | issue: missing `}` | issue, grammar (balanced braces) | yes |
| reserved-keyword-as-name | issue: keyword as name | issue, keyword reference | yes |
| typing-with-equals | issue: `=` not `:` | issue, cited `FeatureValue` + `PartUsage` | yes |
| redefines-nonexistent | issue: target not inherited | issue, cited `Redefinition` | yes |
| duplicate-member-name | issue: duplicate in namespace | issue, cited `Namespace` | yes |
| part-definitions (valid) | valid | valid | yes |
| packages (valid) | valid | valid | yes |

No false positives on the valid examples; no missed findings on the invalid ones.

## Observations & tuning notes

- **Accuracy is sufficient on the suite** – the skill/agent procedure plus the metamodel + grammar
  knowledge base reproduce every expected finding, with correct locations and fixes. No correctness
  tuning was required.
- **Grounding tightened (#51)** – the fixtures now cite the specific constraint or element member where
  one genuinely matches: `redefines-nonexistent` → `validateRedefinitionFeaturingTypes` and
  `redefinedFeature [1..1]` on [Redefinition]. The remaining rules have **no dedicated named OCL
  constraint** in the metamodel and are grounded in the relevant element member instead:
  `bad-multiplicity` → MultiplicityRange `lowerBound`/`upperBound`; `typing-with-equals` → `FeatureValue`
  + PartUsage `partDefinition`; `duplicate-member-name` → Namespace member distinguishability. (Note: an
  earlier draft of this report referenced a `validatePartUsagePartDefinition` constraint that does not
  exist – PartUsage carries `checkPartUsage*Specialization` constraints instead.)
- **Structural rulings are reasoned, not parser-definitive** – `redefines-nonexistent` and
  `duplicate-member-name` are resolved from the snippet's scope, not by a parser. The validator flagged
  this explicitly, consistent with the skill's "not a full compiler" limit. Keep framing such findings
  as reasoned (and say when a definitive ruling needs the pilot-implementation parser).
- **Syntax/lexical findings** (terminators, braces, reserved keywords) correctly ground in the grammar
  reference rather than a metaclass – as intended.

## Re-running

Give a validator the fixture **snippets** (from `invalid/*.md` and a couple of `../examples/`), blind
to the expected findings, and have it follow `skills/sysml-validation/SKILL.md`. Compare each verdict
with the fixture's documented expectation. Update this report when the suite or the skill/agent change.
