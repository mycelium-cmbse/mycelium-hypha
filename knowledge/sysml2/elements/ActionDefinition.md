# ActionDefinition

`Actions` package · concrete metaclass

An ActionDefinition is a Definition that is also a Behavior that defines an Action performed by a system or part of a system.

## Generalizations

- [OccurrenceDefinition](OccurrenceDefinition.md)

## Owned features

### action : ActionUsage [0..*]

The ActionUsages that are steps in this ActionDefinition, which define the actions that specify the behavior of the ActionDefinition.


## Constraints

- **checkActionDefinitionSpecialization**
- **deriveActionDefinitionAction**
