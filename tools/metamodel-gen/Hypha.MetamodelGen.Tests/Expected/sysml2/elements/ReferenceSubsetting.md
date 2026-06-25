---
name: ReferenceSubsetting
package: Features
isAbstract: false
generalizes: [Subsetting]
specializedBy: []
---

# ReferenceSubsetting

`Features` package · concrete metaclass

ReferenceSubsetting is a kind of Subsetting in which the referencedFeature is syntactically distinguished from other Features subsetted by the referencingFeature. ReferenceSubsetting has the same semantics as Subsetting, but the referencedFeature may have a special purpose relative to the referencingFeature. For instance, ReferenceSubsetting is used to identify the relatedFeatures of a Connector.ReferenceSubsetting is always an ownedRelationship of its referencingFeature. A Feature can have at most one ownedReferenceSubsetting.

## Generalizations

- [Subsetting](Subsetting.md)

## Owned features

### referencedFeature : Feature [1..1]

The Feature that is referenced by the referencingFeature of this ReferenceSubsetting.

Redefines: `subsettedFeature`

### referencingFeature : Feature [1..1] {derived}

The Feature that owns this ReferenceSubsetting relationship, which is also its subsettingFeature.

Redefines: `owningFeature`, `subsettingFeature`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| general | Type | [1..1] | [Specialization](Specialization.md) |  |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelatedElement | Element | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningFeature | Feature | [0..1] | [Subsetting](Subsetting.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | Element | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| owningType | Type | [0..1] | [Specialization](Specialization.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| specific | Type | [1..1] | [Specialization](Specialization.md) |  |
| subsettedFeature | Feature | [1..1] | [Subsetting](Subsetting.md) |  |
| subsettingFeature | Feature | [1..1] | [Subsetting](Subsetting.md) |  |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
