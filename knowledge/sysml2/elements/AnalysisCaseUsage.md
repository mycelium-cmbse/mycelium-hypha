# AnalysisCaseUsage

`AnalysisCases` package · concrete metaclass

An AnalysisCaseUsage is a Usage of an AnalysisCaseDefinition.

## Generalizations

- [CaseUsage](CaseUsage.md)

## Owned features

### analysisCaseDefinition : AnalysisCaseDefinition [0..1]

The AnalysisCaseDefinition that is the definition of this AnalysisCaseUsage.

Redefines: `caseDefinition`

### resultExpression : &#171;untyped&#187; [0..1]

An Expression used to compute the result of the AnalysisCaseUsage, owned via a ResultExpressionMembership.


## Constraints

- **checkAnalysisCaseUsageSpecialization**
- **checkAnalysisCaseUsageSubAnalysisCaseSpecialization**
- **deriveAnalysisCaseUsageResultExpression**
