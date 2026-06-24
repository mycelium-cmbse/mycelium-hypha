# InterfaceUsage

`Interfaces` package · concrete metaclass

An InterfaceUsage is a Usage of an InterfaceDefinition to represent an interface connecting parts of a system through specific ports.

## Generalizations

- [ConnectionUsage](ConnectionUsage.md)

## Owned features

### interfaceDefinition : InterfaceDefinition [0..*]

The InterfaceDefinitions that type this InterfaceUsage.

Redefines: `connectionDefinition`


## Constraints

- **checkInterfaceUsageBinarySpecialization**
- **checkInterfaceUsageSpecialization**
