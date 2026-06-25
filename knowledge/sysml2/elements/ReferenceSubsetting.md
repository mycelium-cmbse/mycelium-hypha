---
name: ReferenceSubsetting
package: Features
fully qualified name: KerML::Core::Features::ReferenceSubsetting
isAbstract: false
visibility: public
generalizes: [Subsetting]
specializedBy: []
---

# ReferenceSubsetting

`Features` package · concrete metaclass

ReferenceSubsetting is a kind of Subsetting in which the referencedFeature is syntactically distinguished from other Features subsetted by the referencingFeature. ReferenceSubsetting has the same semantics as Subsetting, but the referencedFeature may have a special purpose relative to the referencingFeature. For instance, ReferenceSubsetting is used to identify the relatedFeatures of a Connector.ReferenceSubsetting is always an ownedRelationship of its referencingFeature. A Feature can have at most one ownedReferenceSubsetting.

## Generalizations

- [Subsetting](Subsetting.md)

## Owned features

### referencedFeature

`+` [Feature](Feature.md) · `[1..1]`

The Feature that is referenced by the referencingFeature of this ReferenceSubsetting.

Redefines [subsettedFeature](Subsetting.md#subsettedfeature)

### referencingFeature

`+` [Feature](Feature.md) · `[1..1]` · *derived*

The Feature that owns this ReferenceSubsetting relationship, which is also its subsettingFeature.

Redefines [owningFeature](Subsetting.md#owningfeature), [subsettingFeature](Subsetting.md#subsettingfeature)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| general | [Type](Type.md) | [1..1] | [Specialization](Specialization.md) |  |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningFeature | [Feature](Feature.md) | [0..1] | [Subsetting](Subsetting.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Specialization](Specialization.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| specific | [Type](Type.md) | [1..1] | [Specialization](Specialization.md) |  |
| subsettedFeature | [Feature](Feature.md) | [1..1] | [Subsetting](Subsetting.md) |  |
| subsettingFeature | [Feature](Feature.md) | [1..1] | [Subsetting](Subsetting.md) |  |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
