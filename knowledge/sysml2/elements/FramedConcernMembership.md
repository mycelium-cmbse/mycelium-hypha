---
name: FramedConcernMembership
package: Requirements
isAbstract: false
generalizes: [RequirementConstraintMembership]
specializedBy: []
---

# FramedConcernMembership

`Requirements` package · concrete metaclass

A FramedConcernMembership is a RequirementConstraintMembership for a framed ConcernUsage of a RequirementDefinition or RequirementUsage.

## Generalizations

- [RequirementConstraintMembership](RequirementConstraintMembership.md)

## Owned features

### kind : RequirementConstraintKind [1..1]

The kind of an FramedConcernMembership must be requirement.

Redefines: `kind`

### ownedConcern : ConcernUsage [1..1] {derived, composite}

The ConcernUsage that is the ownedConstraint of this FramedConcernMembership.

Redefines: `ownedConstraint`

### referencedConcern : ConcernUsage [1..1] {derived}

The ConcernUsage that is referenced through this FramedConcernMembership. It is the referencedConstraint of the FramedConcernMembership considered as a RequirementConstraintMembership, which must be a ConcernUsage.

Redefines: `referencedConstraint`


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
| ownedConstraint | ConstraintUsage | [1..1] | [RequirementConstraintMembership](RequirementConstraintMembership.md) | derived, composite |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedMemberElement | Element | [1..1] | [OwningMembership](OwningMembership.md) | derived, composite |
| ownedMemberElementId | String | [1..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberFeature | Feature | [1..1] | [FeatureMembership](FeatureMembership.md) | derived, composite |
| ownedMemberName | String | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberShortName | String | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedRelatedElement | Element | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | Element | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| owningType | Type | [1..1] | [FeatureMembership](FeatureMembership.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| referencedConstraint | ConstraintUsage | [1..1] | [RequirementConstraintMembership](RequirementConstraintMembership.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| visibility | VisibilityKind | [1..1] | [Membership](Membership.md) |  |

## Constraints

### validateFramedConcernMembershipConstraintKind

A FramedConcernMembership must have kind = requirement.

```ocl
kind = RequirementConstraintKind::requirement
```

