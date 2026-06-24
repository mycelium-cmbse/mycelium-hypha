# ViewpointDefinition

`Views` package · concrete metaclass

A ViewpointDefinition is a RequirementDefinition that specifies one or more stakeholder concerns that are to be satisfied by creating a view of a model.

## Generalizations

- [RequirementDefinition](RequirementDefinition.md)

## Owned features

### viewpointStakeholder : PartUsage [0..*]

The PartUsages that identify the stakeholders with concerns framed by this ViewpointDefinition, which are the owned and inherited stakeholderParameters of the framedConcerns of this ViewpointDefinition.


## Constraints

- **checkViewpointDefinitionSpecialization**
- **deriveViewpointDefinitionViewpointStakeholder**
