# ForLoopActionUsage

`Actions` package · concrete metaclass

A ForLoopActionUsage is a LoopActionUsage that specifies that its bodyAction ActionUsage should be performed once for each value, in order, from the sequence of values obtained as the result of the seqArgument Expression, with the loopVariable set to the value for each iteration.

## Generalizations

- [LoopActionUsage](LoopActionUsage.md)

## Owned features

### loopVariable : ReferenceUsage [1..1]

The ownedFeature of this <co>ForLoopActionUsage that acts as the loop variable, which is assigned the successive values of the input sequence on each iteration. It is the ownedFeature that redefines ForLoopAction::var.</co>

### seqArgument : &#171;untyped&#187; [1..1]

The Expression whose result provides the sequence of values to which the loopVariable is set for each iterative performance of the bodyAction. It is the Expression whose result is bound to the seq input parameter of this ForLoopActionUsage.


## Constraints

- **checkForLoopActionUsageSpecialization**
- **checkForLoopActionUsageSubactionSpecialization**
- **checkForLoopActionUsageVarRedefinition**
- **deriveForLoopActionUsageLoopVariable**
- **deriveForLoopActionUsageSeqArgument**
- **validateForLoopActionUsageLoopVariable**
- **validateForLoopActionUsageParameters**
