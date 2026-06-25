---
name: TransitionFeatureMembership
package: States
isAbstract: false
generalizes: [FeatureMembership]
specializedBy: []
---

# TransitionFeatureMembership

`States` package · concrete metaclass

A TransitionFeatureMembership is a FeatureMembership for a trigger, guard or effect of a TransitionUsage, whose transitionFeature is a AcceptActionUsage, Boolean-valued Expression or ActionUsage, depending on its kind.

## Generalizations

- [FeatureMembership](FeatureMembership.md)

## Owned features

### kind : TransitionFeatureKind [1..1]

Whether this TransitionFeatureMembership is for a trigger, guard or effect.

### transitionFeature : Step [1..1] {derived, composite}

The Step that is the ownedMemberFeature of this TransitionFeatureMembership.

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

### validateTransitionFeatureMembershipEffectAction

If the kind of a TransitionUsage is effect, then its transitionFeature must be a kind of ActionUsage.

```ocl
kind = TransitionFeatureKind::effect implies
    transitionFeature.oclIsKindOf(ActionUsage)
```

### validateTransitionFeatureMembershipGuardExpression

If the kind of a TransitionUsage is guard, then its transitionFeature must be a kind of Expression whose result is a Boolean value.

```ocl
kind = TransitionFeatureKind::guard implies
    transitionFeature.oclIsKindOf(Expression) and
    let guard : Expression = transitionFeature.oclIsKindOf(Expression) in
    guard.result.specializesFromLibrary('ScalarValues::Boolean') and
    guard.result.multiplicity <> null and
    guard.result.multiplicity.hasBounds(1,1)
```

### validateTransitionFeatureMembershipOwningType

The owningType of a TransitionFeatureMembership must be a TransitionUsage.

```ocl
owningType.oclIsKindOf(TransitionUsage)
```

### validateTransitionFeatureMembershipTriggerAction

If the kind of a TransitionUsage is trigger, then its transitionFeature must be a kind of AcceptActionUsage.

```ocl
kind = TransitionFeatureKind::trigger implies
    transitionFeature.oclIsKindOf(AcceptActionUsage)
```

