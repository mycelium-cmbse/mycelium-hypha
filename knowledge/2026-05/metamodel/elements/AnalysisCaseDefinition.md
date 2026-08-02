---
name: AnalysisCaseDefinition
package: AnalysisCases
fully qualified name: SysML::Systems::AnalysisCases::AnalysisCaseDefinition
isAbstract: false
visibility: public
generalizes: [CaseDefinition]
specializedBy: []
---

# AnalysisCaseDefinition

`AnalysisCases` package · concrete metaclass

An AnalysisCaseDefinition is a CaseDefinition for the case of carrying out an analysis.

## Generalizations

- [CaseDefinition](CaseDefinition.md)

## Owned features

### resultExpression

`+` [Expression](Expression.md) · `[0..1]` · *derived*

An Expression used to compute the result of the AnalysisCaseDefinition, owned via a ResultExpressionMembership.

Subsets [expression](Function.md#expression), [ownedFeature](Type.md#ownedfeature)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| action | [ActionUsage](ActionUsage.md) | [0..*] | [ActionDefinition](ActionDefinition.md) | derived, ordered |
| actorParameter | [PartUsage](PartUsage.md) | [0..*] | [CaseDefinition](CaseDefinition.md) | derived, ordered |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| calculation | [CalculationUsage](CalculationUsage.md) | [0..*] | [CalculationDefinition](CalculationDefinition.md) | derived, ordered |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| differencingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedUsage | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| endFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| expression | [Expression](Expression.md) | [0..*] | [Function](Function.md) | derived |
| feature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | [FeatureMembership](FeatureMembership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| importedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | [Membership](Membership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| input | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | [Boolean](Boolean.md) | [1..1] | [Type](Type.md) |  |
| isConjugated | [Boolean](Boolean.md) | [1..1] | [Type](Type.md) | derived |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isIndividual | [Boolean](Boolean.md) | [1..1] | [OccurrenceDefinition](OccurrenceDefinition.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| isModelLevelEvaluable | [Boolean](Boolean.md) | [1..1] | [Function](Function.md) | derived |
| isSufficient | [Boolean](Boolean.md) | [1..1] | [Type](Type.md) |  |
| isVariation | [Boolean](Boolean.md) | [1..1] | [Definition](Definition.md) |  |
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | [Multiplicity](Multiplicity.md) | [0..1] | [Type](Type.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| objectiveRequirement | [RequirementUsage](RequirementUsage.md) | [0..1] | [CaseDefinition](CaseDefinition.md) | derived, ordered |
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
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| result | [Feature](Feature.md) | [1..1] | [Function](Function.md) | derived |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| step | [Step](Step.md) | [0..*] | [Behavior](Behavior.md) | derived |
| subjectParameter | [Usage](Usage.md) | [1..1] | [CaseDefinition](CaseDefinition.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| usage | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| variant | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived |
| variantMembership | [VariantMembership](VariantMembership.md) | [0..*] | [Definition](Definition.md) | derived, composite |

## Constraints

### checkAnalysisCaseDefinitionSpecialization

An AnalysisCaseDefinition must directly or indirectly specialize the base AnalysisCaseDefinition AnalysisCases::AnalysisCase from the Systems Model Library.

```ocl
specializesFromLibrary('AnalysisCases::AnalysisCase')
```

### deriveAnalysisCaseDefinitionResultExpression

The resultExpression of a AnalysisCaseDefinition is the ownedResultExpression of its ResultExpressionMembership, if any.

```ocl
resultExpression =
    let results : OrderedSet(ResultExpressionMembership) =
        featureMembersip->
            selectByKind(ResultExpressionMembership) in
    if results->isEmpty() then null
    else results->first().ownedResultExpression
    endif
```

