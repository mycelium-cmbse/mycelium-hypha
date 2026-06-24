# ConnectionDefinition

`Connections` package · concrete metaclass

A ConnectionDefinition is a PartDefinition that is also an AssociationStructure. The end Features of a ConnectionDefinition must be Usages.

## Generalizations

- [PartDefinition](PartDefinition.md)

## Owned features

### connectionEnd : Usage [0..*]

The Usages that define the things related by the ConnectionDefinition.

### isSufficient : Boolean [1..1]

A ConnectionDefinition always has isSufficient = true.


## Constraints

- **checkConnectionDefinitionBinarySpecialization**
- **checkConnectionDefinitionSpecializations**
- **validateConnectionDefinitionIsSufficient**
