# EnumerationDefinition

`Enumerations` package · concrete metaclass

An EnumerationDefinition is an AttributeDefinition all of whose instances are given by an explicit list of enumeratedValues. This is realized by requiring that the EnumerationDefinition have isVariation = true, with the enumeratedValues being its variants.

## Generalizations

- [AttributeDefinition](AttributeDefinition.md)

## Owned features

### enumeratedValue : EnumerationUsage [0..*]

EnumerationUsages of this EnumerationDefinitionthat have distinct, fixed values. Each enumeratedValue specifies one of the allowed instances of the EnumerationDefinition.

Redefines: `variant`

### isVariation : Boolean [1..1]

An EnumerationDefinition is considered semantically to be a variation whose allowed variants are its enumerationValues.

Redefines: `isVariation`


## Constraints

- **validateEnumerationDefinitionIsVariation**
