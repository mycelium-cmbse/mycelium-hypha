# StateDefinition

`States` package · concrete metaclass

A StateDefinition is the Definition of the Behavior of a system or part of a system in a certain state condition.A StateDefinition may be related to up to three of its ownedFeatures by StateBehaviorMembership Relationships, all of different kinds, corresponding to the entry, do and exit actions of the StateDefinition.

## Generalizations

- [ActionDefinition](ActionDefinition.md)

## Owned features

### doAction : ActionUsage [0..1]

The ActionUsage of this StateDefinition to be performed while in the state defined by the StateDefinition. It is the owned ActionUsage related to the StateDefinition by a StateSubactionMembership with kind = do.

### entryAction : ActionUsage [0..1]

The ActionUsage of this StateDefinition to be performed on entry to the state defined by the StateDefinition. It is the owned ActionUsage related to the StateDefinition by a StateSubactionMembership with kind = entry.

### exitAction : ActionUsage [0..1]

The ActionUsage of this StateDefinition to be performed on exit to the state defined by the StateDefinition. It is the owned ActionUsage related to the StateDefinition by a StateSubactionMembership with kind = exit.

### isParallel : Boolean [1..1]

Whether the ownedStates of this StateDefinition are to all be performed in parallel. If true, none of the ownedActions (which includes ownedStates) may have any incoming or outgoing Transitions. If false, only one ownedState may be performed at a time.

### state : StateUsage [0..*]

The StateUsages, which are actions in the StateDefinition, that specify the discrete states in the behavior defined by the StateDefinition.

Subsets: `action`


## Constraints

- **checkStateDefinitionSpecialization**
- **deriveStateDefinitionDoAction**
- **deriveStateDefinitionEntryAction**
- **deriveStateDefinitionExitAction**
- **deriveStateDefinitionState**
- **validateStateDefinitionParallelSubactions**
- **validateStateDefinitionStateSubactionKind**
