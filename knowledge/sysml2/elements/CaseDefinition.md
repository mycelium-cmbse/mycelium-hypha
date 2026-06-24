# CaseDefinition

`Cases` package · concrete metaclass

A CaseDefinition is a CalculationDefinition for a process, often involving collecting evidence or data, relative to a subject, possibly involving the collaboration of one or more other actors, producing a result that meets an objective.

## Generalizations

- [CalculationDefinition](CalculationDefinition.md)

## Owned features

### actorParameter : PartUsage [0..*]

The parameters of this CaseDefinition that represent actors involved in the case.

### objectiveRequirement : RequirementUsage [0..1]

The RequirementUsage representing the objective of this CaseDefinition.

Subsets: `usage`

### subjectParameter : Usage [1..1]

The parameter of this CaseDefinition that represents its subject.


## Constraints

- **checkCaseDefinitionSpecialization**
- **deriveCaseDefinitionActorParameter**
- **deriveCaseDefinitionObjectiveRequirement**
- **deriveCaseDefinitionSubjectParameter**
- **validateCaseDefinitionOnlyOneObjective**
- **validateCaseDefinitionOnlyOneSubject**
- **validateCaseDefinitionSubjectParameterPosition**
