# AttributeUsage

`Attributes` package · concrete metaclass

An AttributeUsage is a Usage whose type is a DataType. Nominally, if the type is an AttributeDefinition, an AttributeUsage is a usage of a AttributeDefinition to represent the value of some system quality or characteristic. However, other kinds of kernel DataTypes are also allowed, to permit use of DataTypes from the Kernel Model Libraries. An AttributeUsage itself as well as all its nested features must be referential (non-composite).An AttributeUsage must specialize, directly or indirectly, the base Feature Base::dataValues from the Kernel Semantic Library.

## Generalizations

- [Usage](Usage.md)

## Owned features

### attributeDefinition : &#171;untyped&#187; [0..*]

The DataTypes that are the types of this AttributeUsage. Nominally, these are AttributeDefinitions, but other kinds of kernel DataTypes are also allowed, to permit use of DataTypes from the Kernel Model Libraries.

Redefines: `definition`

### isReference : Boolean [1..1]

Always true for an AttributeUsage.

Redefines: `isReference`


## Constraints

- **checkAttributeUsageSpecialization**
- **validateAttributeUsageFeatures**
- **validateAttributeUsageIsReference**
