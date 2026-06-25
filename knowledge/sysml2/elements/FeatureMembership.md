---
name: FeatureMembership
package: Types
isAbstract: false
generalizes: [OwningMembership]
specializedBy: [EndFeatureMembership, ObjectiveMembership, ParameterMembership, RequirementConstraintMembership, ResultExpressionMembership, StateSubactionMembership, TransitionFeatureMembership, ViewRenderingMembership]
---

# FeatureMembership

`Types` package · concrete metaclass

A FeatureMembership is an OwningMembership between an ownedMemberFeature and an owningType. If the ownedMemberFeature has isVariable = false, then the FeatureMembership implies that the owningType is also a featuringType of the ownedMemberFeature. If the ownedMemberFeature has isVariable = true, then the FeatureMembership implies that the ownedMemberFeature is featured by the snapshots of the owningType, which must specialize the Kernel Semantic Library base class Occurrence.

## Generalizations

- [OwningMembership](OwningMembership.md)

## Specializations

- [EndFeatureMembership](EndFeatureMembership.md)
- [ObjectiveMembership](ObjectiveMembership.md)
- [ParameterMembership](ParameterMembership.md)
- [RequirementConstraintMembership](RequirementConstraintMembership.md)
- [ResultExpressionMembership](ResultExpressionMembership.md)
- [StateSubactionMembership](StateSubactionMembership.md)
- [TransitionFeatureMembership](TransitionFeatureMembership.md)
- [ViewRenderingMembership](ViewRenderingMembership.md)

## Owned features

### ownedMemberFeature : Feature [1..1] {derived, composite}

The Feature that this FeatureMembership relates to its owningType, making it an ownedFeature of the owningType.

Redefines: `ownedMemberElement`

### owningType : Type [1..1] {derived}

The Type that owns this FeatureMembership.

Redefines: `membershipOwningNamespace`

Subsets: `type`


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
| memberElement | Element | [1..1] | [Membership](Membership.md) |  |
| memberElementId | String | [1..1] | [Membership](Membership.md) | derived |
| memberName | String | [0..1] | [Membership](Membership.md) |  |
| memberShortName | String | [0..1] | [Membership](Membership.md) |  |
| membershipOwningNamespace | Namespace | [1..1] | [Membership](Membership.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedMemberElement | Element | [1..1] | [OwningMembership](OwningMembership.md) | derived, composite |
| ownedMemberElementId | String | [1..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberName | String | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberShortName | String | [0..1] | [OwningMembership](OwningMembership.md) | derived |
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
| visibility | VisibilityKind | [1..1] | [Membership](Membership.md) |  |
