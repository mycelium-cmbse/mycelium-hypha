# IfActionUsage

`Actions` package · concrete metaclass

An IfActionUsage is an ActionUsage that specifies that the thenAction ActionUsage should be performed if the result of the ifArgument Expression is true. It may also optionally specify an elseAction ActionUsage that is performed if the result of the ifArgument is false.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### elseAction : ActionUsage [0..1]

The ActionUsage that is to be performed if the result of the ifArgument is false. It is the (optional) third parameter of the IfActionUsage.

### ifArgument : &#171;untyped&#187; [1..1]

The Expression whose result determines whether the thenAction or (optionally) the elseAction is performed. It is the first parameter of the IfActionUsage.

### thenAction : ActionUsage [1..1]

The ActionUsage that is to be performed if the result of the ifArgument is true. It is the second parameter of the IfActionUsage.


## Constraints

- **checkIfActionUsageSpecialization**
- **checkIfActionUsageSubactionSpecialization**
- **deriveIfActionUsageElseAction**
- **deriveIfActionUsageIfArgument**
- **deriveIfActionUsageThenAction**
- **validateIfActionUsageParameters**
