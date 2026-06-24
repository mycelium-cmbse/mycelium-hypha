# UseCaseDefinition

`UseCases` package · concrete metaclass

A UseCaseDefinition is a CaseDefinition that specifies a set of actions performed by its subject, in interaction with one or more actors external to the subject. The objective is to yield an observable result that is of value to one or more of the actors.

## Generalizations

- [CaseDefinition](CaseDefinition.md)

## Owned features

### includedUseCase : UseCaseUsage [0..*]

The UseCaseUsages that are included by this UseCaseDefinition, which are the useCaseIncludeds of the IncludeUseCaseUsages owned by this UseCaseDefinition.


## Constraints

- **checkUseCaseDefinitionSpecialization**
- **deriveUseCaseDefinitionIncludedUseCase**
