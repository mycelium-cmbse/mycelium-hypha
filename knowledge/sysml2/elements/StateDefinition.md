---
name: StateDefinition
package: States
fully qualified name: SysML::Systems::States::StateDefinition
isAbstract: false
visibility: public
generalizes: [ActionDefinition]
specializedBy: []
---

# StateDefinition

`States` package · concrete metaclass

A StateDefinition is the Definition of the Behavior of a system or part of a system in a certain state condition.A StateDefinition may be related to up to three of its ownedFeatures by StateBehaviorMembership Relationships, all of different kinds, corresponding to the entry, do and exit actions of the StateDefinition.

## Generalizations

- [ActionDefinition](ActionDefinition.md)

## Owned features

### doAction

`+` [ActionUsage](ActionUsage.md) · `[0..1]` · *derived*

The ActionUsage of this StateDefinition to be performed while in the state defined by the StateDefinition. It is the owned ActionUsage related to the StateDefinition by a StateSubactionMembership with kind = do.

### entryAction

`+` [ActionUsage](ActionUsage.md) · `[0..1]` · *derived*

The ActionUsage of this StateDefinition to be performed on entry to the state defined by the StateDefinition. It is the owned ActionUsage related to the StateDefinition by a StateSubactionMembership with kind = entry.

### exitAction

`+` [ActionUsage](ActionUsage.md) · `[0..1]` · *derived*

The ActionUsage of this StateDefinition to be performed on exit to the state defined by the StateDefinition. It is the owned ActionUsage related to the StateDefinition by a StateSubactionMembership with kind = exit.

### isParallel

`+` Boolean · `[1..1]`

Whether the ownedStates of this StateDefinition are to all be performed in parallel. If true, none of the ownedActions (which includes ownedStates) may have any incoming or outgoing Transitions. If false, only one ownedState may be performed at a time.

### state

`+` [StateUsage](StateUsage.md) · `[0..*]` · *derived, ordered*

The StateUsages, which are actions in the StateDefinition, that specify the discrete states in the behavior defined by the StateDefinition.

Subsets [action](ActionDefinition.md#action)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| action | [ActionUsage](ActionUsage.md) | [0..*] | [ActionDefinition](ActionDefinition.md) | derived, ordered |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| differencingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedUsage | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| endFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| feature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | [FeatureMembership](FeatureMembership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| importedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | [Membership](Membership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| input | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | Boolean | [1..1] | [Type](Type.md) |  |
| isConjugated | Boolean | [1..1] | [Type](Type.md) | derived |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isIndividual | Boolean | [1..1] | [OccurrenceDefinition](OccurrenceDefinition.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isSufficient | Boolean | [1..1] | [Type](Type.md) |  |
| isVariation | Boolean | [1..1] | [Definition](Definition.md) |  |
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | [Multiplicity](Multiplicity.md) | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| output | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedAction | [ActionUsage](ActionUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedAllocation | [AllocationUsage](AllocationUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedAnalysisCase | [AnalysisCaseUsage](AnalysisCaseUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedAttribute | [AttributeUsage](AttributeUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedCalculation | [CalculationUsage](CalculationUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedCase | [CaseUsage](CaseUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedConcern | [ConcernUsage](ConcernUsage.md) | [0..*] | [Definition](Definition.md) | derived |
| ownedConjugator | [Conjugation](Conjugation.md) | [0..1] | [Type](Type.md) | derived, composite |
| ownedConnection | [ConnectorAsUsage](ConnectorAsUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedConstraint | [ConstraintUsage](ConstraintUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedDifferencing | [Differencing](Differencing.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedDisjoining | [Disjoining](Disjoining.md) | [0..*] | [Type](Type.md) | derived, composite |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedEndFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedEnumeration | [EnumerationUsage](EnumerationUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedFlow | [FlowUsage](FlowUsage.md) | [0..*] | [Definition](Definition.md) | derived |
| ownedImport | [Import](Import.md) | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedInterface | [InterfaceUsage](InterfaceUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedIntersecting | [Intersecting](Intersecting.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedItem | [ItemUsage](ItemUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedMember | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedMetadata | [MetadataUsage](MetadataUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedOccurrence | [OccurrenceUsage](OccurrenceUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedPart | [PartUsage](PartUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedPort | [PortUsage](PortUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedReference | [ReferenceUsage](ReferenceUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| ownedRendering | [RenderingUsage](RenderingUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedRequirement | [RequirementUsage](RequirementUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedSpecialization | [Specialization](Specialization.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedState | [StateUsage](StateUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedSubclassification | [Subclassification](Subclassification.md) | [0..*] | [Classifier](Classifier.md) | derived, composite |
| ownedTransition | [TransitionUsage](TransitionUsage.md) | [0..*] | [Definition](Definition.md) | derived |
| ownedUnioning | [Unioning](Unioning.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedUsage | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedUseCase | [UseCaseUsage](UseCaseUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedVerificationCase | [VerificationCaseUsage](VerificationCaseUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedView | [ViewUsage](ViewUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedViewpoint | [ViewpointUsage](ViewpointUsage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| parameter | [Feature](Feature.md) | [0..*] | [Behavior](Behavior.md) | derived, ordered |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| step | [Step](Step.md) | [0..*] | [Behavior](Behavior.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| usage | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| variant | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived |
| variantMembership | [VariantMembership](VariantMembership.md) | [0..*] | [Definition](Definition.md) | derived, composite |

## Constraints

### checkStateDefinitionSpecialization

A StateDefinition must directly or indirectly specialize the StateDefinition States::StateAction from the Systems Model Library.

```ocl
specializesFromLibrary('States::StateAction')
```

### deriveStateDefinitionDoAction

The doAction of a StateDefinition is the action of the owned StateSubactionMembership with kind = do.

```ocl
doAction =
    let doMemberships : Sequence(StateSubactionMembership) =
        ownedMembership->
            selectByKind(StateSubactionMembership)->
            select(kind = StateSubactionKind::do) in
    if doMemberships->isEmpty() then null
    else doMemberships->at(1)
    endif
```

### deriveStateDefinitionEntryAction

The entryAction of a StateDefinition is the action of the owned StateSubactionMembership with kind = entry.

```ocl
entryAction =
    let entryMemberships : Sequence(StateSubactionMembership) =
        ownedMembership->
            selectByKind(StateSubactionMembership)->
            select(kind = StateSubactionKind::entry) in
    if entryMemberships->isEmpty() then null
    else entryMemberships->at(1)
    endif
```

### deriveStateDefinitionExitAction

The exitAction of a StateDefinition is the action of the owned StateSubactionMembership with kind = exit.

```ocl
exitAction = 
    let exitMemberships : Sequence(StateSubactionMembership) =
        ownedMembership->
            selectByKind(StateSubactionMembership)->
            select(kind = StateSubactionKind::exit) in
    if exitMemberships->isEmpty() then null
    else exitMemberships->at(1)
    endif
```

### deriveStateDefinitionState

The states of a StateDefinition are those of its actions that are StateUsages.

```ocl
state = action->selectByKind(StateUsage)
```

### validateStateDefinitionParallelSubactions

If a StateDefinition is parallel, then its ownedActions (which includes its ownedStates) must not have any incomingTransitions or outgoingTransitions.

```ocl
isParallel implies
    ownedAction.incomingTransition->isEmpty() and
    ownedAction.outgoingTransition->isEmpty()
```

### validateStateDefinitionStateSubactionKind

A StateDefinition must not have more than one owned StateSubactionMembership of each kind.

```ocl
ownedMembership->
    selectByKind(StateSubactionMembership)->
    isUnique(kind)
```

