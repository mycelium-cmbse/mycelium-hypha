# TerminateActionUsage

`Actions` package · concrete metaclass

A TerminateActionUsage is an ActionUsage that directly or indirectly specializes the ActionDefinition TerminateAction from the Systems Model Library, which causes a given terminatedOccurrence to end during its performance. By default, the terminatedOccurrence is the featuring instance (that) of the performance of the TerminateActionUsage, generally the performance of its immediately containing ActionDefinition or ActionUsage.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### terminatedOccurrenceArgument : &#171;untyped&#187; [0..1]

The Expression that is the featureValue of the terminateOccurrence parameter of this TerminateActionUsage.


## Constraints

- **checkTerminateActionUsageSpecialization**
- **checkTerminateActionUsageSubactionSpecialization**
- **deriveTerminateActionUsageTerminatedOccurrenceArgument**
