# ViewUsage

`Views` package · concrete metaclass

A ViewUsage is a usage of a ViewDefinition to specify the generation of a view of the members of a collection of exposedNamespaces. The ViewUsage can satisfy more viewpoints than its definition, and it can specialize the viewRendering specified by its definition.

## Generalizations

- [PartUsage](PartUsage.md)

## Owned features

### exposedElement : &#171;untyped&#187; [0..*]

The Elements that are exposed by this ViewUsage, which are those memberElements of the imported Memberships from all the Expose Relationships that meet all the owned and inherited viewConditions.

### satisfiedViewpoint : ViewpointUsage [0..*]

The nestedRequirements of this ViewUsage that are ViewpointUsages for (additional) viewpoints satisfied by the ViewUsage.

Subsets: `nestedRequirement`

### viewCondition : &#171;untyped&#187; [0..*]

The Expressions related to this ViewUsage by ElementFilterMemberships, which specify conditions on Elements to be rendered in a view.

### viewDefinition : ViewDefinition [0..1]

The ViewDefinition that is the definition of this ViewUsage.

Redefines: `partDefinition`

### viewRendering : RenderingUsage [0..1]

The RenderingUsage to be used to render views defined by this ViewUsage, which is the referencedRendering of the ViewRenderingMembership of the ViewUsage.


## Constraints

- **checkViewUsageSpecialization**
- **checkViewUsageSubviewSpecialization**
- **deriveViewUsageExposedElement**
- **deriveViewUsageSatisfiedViewpoint**
- **deriveViewUsageViewCondition**
- **deriveViewUsageViewRendering**
- **validateViewUsageOnlyOneViewRendering**
