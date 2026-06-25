---
name: TypeFeaturing
package: Features
fully qualified name: KerML::Core::Features::TypeFeaturing
isAbstract: false
visibility: public
generalizes: [Relationship]
specializedBy: []
---

# TypeFeaturing

`Features` package · concrete metaclass

A TypeFeaturing is a Featuring Relationship in which the featureOfType is the source and the featuringType is the target.

## Generalizations

- [Relationship](Relationship.md)

## Owned features

### featureOfType

`+` [Feature](Feature.md) · `[1..1]`

The Feature that is featured by the featuringType. It is the source of the TypeFeaturing.

Redefines [source](Relationship.md#source)

### featuringType

`+` [Type](Type.md) · `[1..1]`

The Type that features the featureOfType. It is the target of the TypeFeaturing.

Redefines [target](Relationship.md#target)

### owningFeatureOfType

`+` [Feature](Feature.md) · `[0..1]` · *derived*

A featureOfType that is also the owningRelatedElement of this TypeFeaturing.

Subsets [owningRelatedElement](Relationship.md#owningrelatedelement), [featureOfType](#featureoftype)


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
