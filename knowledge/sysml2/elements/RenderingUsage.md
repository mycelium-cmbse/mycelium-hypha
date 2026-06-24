# RenderingUsage

`Views` package · concrete metaclass

A RenderingUsage is the usage of a RenderingDefinition to specify the rendering of a specific model view to produce a physical view artifact.

## Generalizations

- [PartUsage](PartUsage.md)

## Owned features

### renderingDefinition : RenderingDefinition [0..1]

The RenderingDefinition that is the definition of this RenderingUsage.

Redefines: `partDefinition`


## Constraints

- **checkRenderingUsageRedefinition**
- **checkRenderingUsageSpecialization**
- **checkRenderingUsageSubrenderingSpecialization**
