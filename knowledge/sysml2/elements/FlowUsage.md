# FlowUsage

`Flows` package · concrete metaclass

A FlowUsage is an ActionUsage that is also a ConnectorAsUsage and a KerML Flow.

## Generalizations

- [ActionUsage](ActionUsage.md)
- [ConnectorAsUsage](ConnectorAsUsage.md)

## Owned features

### flowDefinition : &#171;untyped&#187; [0..*]

The Interactions that are the types of this FlowUsage. Nominally, these are FlowDefinitions, but other kinds of Kernel Interactions are also allowed, to permit use of Interactions from the Kernel Model Libraries.


## Constraints

- **checkFlowUsageFlowSpecialization**
- **checkFlowUsageSpecialization**
