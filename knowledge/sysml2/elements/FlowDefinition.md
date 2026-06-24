# FlowDefinition

`Flows` package · concrete metaclass

A FlowDefinition is an ActionDefinition that is also an Interaction (which is both a KerML Behavior and Association), representing flows between Usages.

## Generalizations

- [ActionDefinition](ActionDefinition.md)

## Owned features

### flowEnd : Usage [0..*]

The Usages that define the things related by the FlowDefinition.


## Constraints

- **checkFlowDefinitionBinarySpecialization**
- **checkFlowDefinitionSpecialization**
- **validateFlowDefinitionFlowEnds**
