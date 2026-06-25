---
name: Specialization
package: Types
fully qualified name: KerML::Core::Types::Specialization
isAbstract: false
visibility: public
generalizes: [Relationship]
specializedBy: [FeatureTyping, Subclassification, Subsetting]
---

# Specialization

`Types` package · concrete metaclass

Specialization is a Relationship between two Types that requires all instances of the specific type to also be instances of the general Type (i.e., the set of instances of the specific Type is a subset of those of the general Type, which might be the same set).

## Generalizations

- [Relationship](Relationship.md)

## Specializations

- [FeatureTyping](FeatureTyping.md)
- [Subclassification](Subclassification.md)
- [Subsetting](Subsetting.md)

## Owned features

### general

`+` [Type](Type.md) · `[1..1]`

A Type with a superset of all instances of the specific Type, which might be the same set.

Redefines [target](Relationship.md#target)

### owningType

`+` [Type](Type.md) · `[0..1]` · *derived*

The Type that is the specific Type of this Specialization and owns it as its owningRelatedElement.

Subsets [owningRelatedElement](Relationship.md#owningrelatedelement), [specific](#specific)

### specific

`+` [Type](Type.md) · `[1..1]`

A Type with a subset of all instances of the general Type, which might be the same set.

Redefines [source](Relationship.md#source)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| isImplied | [Boolean](Boolean.md) | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |

## Constraints

### validateSpecificationSpecificNotConjugated

The specific Type of a Specialization cannot be a conjugated Type.

```ocl
not specific.isConjugated
```

