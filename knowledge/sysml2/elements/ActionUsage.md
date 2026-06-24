# ActionUsage

`Actions` package · concrete metaclass

An ActionUsage is a Usage that is also a Step, and, so, is typed by a Behavior. Nominally, if the type is an ActionDefinition, an ActionUsage is a Usage of that ActionDefinition within a system. However, other kinds of kernel Behaviors are also allowed, to permit use of Behaviors from the Kernel Model Libraries.

## Generalizations

- [OccurrenceUsage](OccurrenceUsage.md)

## Owned features

### actionDefinition : &#171;untyped&#187; [0..*]

The Behaviors that are the types of this ActionUsage. Nominally, these would be ActionDefinitions, but other kinds of Kernel Behaviors are also allowed, to permit use of Behaviors from the Kernel Model Libraries.


## Constraints

- **checkActionUsageOwnedActionSpecialization**
- **checkActionUsageSpecialization**
- **checkActionUsageStateActionRedefinition**
- **checkActionUsageSubactionSpecialization**
