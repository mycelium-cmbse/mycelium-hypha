---
name: ForLoopActionUsage
package: Actions
isAbstract: false
generalizes: [LoopActionUsage]
specializedBy: []
---

# ForLoopActionUsage

`Actions` package · concrete metaclass

A ForLoopActionUsage is a LoopActionUsage that specifies that its bodyAction ActionUsage should be performed once for each value, in order, from the sequence of values obtained as the result of the seqArgument Expression, with the loopVariable set to the value for each iteration.

## Generalizations

- [LoopActionUsage](LoopActionUsage.md)

## Owned features

### loopVariable : ReferenceUsage [1..1] {derived}

The ownedFeature of this <co>ForLoopActionUsage that acts as the loop variable, which is assigned the successive values of the input sequence on each iteration. It is the ownedFeature that redefines ForLoopAction::var.</co>

### seqArgument : Expression [1..1] {derived}

The Expression whose result provides the sequence of values to which the loopVariable is set for each iterative performance of the bodyAction. It is the Expression whose result is bound to the seq input parameter of this ForLoopActionUsage.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| actionDefinition | Behavior | [0..*] | [ActionUsage](ActionUsage.md) | derived, ordered |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| behavior | Behavior | [0..*] | [Step](Step.md) | derived, ordered |
| bodyAction | ActionUsage | [1..1] | [LoopActionUsage](LoopActionUsage.md) | derived |
| chainingFeature | Feature | [0..*] | [Feature](Feature.md) | derived, ordered |
| crossFeature | Feature | [0..1] | [Feature](Feature.md) | derived |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| definition | Classifier | [0..*] | [Usage](Usage.md) | derived, ordered |
| differencingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| directedUsage | Usage | [0..*] | [Usage](Usage.md) | derived, ordered |
| direction | FeatureDirectionKind | [0..1] | [Feature](Feature.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| endFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| endOwningType | Type | [0..1] | [Feature](Feature.md) | derived |
| feature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, ordered |
| featureTarget | Feature | [1..1] | [Feature](Feature.md) | derived |
| featuringType | Type | [0..*] | [Feature](Feature.md) | derived, ordered |
| importedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| individualDefinition | OccurrenceDefinition | [0..1] | [OccurrenceUsage](OccurrenceUsage.md) | derived |
| inheritedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | Membership | [0..*] | [Type](Type.md) | derived, ordered |
| input | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
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
| member | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | Multiplicity | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| nestedAction | ActionUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAllocation | AllocationUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAnalysisCase | AnalysisCaseUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAttribute | AttributeUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedCalculation | CalculationUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedCase | CaseUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedConcern | ConcernUsage | [0..*] | [Usage](Usage.md) | derived |
| nestedConnection | ConnectorAsUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedConstraint | ConstraintUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedEnumeration | EnumerationUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedFlow | FlowUsage | [0..*] | [Usage](Usage.md) | derived |
| nestedInterface | InterfaceUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedItem | ItemUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedMetadata | MetadataUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedOccurrence | OccurrenceUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedPart | PartUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedPort | PortUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedReference | ReferenceUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedRendering | RenderingUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedRequirement | RequirementUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedState | StateUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedTransition | TransitionUsage | [0..*] | [Usage](Usage.md) | derived |
| nestedUsage | Usage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedUseCase | UseCaseUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedVerificationCase | VerificationCaseUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedView | ViewUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedViewpoint | ViewpointUsage | [0..*] | [Usage](Usage.md) | derived, ordered |
| occurrenceDefinition | Class | [0..*] | [OccurrenceUsage](OccurrenceUsage.md) | derived, ordered |
| output | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedConjugator | Conjugation | [0..1] | [Type](Type.md) | derived, composite |
| ownedCrossSubsetting | CrossSubsetting | [0..1] | [Feature](Feature.md) | derived, composite |
| ownedDifferencing | Differencing | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedDisjoining | Disjoining | [0..*] | [Type](Type.md) | derived, composite |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedEndFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeatureChaining | FeatureChaining | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedFeatureInverting | FeatureInverting | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedFeatureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedImport | Import | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedIntersecting | Intersecting | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedMember | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedRedefinition | Redefinition | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedReferenceSubsetting | ReferenceSubsetting | [0..1] | [Feature](Feature.md) | derived, composite |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | Specialization | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubsetting | Subsetting | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedTypeFeaturing | TypeFeaturing | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedTyping | FeatureTyping | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedUnioning | Unioning | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningDefinition | Definition | [0..1] | [Usage](Usage.md) | derived |
| owningFeatureMembership | FeatureMembership | [0..1] | [Feature](Feature.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| owningType | Type | [0..1] | [Feature](Feature.md) | derived |
| owningUsage | Usage | [0..1] | [Usage](Usage.md) | derived |
| parameter | Feature | [0..*] | [Step](Step.md) | derived, ordered |
| portionKind | PortionKind | [0..1] | [OccurrenceUsage](OccurrenceUsage.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| type | Type | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| usage | Usage | [0..*] | [Usage](Usage.md) | derived, ordered |
| variant | Usage | [0..*] | [Usage](Usage.md) | derived |
| variantMembership | VariantMembership | [0..*] | [Usage](Usage.md) | derived, composite |

## Constraints

### checkForLoopActionUsageSpecialization

A ForLoopActionUsage must directly or indirectly specialize the ActionUsage Actions::forLoopActions from the Systems Model Library.

```ocl
specializesFromLibrary('Actions::forLoopActions')
```

### checkForLoopActionUsageSubactionSpecialization

A composite ForLoopActionUsage that is a subaction usage must directly or indirectly specialize the ActionUsage Actions::Action::forLoops from the Systems Model Library.

```ocl
isSubactionUsage() implies
    specializesFromLibrary('Actions::Action::forLoops')
```

### checkForLoopActionUsageVarRedefinition

The loopVariable of a ForLoopActionUsage must redefine the ActionUsage Actions::ForLoopAction::var.

```ocl
loopVariable <> null and
loopVariable.redefinesFromLibrary('Actions::ForLoopAction::var')
```

### deriveForLoopActionUsageLoopVariable

The loopVariable of a ForLoopActionUsage is its first ownedFeature, which must be a ReferenceUsage.

```ocl
loopVariable =
    if ownedFeature->isEmpty() or 
        not ownedFeature->first().oclIsKindOf(ReferenceUsage) then 
        null
    else 
        ownedFeature->first().oclAsType(ReferenceUsage)
    endif
```

### deriveForLoopActionUsageSeqArgument

The seqArgument of a ForLoopActionUsage is its first argument Expression.

```ocl
seqArgument = argument(1)
```

### validateForLoopActionUsageLoopVariable

The first ownedFeature of a ForLoopActionUsage must be a ReferenceUsage.

```ocl
ownedFeature->notEmpty() and
ownedFeature->at(1).oclIsKindOf(ReferenceUsage)
```

### validateForLoopActionUsageParameters

A ForLoopActionUsage must have two owned input parameters.

```ocl
inputParameters()->size() = 2
```

