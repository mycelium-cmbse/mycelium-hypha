# ItemUsage

`Items` package · concrete metaclass

An ItemUsage is an OccurrenceUsage whose definition is a Structure. Nominally, if the definition is an ItemDefinition, an ItemUsage is a ItemUsage of that ItemDefinition within a system. However, other kinds of Kernel Structures are also allowed, to permit use of Structures from the Kernel Model Libraries.

## Generalizations

- [OccurrenceUsage](OccurrenceUsage.md)

## Owned features

### itemDefinition : &#171;untyped&#187; [0..*]

The Structures that are the definitions of this ItemUsage. Nominally, these are ItemDefinitions, but other kinds of Kernel Structures are also allowed, to permit use of Structures from the Kernel Library.

Subsets: `occurrenceDefinition`


## Constraints

- **checkItemUsageSpecialization**
- **checkItemUsageSubitemSpecialization**
- **deriveItemUsageItemDefinition**
