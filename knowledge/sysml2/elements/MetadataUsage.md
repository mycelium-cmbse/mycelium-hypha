# MetadataUsage

`Metadata` package · concrete metaclass

A MetadataUsage is a Usage and a MetadataFeature, used to annotate other Elements in a system model with metadata. As a MetadataFeature, its type must be a Metaclass, which will nominally be a MetadataDefinition. However, any kernel Metaclass is also allowed, to permit use of Metaclasses from the Kernel Model Libraries.

## Generalizations

- [ItemUsage](ItemUsage.md)

## Owned features

### metadataDefinition : &#171;untyped&#187; [0..1]

The MetadataDefinition that is the definition of this MetadataUsage.


## Constraints

- **checkMetadataUsageSpecialization**
