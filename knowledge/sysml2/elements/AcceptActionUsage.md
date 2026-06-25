---
name: AcceptActionUsage
package: Actions
fully qualified name: SysML::Systems::Actions::AcceptActionUsage
isAbstract: false
visibility: public
generalizes: [ActionUsage]
specializedBy: []
---

# AcceptActionUsage

`Actions` package · concrete metaclass

An AcceptActionUsage is an ActionUsage that specifies the acceptance of an incomingTransfer from the Occurrence given by the result of its receiverArgument Expression. (If no receiverArgument is provided, the default is the this context of the AcceptActionUsage.) The payload of the accepted Transfer is output on its payloadParameter. Which Transfers may be accepted is determined by conformance to the typing and (potentially) binding of the payloadParameter.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### payloadArgument

`+` [Expression](Expression.md) · `[0..1]` · *derived*

An Expression whose result is bound to the payload parameter of this AcceptActionUsage. If provided, the AcceptActionUsage will only accept a Transfer with exactly this payload.

### payloadParameter

`+` [ReferenceUsage](ReferenceUsage.md) · `[1..1]` · *derived*

The nestedReference of this AcceptActionUsage that redefines the payload output parameter of the base AcceptActionUsage AcceptAction from the Systems Model Library.

Subsets [parameter](Step.md#parameter)

### receiverArgument

`+` [Expression](Expression.md) · `[0..1]` · *derived*

An Expression whose result is bound to the receiver input parameter of this AcceptActionUsage.


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

### checkAcceptActionUsageReceiverBindingConnector

If the payloadArgument of an AcceptActionUsage is a TriggerInvocationExpression, then the AcceptActionusage must have an ownedFeature that is a BindingConnector between its receiver parameter and the receiver parameter of the TriggerInvocationExpression.

```ocl
payloadArgument <> null and
payloadArgument.oclIsKindOf(TriggerInvocationExpression) implies
    let invocation : Expression =
        payloadArgument.oclAsType(Expression) in
    parameter->size() >= 2 and
    invocation.parameter->size() >= 2 and        
    ownedFeature->selectByKind(BindingConnector)->exists(b |
        b.relatedFeatures->includes(parameter->at(2)) and
        b.relatedFeatures->includes(invocation.parameter->at(2)))
```

### checkAcceptActionUsageSpecialization

An AcceptActionUsage that is not the triggerAction of a TransitionUsage must directly or indirectly specialize the ActionUsage Actions::acceptActions from the Systems Model Library.

```ocl
not isTriggerAction() implies
    specializesFromLibrary('Actions::acceptActions')
```

### checkAcceptActionUsageSubactionSpecialization

A composite AcceptActionUsage that is a subaction usage, but is not the triggerAction of a TransitionUsage, must directly or indirectly specialize the ActionUsage Actions::Action::acceptSubactions from the Systems Model Library.

```ocl
isSubactionUsage() and not isTriggerAction() implies
    specializesFromLibrary('Actions::Action::acceptSubactions')
```

### checkAcceptActionUsageTriggerActionSpecialization

An AcceptActionUsage that is the triggerAction of TransitionUsage must directly or indirectly specialize the ActionUsage Actions::TransitionAction::accepter from the Systems Model Library.

```ocl
isTriggerAction() implies
    specializesFromLibrary('Actions::TransitionAction::accepter')
```

### deriveAcceptActionUsagePayloadArgument

The payloadArgument of an AcceptUsageAction is its first argument Expression.

```ocl
payloadArgument = argument(1)
```

### deriveAcceptActionUsagePayloadParameter

The payloadParameter of an AcceptActionUsage is its first parameter.

```ocl
payloadParameter = 
 if parameter->isEmpty() then null
 else parameter->first() endif
```

### deriveAcceptActionUsageReceiverArgument

The receiverArgument of an AcceptUsageAction is its second argument Expression.

```ocl
receiverArgument = argument(2)
```

### validateAcceptActionUsageParameters

An AcceptUsageAction must have at least on input parameter, corresponding to its payload (even if it has no FeatureValue). (Note that the payloadParameter is an input as well as an output.)

```ocl
inputParameters()->notEmpty()
```

