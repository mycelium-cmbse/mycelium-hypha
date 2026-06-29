---
name: TransitionFeatureMembership
package: States
fully qualified name: SysML::Systems::States::TransitionFeatureMembership
isAbstract: false
visibility: public
generalizes: [FeatureMembership]
specializedBy: []
---

# TransitionFeatureMembership

`States` package · concrete metaclass

A TransitionFeatureMembership is a FeatureMembership for a trigger, guard or effect of a TransitionUsage, whose transitionFeature is a AcceptActionUsage, Boolean-valued Expression or ActionUsage, depending on its kind.

## Generalizations

- [FeatureMembership](FeatureMembership.md)

## Owned features

### kind

`+` [TransitionFeatureKind](TransitionFeatureKind.md) · `[1..1]`

Whether this TransitionFeatureMembership is for a trigger, guard or effect.

### transitionFeature

`+` [Step](Step.md) · `[1..1]` · *derived, composite*

The Step that is the ownedMemberFeature of this TransitionFeatureMembership.

Redefines [ownedMemberFeature](FeatureMembership.md#ownedmemberfeature)


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
| ownedMemberFeature | [Feature](Feature.md) | [1..1] | [FeatureMembership](FeatureMembership.md) | derived, composite |
| ownedMemberName | [String](String.md) | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberShortName | [String](String.md) | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [1..1] | [FeatureMembership](FeatureMembership.md) | derived |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| visibility | [VisibilityKind](VisibilityKind.md) | [1..1] | [Membership](Membership.md) |  |

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

