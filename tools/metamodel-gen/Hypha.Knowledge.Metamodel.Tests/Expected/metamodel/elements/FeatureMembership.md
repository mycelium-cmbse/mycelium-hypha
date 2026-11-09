---
name: FeatureMembership
package: Types
fully qualified name: KerML::Core::Types::FeatureMembership
isAbstract: false
visibility: public
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

### ownedMemberFeature

`+` [Feature](Feature.md) · `[1..1]` · *derived, composite*

The Feature that this FeatureMembership relates to its owningType, making it an ownedFeature of the owningType.

Redefines [ownedMemberElement](OwningMembership.md#ownedmemberelement)

### owningType

`+` [Type](Type.md) · `[1..1]` · *derived*

The Type that owns this FeatureMembership.

Redefines [membershipOwningNamespace](Membership.md#membershipowningnamespace)

Subsets `type`


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
| memberElement | [Element](Element.md) | [1..1] | [Membership](Membership.md) |  |
| memberElementId | [String](String.md) | [1..1] | [Membership](Membership.md) | derived |
| memberName | [String](String.md) | [0..1] | [Membership](Membership.md) |  |
| memberShortName | [String](String.md) | [0..1] | [Membership](Membership.md) |  |
| membershipOwningNamespace | [Namespace](Namespace.md) | [1..1] | [Membership](Membership.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedMemberElement | [Element](Element.md) | [1..1] | [OwningMembership](OwningMembership.md) | derived, composite |
| ownedMemberElementId | [String](String.md) | [1..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberName | [String](String.md) | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberShortName | [String](String.md) | [0..1] | [OwningMembership](OwningMembership.md) | derived |
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
| visibility | [VisibilityKind](VisibilityKind.md) | [1..1] | [Membership](Membership.md) |  |
