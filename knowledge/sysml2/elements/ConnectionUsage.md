# ConnectionUsage

`Connections` package · concrete metaclass

A ConnectionUsage is a ConnectorAsUsage that is also a PartUsage. Nominally, if its type is a ConnectionDefinition, then a ConnectionUsage is a Usage of that ConnectionDefinition, representing a connection between parts of a system. However, other kinds of kernel AssociationStructures are also allowed, to permit use of AssociationStructures from the Kernel Model Libraries.

## Generalizations

- [ConnectorAsUsage](ConnectorAsUsage.md)
- [PartUsage](PartUsage.md)

## Owned features

### connectionDefinition : &#171;untyped&#187; [0..*]

The AssociationStructures that are the types of this ConnectionUsage. Nominally, these are , but other kinds of Kernel AssociationStructures are also allowed, to permit use of AssociationStructures from the Kernel Model Libraries

Subsets: `itemDefinition`


## Constraints

- **checkConnectionUsageBinarySpecialization**
- **checkConnectionUsageSpecialization**
