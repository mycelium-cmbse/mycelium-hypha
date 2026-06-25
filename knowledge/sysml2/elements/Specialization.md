---
name: Specialization
package: Types
isAbstract: false
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

### general : Type [1..1]

A Type with a superset of all instances of the specific Type, which might be the same set.

Redefines: `target`

### owningType : Type [0..1] {derived}

The Type that is the specific Type of this Specialization and owns it as its owningRelatedElement.

Subsets: `owningRelatedElement`, `specific`

### specific : Type [1..1]

A Type with a subset of all instances of the general Type, which might be the same set.

Redefines: `source`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelatedElement | Element | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | Element | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |

## Constraints

### validateSpecificationSpecificNotConjugated

The specific Type of a Specialization cannot be a conjugated Type.

```ocl
not specific.isConjugated
```

