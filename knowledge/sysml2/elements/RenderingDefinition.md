# RenderingDefinition

`Views` package · concrete metaclass

A RenderingDefinition is a PartDefinition that defines a specific rendering of the content of a model view (e.g., symbols, style, layout, etc.).

## Generalizations

- [PartDefinition](PartDefinition.md)

## Owned features

### rendering : RenderingUsage [0..*]

The usages of a RenderingDefinition that are RenderingUsages.

Subsets: `usage`


## Constraints

- **checkRenderingDefinitionSpecialization**
- **deriveRenderingDefinitionRendering**
