# WhileLoopActionUsage

`Actions` package · concrete metaclass

A WhileLoopActionUsage is a LoopActionUsage that specifies that the bodyAction ActionUsage should be performed repeatedly while the result of the whileArgument Expression is true or until the result of the untilArgument Expression (if provided) is true. The whileArgument Expression is evaluated before each (possible) performance of the bodyAction, and the untilArgument Expression is evaluated after each performance of the bodyAction.

## Generalizations

- [LoopActionUsage](LoopActionUsage.md)

## Owned features

### untilArgument : &#171;untyped&#187; [0..1]

The Expression whose result, if false, determines that the bodyAction should continue to be performed. It is the (optional) third owned parameter of the WhileLoopActionUsage.

### whileArgument : &#171;untyped&#187; [1..1]

The Expression whose result, if true, determines that the bodyAction should continue to be performed. It is the first owned parameter of the WhileLoopActionUsage.


## Constraints

- **checkWhileLoopActionUsageSpecialization**
- **checkWhileLoopActionUsageSubactionSpecialization**
- **deriveWhileLoopActionUsageUntilArgument**
- **deriveWhileLoopActionUsageWhileArgument**
- **validateWhileLoopActionUsage**
