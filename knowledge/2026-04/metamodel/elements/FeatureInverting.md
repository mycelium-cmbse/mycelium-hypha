---
name: FeatureInverting
package: Features
fully qualified name: KerML::Core::Features::FeatureInverting
isAbstract: false
visibility: public
generalizes: [Relationship]
specializedBy: []
---

# FeatureInverting

`Features` package · concrete metaclass

A FeatureInverting is a Relationship between Features asserting that their interpretations (sequences) are the reverse of each other, identified as featureInverted and invertingFeature. For example, a Feature identifying each person&#39;s parents is the inverse of a Feature identifying each person&#39;s children. A person identified as a parent of another will identify that other as one of their children.

## Generalizations

- [Relationship](Relationship.md)

## Owned features

### featureInverted

`+` [Feature](Feature.md) · `[1..1]`

The Feature that is an inverse of the invertingFeature.

Redefines [source](Relationship.md#source)

### invertingFeature

`+` [Feature](Feature.md) · `[1..1]`

The Feature that is an inverse of the invertedFeature.

Redefines [target](Relationship.md#target)

### owningFeature

`+` [Feature](Feature.md) · `[0..1]` · *derived*

A featureInverted that is also the owningRelatedElement of this FeatureInverting.

Subsets [featureInverted](#featureinverted), [owningRelatedElement](Relationship.md#owningrelatedelement)


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
