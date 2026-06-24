# TransitionUsage

`States` package · concrete metaclass

A TransitionUsage is an ActionUsage representing a triggered transition between ActionUsages or StateUsages. When triggered by a triggerAction, when its guardExpression is true, the TransitionUsage asserts that its source is exited, then its effectAction (if any) is performed, and then its target is entered.A TransitionUsage can be related to some of its ownedFeatures using TransitionFeatureMembership Relationships, corresponding to the triggerAction, guardExpression and effectAction of the TransitionUsage.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### effectAction : ActionUsage [0..*]

The ActionUsages that define the effects of this TransitionUsage, which are the ownedFeatures of the TransitionUsage related to it by TransitionFeatureMemberships with kind = effect, which must all be ActionUsages.

### guardExpression : &#171;untyped&#187; [0..*]

The Expressions that define the guards of this TransitionUsage, which are the ownedFeatures of the TransitionUsage related to it by TransitionFeatureMemberships with kind = guard, which must all be Expressions.

### source : ActionUsage [1..1]

The source ActionUsage of this TransitionUsage, which becomes the source of the succession for the TransitionUsage.

### succession : &#171;untyped&#187; [1..1]

The Succession that is the ownedFeature of this TransitionUsage, which, if the TransitionUsage is triggered, asserts the temporal ordering of the source and target.

### target : ActionUsage [1..1]

The target ActionUsage of this TransitionUsage, which is the targetFeature of the succession for the TransitionUsage.

### triggerAction : AcceptActionUsage [0..*]

The AcceptActionUsages that define the triggers of this TransitionUsage, which are the ownedFeatures of the TransitionUsage related to it by TransitionFeatureMemberships with kind = trigger, which must all be AcceptActionUsages.


## Constraints

- **checkTransitionUsageActionSpecialization**
- **checkTransitionUsagePayloadSpecialization**
- **checkTransitionUsageSourceBindingConnector**
- **checkTransitionUsageSpecialization**
- **checkTransitionUsageStateSpecialization**
- **checkTransitionUsageSuccessionBindingConnector**
- **checkTransitionUsageSuccessionSourceSpecialization**
- **checkTransitionUsageTransitionFeatureSpecialization**
- **deriveTransitionUsageEffectAction**
- **deriveTransitionUsageGuardExpression**
- **deriveTransitionUsageSource**
- **deriveTransitionUsageSuccession**
- **deriveTransitionUsageTarget**
- **deriveTransitionUsageTriggerAction**
- **validateTransitionUsageParameters**
- **validateTransitionUsageSuccession**
- **validateTransitionUsageTriggerActions**
