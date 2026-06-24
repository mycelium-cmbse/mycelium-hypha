# InterfaceDefinition

`Interfaces` package · concrete metaclass

An InterfaceDefinition is a ConnectionDefinition all of whose ends are PortUsages, defining an interface between elements that interact through such ports.

## Generalizations

- [ConnectionDefinition](ConnectionDefinition.md)

## Owned features

### interfaceEnd : PortUsage [0..*]

The PortUsages that are the connectionEnds of this InterfaceDefinition.

Redefines: `connectionEnd`


## Constraints

- **checkInterfaceDefinitionBinarySpecialization**
- **checkInterfaceDefinitionSpecialization**
