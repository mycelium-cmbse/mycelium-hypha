# UseCaseUsage

`UseCases` package · concrete metaclass

A UseCaseUsage is a Usage of a UseCaseDefinition.

## Generalizations

- [CaseUsage](CaseUsage.md)

## Owned features

### includedUseCase : UseCaseUsage [0..*]

The UseCaseUsages that are included by this UseCaseUse, which are the useCaseIncludeds of the IncludeUseCaseUsages owned by this UseCaseUsage.

### useCaseDefinition : UseCaseDefinition [0..1]

The UseCaseDefinition that is the definition of this UseCaseUsage.

Redefines: `caseDefinition`


## Constraints

- **checkUseCaseUsageSpecialization**
- **checkUseCaseUsageSubUseCaseSpecialization**
- **deriveUseCaseUsageIncludedUseCase**
