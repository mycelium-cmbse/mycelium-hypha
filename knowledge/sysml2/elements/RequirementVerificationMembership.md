---
name: RequirementVerificationMembership
package: VerificationCases
fully qualified name: SysML::Systems::VerificationCases::RequirementVerificationMembership
isAbstract: false
visibility: public
generalizes: [RequirementConstraintMembership]
specializedBy: []
---

# RequirementVerificationMembership

`VerificationCases` package · concrete metaclass

A RequirementVerificationMembership is a RequirementConstraintMembership used in the objective of a VerificationCase to identify a RequirementUsage that is verified by the VerificationCase.

## Generalizations

- [RequirementConstraintMembership](RequirementConstraintMembership.md)

## Owned features

### kind

`+` RequirementConstraintKind · `[1..1]`

The kind of a RequirementVerificationMembership must be requirement.

Redefines [kind](#kind)

### ownedRequirement

`+` [RequirementUsage](RequirementUsage.md) · `[1..1]` · *derived, composite*

The owned RequirementUsage that acts as the ownedConstraint for this RequirementVerificationMembership. This will either be the verifiedRequirement, or it will subset the verifiedRequirement.

Redefines [ownedConstraint](RequirementConstraintMembership.md#ownedconstraint)

### verifiedRequirement

`+` [RequirementUsage](RequirementUsage.md) · `[1..1]` · *derived*

The RequirementUsage that is identified as being verified. It is the referencedConstraint of the RequirementVerificationMembership considered as a RequirementConstraintMembership, which must be a RequirementUsage.

Redefines [referencedConstraint](RequirementConstraintMembership.md#referencedconstraint)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| memberElement | [Element](Element.md) | [1..1] | [Membership](Membership.md) |  |
| memberElementId | String | [1..1] | [Membership](Membership.md) | derived |
| memberName | String | [0..1] | [Membership](Membership.md) |  |
| memberShortName | String | [0..1] | [Membership](Membership.md) |  |
| membershipOwningNamespace | [Namespace](Namespace.md) | [1..1] | [Membership](Membership.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedConstraint | [ConstraintUsage](ConstraintUsage.md) | [1..1] | [RequirementConstraintMembership](RequirementConstraintMembership.md) | derived, composite |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedMemberElement | [Element](Element.md) | [1..1] | [OwningMembership](OwningMembership.md) | derived, composite |
| ownedMemberElementId | String | [1..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberFeature | [Feature](Feature.md) | [1..1] | [FeatureMembership](FeatureMembership.md) | derived, composite |
| ownedMemberName | String | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberShortName | String | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [1..1] | [FeatureMembership](FeatureMembership.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| referencedConstraint | [ConstraintUsage](ConstraintUsage.md) | [1..1] | [RequirementConstraintMembership](RequirementConstraintMembership.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| visibility | VisibilityKind | [1..1] | [Membership](Membership.md) |  |

## Constraints

### validateRequirementVerificationMembershipKind

A RequirementVerificationMembership must have kind = requirement.

```ocl
kind = RequirementConstraintKind::requirement
```

### validateRequirementVerificationMembershipOwningType

The owningType of a RequirementVerificationMembership must a RequirementUsage that is owned by an ObjectiveMembership.

```ocl
owningType.oclIsKindOf(RequirementUsage) and
owningType.owningFeatureMembership <> null and
owningType.owningFeatureMembership.oclIsKindOf(ObjectiveMembership)
```

