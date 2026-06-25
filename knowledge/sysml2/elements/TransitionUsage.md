---
name: TransitionUsage
package: States
fully qualified name: SysML::Systems::States::TransitionUsage
isAbstract: false
visibility: public
generalizes: [ActionUsage]
specializedBy: []
---

# TransitionUsage

`States` package · concrete metaclass

A TransitionUsage is an ActionUsage representing a triggered transition between ActionUsages or StateUsages. When triggered by a triggerAction, when its guardExpression is true, the TransitionUsage asserts that its source is exited, then its effectAction (if any) is performed, and then its target is entered.A TransitionUsage can be related to some of its ownedFeatures using TransitionFeatureMembership Relationships, corresponding to the triggerAction, guardExpression and effectAction of the TransitionUsage.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### effectAction

`+` [ActionUsage](ActionUsage.md) · `[0..*]` · *derived*

The ActionUsages that define the effects of this TransitionUsage, which are the ownedFeatures of the TransitionUsage related to it by TransitionFeatureMemberships with kind = effect, which must all be ActionUsages.

Subsets [feature](Type.md#feature)

### guardExpression

`+` [Expression](Expression.md) · `[0..*]` · *derived*

The Expressions that define the guards of this TransitionUsage, which are the ownedFeatures of the TransitionUsage related to it by TransitionFeatureMemberships with kind = guard, which must all be Expressions.

Subsets [ownedFeature](Type.md#ownedfeature)

### source

`+` [ActionUsage](ActionUsage.md) · `[1..1]` · *derived*

The source ActionUsage of this TransitionUsage, which becomes the source of the succession for the TransitionUsage.

### succession

`+` [Succession](Succession.md) · `[1..1]` · *derived*

The Succession that is the ownedFeature of this TransitionUsage, which, if the TransitionUsage is triggered, asserts the temporal ordering of the source and target.

Subsets [ownedMember](Namespace.md#ownedmember)

### target

`+` [ActionUsage](ActionUsage.md) · `[1..1]` · *derived*

The target ActionUsage of this TransitionUsage, which is the targetFeature of the succession for the TransitionUsage.

### triggerAction

`+` [AcceptActionUsage](AcceptActionUsage.md) · `[0..*]` · *derived*

The AcceptActionUsages that define the triggers of this TransitionUsage, which are the ownedFeatures of the TransitionUsage related to it by TransitionFeatureMemberships with kind = trigger, which must all be AcceptActionUsages.

Subsets [ownedFeature](Type.md#ownedfeature)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| actionDefinition | [Behavior](Behavior.md) | [0..*] | [ActionUsage](ActionUsage.md) | derived, ordered |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| behavior | [Behavior](Behavior.md) | [0..*] | [Step](Step.md) | derived, ordered |
| chainingFeature | [Feature](Feature.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| crossFeature | [Feature](Feature.md) | [0..1] | [Feature](Feature.md) | derived |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| definition | [Classifier](Classifier.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| differencingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedUsage | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| direction | FeatureDirectionKind | [0..1] | [Feature](Feature.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| endFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| endOwningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| feature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | [FeatureMembership](FeatureMembership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| featureTarget | [Feature](Feature.md) | [1..1] | [Feature](Feature.md) | derived |
| featuringType | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| importedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| individualDefinition | [OccurrenceDefinition](OccurrenceDefinition.md) | [0..1] | [OccurrenceUsage](OccurrenceUsage.md) | derived |
| inheritedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | [Membership](Membership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| input | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | Boolean | [1..1] | [Type](Type.md) |  |
| isComposite | Boolean | [1..1] | [Feature](Feature.md) |  |
| isConjugated | Boolean | [1..1] | [Type](Type.md) | derived |
| isConstant | Boolean | [1..1] | [Feature](Feature.md) |  |
| isDerived | Boolean | [1..1] | [Feature](Feature.md) |  |
| isEnd | Boolean | [1..1] | [Feature](Feature.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isIndividual | Boolean | [1..1] | [OccurrenceUsage](OccurrenceUsage.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isOrdered | Boolean | [1..1] | [Feature](Feature.md) |  |
| isPortion | Boolean | [1..1] | [Feature](Feature.md) |  |
| isReference | Boolean | [1..1] | [Usage](Usage.md) | derived |
| isSufficient | Boolean | [1..1] | [Type](Type.md) |  |
| isUnique | Boolean | [1..1] | [Feature](Feature.md) |  |
| isVariable | Boolean | [1..1] | [Feature](Feature.md) |  |
| isVariation | Boolean | [1..1] | [Usage](Usage.md) |  |
| mayTimeVary | Boolean | [1..1] | [Usage](Usage.md) | derived |
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | [Multiplicity](Multiplicity.md) | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| nestedAction | [ActionUsage](ActionUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAllocation | [AllocationUsage](AllocationUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAnalysisCase | [AnalysisCaseUsage](AnalysisCaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAttribute | [AttributeUsage](AttributeUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedCalculation | [CalculationUsage](CalculationUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedCase | [CaseUsage](CaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedConcern | [ConcernUsage](ConcernUsage.md) | [0..*] | [Usage](Usage.md) | derived |
| nestedConnection | [ConnectorAsUsage](ConnectorAsUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedConstraint | [ConstraintUsage](ConstraintUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedEnumeration | [EnumerationUsage](EnumerationUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedFlow | [FlowUsage](FlowUsage.md) | [0..*] | [Usage](Usage.md) | derived |
| nestedInterface | [InterfaceUsage](InterfaceUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedItem | [ItemUsage](ItemUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedMetadata | [MetadataUsage](MetadataUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedOccurrence | [OccurrenceUsage](OccurrenceUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedPart | [PartUsage](PartUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedPort | [PortUsage](PortUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedReference | [ReferenceUsage](ReferenceUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedRendering | [RenderingUsage](RenderingUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedRequirement | [RequirementUsage](RequirementUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedState | [StateUsage](StateUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedTransition | [TransitionUsage](TransitionUsage.md) | [0..*] | [Usage](Usage.md) | derived |
| nestedUsage | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedUseCase | [UseCaseUsage](UseCaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedVerificationCase | [VerificationCaseUsage](VerificationCaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedView | [ViewUsage](ViewUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedViewpoint | [ViewpointUsage](ViewpointUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| occurrenceDefinition | [Class](Class.md) | [0..*] | [OccurrenceUsage](OccurrenceUsage.md) | derived, ordered |
| output | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedConjugator | [Conjugation](Conjugation.md) | [0..1] | [Type](Type.md) | derived, composite |
| ownedCrossSubsetting | [CrossSubsetting](CrossSubsetting.md) | [0..1] | [Feature](Feature.md) | derived, composite |
| ownedDifferencing | [Differencing](Differencing.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedDisjoining | [Disjoining](Disjoining.md) | [0..*] | [Type](Type.md) | derived, composite |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedEndFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeatureChaining | [FeatureChaining](FeatureChaining.md) | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedFeatureInverting | [FeatureInverting](FeatureInverting.md) | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedImport | [Import](Import.md) | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedIntersecting | [Intersecting](Intersecting.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedMember | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedRedefinition | [Redefinition](Redefinition.md) | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedReferenceSubsetting | [ReferenceSubsetting](ReferenceSubsetting.md) | [0..1] | [Feature](Feature.md) | derived, composite |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | [Specialization](Specialization.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubsetting | [Subsetting](Subsetting.md) | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedTypeFeaturing | [TypeFeaturing](TypeFeaturing.md) | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedTyping | [FeatureTyping](FeatureTyping.md) | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedUnioning | [Unioning](Unioning.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningDefinition | [Definition](Definition.md) | [0..1] | [Usage](Usage.md) | derived |
| owningFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..1] | [Feature](Feature.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| owningUsage | [Usage](Usage.md) | [0..1] | [Usage](Usage.md) | derived |
| parameter | [Feature](Feature.md) | [0..*] | [Step](Step.md) | derived, ordered |
| portionKind | PortionKind | [0..1] | [OccurrenceUsage](OccurrenceUsage.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| type | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| usage | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| variant | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived |
| variantMembership | [VariantMembership](VariantMembership.md) | [0..*] | [Usage](Usage.md) | derived, composite |

## Constraints

### checkTransitionUsageActionSpecialization

A composite TransitionUsage whose owningType is an ActionDefinition or ActionUsage and whose source is not a StateUsage must directly or indirectly specialize the ActionUsage Actions::Action::decisionTransitions from the Systems Model Library.

```ocl
isComposite and owningType <> null and
(owningType.oclIsKindOf(ActionDefinition) or
 owningType.oclIsKindOf(ActionUsage)) and
source <> null and not source.oclIsKindOf(StateUsage) implies
    specializesFromLibrary('Actions::Action::decisionTransitions')
```

### checkTransitionUsagePayloadSpecialization

If a TransitionUsage has a triggerAction, then the payload parameter of the TransitionUsage subsets the Feature chain of the triggerAction and its payloadParameter.

```ocl
triggerAction->notEmpty() implies
    let payloadParameter : Feature = inputParameter(2) in
    payloadParameter <> null and
    payloadParameter.subsetsChain(triggerAction->at(1), triggerPayloadParameter())
```

### checkTransitionUsageSourceBindingConnector

A TransitionUsage must have an ownedMember that is a BindingConnector between its source and its first input parameter (which redefines Actions::TransitionAction::transitionLinkSource).

```ocl
ownedMember->selectByKind(BindingConnector)->exists(b |
    b.relatedFeatures->includes(source) and
    b.relatedFeatures->includes(inputParameter(1)))
```

### checkTransitionUsageSpecialization

A TransitionUsage must directly or indirectly specialize the ActionUsage Actions::transitionActions from the Systems Model Library.

```ocl
specializesFromLibrary('Actions::transitionActions')
```

### checkTransitionUsageStateSpecialization

A composite TransitionUsage whose owningType is a StateDefinition or StateUsage and whose source is a StateUsage must directly or indirectly specialize the ActionUsage States::StateAction::stateTransitions from the Systems Model Library

```ocl
isComposite and owningType <> null and
(owningType.oclIsKindOf(StateDefinition) or
 owningType.oclIsKindOf(StateUsage)) and
source <> null and source.oclIsKindOf(StateUsage) implies
    specializesFromLibrary('States::StateAction::stateTransitions')
```

### checkTransitionUsageSuccessionBindingConnector

A TransitionUsage must have an ownedMember that is a BindingConnector between its succession and the inherited Feature TransitionPerformances::TransitionPerformance::transitionLink.

```ocl
ownedMember->selectByKind(BindingConnector)->exists(b |
    b.relatedFeatures->includes(succession) and
    b.relatedFeatures->includes(resolveGlobal(
        'TransitionPerformances::TransitionPerformance::transitionLink')))
```

### checkTransitionUsageSuccessionSourceSpecialization

The sourceFeature of the succession of a TransitionUsage must be the source of the TransitionUsage (i.e., the first connectorEnd of the succession must have a ReferenceSubsetting Relationship with the source).

```ocl
succession.sourceFeature = source
```

### checkTransitionUsageTransitionFeatureSpecialization

The triggerActions, guardExpressions, and effectActions of a TransitionUsage must specialize, respectively, the accepter, guard, and effect features of the ActionUsage Actions::TransitionActions from the Systems Model Library.

```ocl
triggerAction->forAll(specializesFromLibrary('Actions::TransitionAction::accepter') and
guardExpression->forAll(specializesFromLibrary('Actions::TransitionAction::guard') and
effectAction->forAll(specializesFromLibrary('Actions::TransitionAction::effect'))
```

### deriveTransitionUsageEffectAction

The effectActions of a TransitionUsage are the transitionFeatures of the ownedFeatureMemberships of the TransitionUsage with kind = effect, which must all be ActionUsages.

```ocl
triggerAction = ownedFeatureMembership->
    selectByKind(TransitionFeatureMembership)->
    select(kind = TransitionFeatureKind::trigger).transitionFeatures->
    selectByKind(AcceptActionUsage)
```

### deriveTransitionUsageGuardExpression

The triggerActions of a TransitionUsage are the transitionFeatures of the ownedFeatureMemberships of the TransitionUsage with kind = trigger, which must all be Expressions.

```ocl
guardExpression = ownedFeatureMembership->
    selectByKind(TransitionFeatureMembership)->
    select(kind = TransitionFeatureKind::trigger).transitionFeature->
    selectByKind(Expression)
```

### deriveTransitionUsageSource

The source of a TransitionUsage is featureTarget of the result of sourceFeature(), which must be an ActionUsage.

```ocl
source =
    let sourceFeature : Feature = sourceFeature() in
    if sourceFeature = null then null
    else sourceFeature.featureTarget.oclAsType(ActionUsage)
```

### deriveTransitionUsageSuccession

The succession of a TransitionUsage is its first ownedMember that is a Succession.

```ocl
succession = ownedMember->selectByKind(Succession)->at(1)
```

### deriveTransitionUsageTarget

The target of a TransitionUsage is given by the featureTarget of the targetFeature of its succession, which must be an ActionUsage.

```ocl
target =
    if succession.targetFeature->isEmpty() then null
    else
        let targetFeature : Feature =
            succession.targetFeature->first().featureTarget in
        if not targetFeature.oclIsKindOf(ActionUsage) then null
        else targetFeature.oclAsType(ActionUsage)
        endif
    endif
```

### deriveTransitionUsageTriggerAction

The triggerActions of a TransitionUsage are the transitionFeatures of the ownedFeatureMemberships of the TransitionUsage with kind = trigger, which must all be AcceptActionUsages.

```ocl
triggerAction = ownedFeatureMembership->
    selectByKind(TransitionFeatureMembership)->
    select(kind = TransitionFeatureKind::trigger).transitionFeature->
    selectByKind(AcceptActionUsage)
```

### validateTransitionUsageParameters

A TransitionUsage must have at least one owned input parameter and, if it has a triggerAction, it must have at least two.

```ocl
if triggerAction->isEmpty() then
    inputParameters()->size() >= 1
else
    inputParameters()->size() >= 2
endif
```

### validateTransitionUsageSuccession

A TransitionUsage must have an ownedMember that is a Succession with an ActionUsage as the featureTarget of its targetFeature.

```ocl
let successions : Sequence(Successions) = 
    ownedMember->selectByKind(Succession) in
successions->notEmpty() and
successions->at(1).targetFeature.featureTarget->
    forAll(oclIsKindOf(ActionUsage))
```

### validateTransitionUsageTriggerActions

If the source of a TransitionUsage is not a StateUsage, then the TransitionUsage must not have any triggerActions.

```ocl
source <> null and not source.oclIsKindOf(StateUsage) implies
    triggerAction->isEmpty()
```

