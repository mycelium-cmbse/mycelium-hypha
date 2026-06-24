# ViewDefinition

`Views` package · concrete metaclass

A ViewDefinition is a PartDefinition that specifies how a view artifact is constructed to satisfy a viewpoint. It specifies a viewConditions to define the model content to be presented and a viewRendering to define how the model content is presented.

## Generalizations

- [PartDefinition](PartDefinition.md)

## Owned features

### satisfiedViewpoint : ViewpointUsage [0..*]

The composite ownedRequirements of this ViewDefinition that are ViewpointUsages for viewpoints satisfied by the ViewDefinition.

Subsets: `ownedRequirement`

### view : ViewUsage [0..*]

The usages of this ViewDefinition that are ViewUsages.

Subsets: `usage`

### viewCondition : &#171;untyped&#187; [0..*]

The Expressions related to this ViewDefinition by ElementFilterMemberships, which specify conditions on Elements to be rendered in a view.

### viewRendering : RenderingUsage [0..1]

The RenderingUsage to be used to render views defined by this ViewDefinition, which is the referencedRendering of the ViewRenderingMembership of the ViewDefinition.


## Constraints

- **checkViewDefinitionSpecialization**
- **deriveViewDefinitionSatisfiedViewpoint**
- **deriveViewDefinitionView**
- **deriveViewDefinitionViewCondition**
- **deriveViewDefinitionViewRendering**
- **validateViewDefinitionOnlyOneViewRendering**
