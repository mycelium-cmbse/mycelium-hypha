# IncludeUseCaseUsage

`UseCases` package · concrete metaclass

An IncludeUseCaseUsage is a UseCaseUsage that represents the inclusion of a UseCaseUsage by a UseCaseDefinition or UseCaseUsage. Unless it is the IncludeUseCaseUsage itself, the UseCaseUsage to be included is related to the includedUseCase by a ReferenceSubsetting Relationship. An IncludeUseCaseUsage is also a PerformActionUsage, with its useCaseIncluded as the performedAction.

## Generalizations

- [PerformActionUsage](PerformActionUsage.md)
- [UseCaseUsage](UseCaseUsage.md)

## Owned features

### useCaseIncluded : UseCaseUsage [1..1]

The UseCaseUsage to be included by this IncludeUseCaseUsage. It is the performedAction of the IncludeUseCaseUsage considered as a PerformActionUsage, which must be a UseCaseUsage.

Redefines: `performedAction`


## Constraints

- **checkIncludeUseCaseUsageSpecialization**
- **validateIncludeUseCaseUsageReference**
