# LoopActionUsage

`Actions` package · abstract metaclass

A LoopActionUsage is an ActionUsage that specifies that its bodyAction should be performed repeatedly. Its subclasses WhileLoopActionUsage and ForLoopActionUsage provide different ways to determine how many times the bodyAction should be performed.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### bodyAction : ActionUsage [1..1]

The ActionUsage to be performed repeatedly by the LoopActionUsage. It is the second parameter of the LoopActionUsage.


## Constraints

- **deriveLoopActionUsageBodyAction**
