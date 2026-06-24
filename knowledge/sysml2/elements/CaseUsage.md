# CaseUsage

`Cases` package · concrete metaclass

A CaseUsage is a Usage of a CaseDefinition.

## Generalizations

- [CalculationUsage](CalculationUsage.md)

## Owned features

### actorParameter : PartUsage [0..*]

The parameters of this CaseUsage that represent actors involved in the case.

### caseDefinition : CaseDefinition [0..1]

The CaseDefinition that is the type of this CaseUsage.

Redefines: `calculationDefinition`

### objectiveRequirement : RequirementUsage [0..1]

The RequirementUsage representing the objective of this CaseUsage.

Subsets: `usage`

### subjectParameter : Usage [1..1]

The parameter of this CaseUsage that represents its subject.


## Constraints

- **checkCaseUsageSpecialization**
- **checkCaseUsageSubcaseSpecialization**
- **deriveCaseUsageActorParameter**
- **deriveCaseUsageObjectiveRequirement**
- **deriveCaseUsageSubjectParameter**
- **validateCaseUsageOnlyOneObjective**
- **validateCaseUsageOnlyOneSubject**
- **validateCaseUsageSubjectParameterPosition**
