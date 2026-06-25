---
name: ParameterMembership
package: Behaviors
isAbstract: false
generalizes: [FeatureMembership]
specializedBy: [ActorMembership, ReturnParameterMembership, StakeholderMembership, SubjectMembership]
---

# ParameterMembership

`Behaviors` package · concrete metaclass

A ParameterMembership is a FeatureMembership that identifies its memberFeature as a parameter, which is always owned, and must have a direction. A ParameterMembership must be owned by a Behavior, a Step, or the result parameter of a ConstructorExpression.

## Generalizations

- [FeatureMembership](FeatureMembership.md)

## Specializations

- [ActorMembership](ActorMembership.md)
- [ReturnParameterMembership](ReturnParameterMembership.md)
- [StakeholderMembership](StakeholderMembership.md)
- [SubjectMembership](SubjectMembership.md)

## Owned features

### ownedMemberParameter : Feature [1..1] {derived, composite}

The Feature that is identified as a parameter by this ParameterMembership.

Redefines: `ownedMemberFeature`


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
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| visibility | VisibilityKind | [1..1] | [Membership](Membership.md) |  |

## Constraints

### validateParameterMembershipOwningType

A ParameterMembership must be owned by a Behavior,Step, or the result parameter of a ConstructorExpression.

```ocl
owningType.oclIsKindOf(Behavior) or owningType.oclIsKindOf(Step) or
owningType.owningMembership.oclIsKindOf(ReturnParameterMembership) and
    owningType.owningNamespace.oclIsKindOf(ConstructorExpression)
```

### validateParameterMembershipParameterDirection

The ownedMemberParameter of a ParameterMembership must have a direction equal to the result of the parameterDirection() operation.

```ocl
ownedMemberParameter.direction = parameterDirection()
```

