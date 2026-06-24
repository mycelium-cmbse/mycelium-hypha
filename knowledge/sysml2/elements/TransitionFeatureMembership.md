# TransitionFeatureMembership

`States` package · concrete metaclass

A TransitionFeatureMembership is a FeatureMembership for a trigger, guard or effect of a TransitionUsage, whose transitionFeature is a AcceptActionUsage, Boolean-valued Expression or ActionUsage, depending on its kind.

## Owned features

### kind : TransitionFeatureKind [1..1]

Whether this TransitionFeatureMembership is for a trigger, guard or effect.

### transitionFeature : &#171;untyped&#187; [1..1]

The Step that is the ownedMemberFeature of this TransitionFeatureMembership.


## Constraints

- **validateTransitionFeatureMembershipEffectAction**
- **validateTransitionFeatureMembershipGuardExpression**
- **validateTransitionFeatureMembershipOwningType**
- **validateTransitionFeatureMembershipTriggerAction**
