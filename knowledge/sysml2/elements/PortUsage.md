# PortUsage

`Ports` package · concrete metaclass

A PortUsage is a usage of a PortDefinition. A PortUsage itself as well as all its nestedUsages must be referential (non-composite).

## Generalizations

- [OccurrenceUsage](OccurrenceUsage.md)

## Owned features

### portDefinition : PortDefinition [0..*]

The occurrenceDefinitions of this PortUsage, which must all be PortDefinitions.

Redefines: `occurrenceDefinition`


## Constraints

- **checkPortUsageOwnedPortSpecialization**
- **checkPortUsageSpecialization**
- **checkPortUsageSubportSpecialization**
- **validatePortUsageIsReference**
- **validatePortUsageNestedUsagesNotComposite**
