# StateUsage

`States` package · concrete metaclass

A StateUsage is an ActionUsage that is nominally the Usage of a StateDefinition. However, other kinds of kernel Behaviors are also allowed as types, to permit use of BehaviorsA StateUsage may be related to up to three of its ownedFeatures by StateSubactionMembership Relationships, all of different kinds, corresponding to the entry, do and exit actions of the StateUsage.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### doAction : ActionUsage [0..1]

The ActionUsage of this StateUsage to be performed while in the state defined by the StateDefinition. It is the owned ActionUsage related to the StateUsage by a StateSubactionMembership with kind = do.

### entryAction : ActionUsage [0..1]

The ActionUsage of this StateUsage to be performed on entry to the state defined by the StateDefinition. It is the owned ActionUsage related to the StateUsage by a StateSubactionMembership with kind = entry.

### exitAction : ActionUsage [0..1]

The ActionUsage of this StateUsage to be performed on exit to the state defined by the StateDefinition. It is the owned ActionUsage related to the StateUsage by a StateSubactionMembership with kind = exit.

### isParallel : Boolean [1..1]

Whether the nestedStates of this StateUsage are to all be performed in parallel. If true, none of the nestedActions (which include nestedStates) may have any incoming or outgoing Transitions. If false, only one nestedState may be performed at a time.

### stateDefinition : &#171;untyped&#187; [0..*]

The Behaviors that are the types of this StateUsage. Nominally, these would be StateDefinitions, but kernel Behaviors are also allowed, to permit use of Behaviors from the Kernel Model Libraries.

Redefines: `actionDefinition`


## Constraints

- **checkStateUsageExclusiveStateSpecialization**
- **checkStateUsageOwnedStateSpecialization**
- **checkStateUsageSpecialization**
- **checkStateUsageSubstateSpecialization**
- **deriveStateUsageDoAction**
- **deriveStateUsageEntryAction**
- **deriveStateUsageExitAction**
- **validateStateUsageParallelSubactions**
- **validateStateUsageStateSubactionKind**
