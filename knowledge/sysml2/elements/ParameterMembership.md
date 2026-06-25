---
name: ParameterMembership
package: Behaviors
fully qualified name: KerML::Kernel::Behaviors::ParameterMembership
isAbstract: false
visibility: public
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

### ownedMemberParameter

`+` [Feature](Feature.md) · `[1..1]` · *derived, composite*

The Feature that is identified as a parameter by this ParameterMembership.

Redefines [ownedMemberFeature](FeatureMembership.md#ownedmemberfeature)


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
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
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

