# ViewpointUsage

`Views` package · concrete metaclass

A ViewpointUsage is a Usage of a ViewpointDefinition.

## Generalizations

- [RequirementUsage](RequirementUsage.md)

## Owned features

### viewpointDefinition : ViewpointDefinition [0..1]

The ViewpointDefinition that is the definition of this ViewpointUsage.

Redefines: `requirementDefinition`

### viewpointStakeholder : PartUsage [0..*]

The PartUsages that identify the stakeholders with concerns framed by this ViewpointUsage, which are the owned and inherited stakeholderParameters of the framedConcerns of this ViewpointUsage.


## Constraints

- **checkViewpointUsageSpecialization**
- **checkViewpointUsageViewpointSatisfactionSpecialization**
- **deriveViewpointUsageViewpointStakeholder**
