---
name: ConjugatedPortDefinition
package: Ports
fully qualified name: SysML::Systems::Ports::ConjugatedPortDefinition
isAbstract: false
visibility: public
generalizes: [PortDefinition]
specializedBy: []
---

# ConjugatedPortDefinition

`Ports` package · concrete metaclass

A ConjugatedPortDefinition is a PortDefinition that is a PortDefinition of its original PortDefinition. That is, a ConjugatedPortDefinition inherits all the features of the original PortDefinition, but input flows of the original PortDefinition become outputs on the ConjugatedPortDefinition and output flows of the original PortDefinition become inputs on the ConjugatedPortDefinition. Every PortDefinition (that is not itself a ConjugatedPortDefinition) has exactly one corresponding ConjugatedPortDefinition, whose effective name is the name of the originalPortDefinition, with the character ~ prepended.

## Generalizations

- [PortDefinition](PortDefinition.md)

## Owned features

### originalPortDefinition

`+` [PortDefinition](PortDefinition.md) · `[1..1]` · *derived*

The original PortDefinition for this ConjugatedPortDefinition, which is the owningNamespace of the ConjugatedPortDefinition.

Redefines [owningNamespace](Element.md#owningnamespace)

### ownedPortConjugator

`+` [PortConjugation](PortConjugation.md) · `[1..1]` · *derived*

The PortConjugation that is the ownedConjugator of this ConjugatedPortDefinition, linking it to its originalPortDefinition.

Redefines [ownedConjugator](Type.md#ownedconjugator)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| conjugatedPortDefinition | [ConjugatedPortDefinition](ConjugatedPortDefinition.md) | [0..1] | [PortDefinition](PortDefinition.md) | derived |
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
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| usage | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| variant | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived |
| variantMembership | [VariantMembership](VariantMembership.md) | [0..*] | [Definition](Definition.md) | derived, composite |

## Constraints

### validateConjugatedPortDefinitionConjugatedPortDefinitionIsEmpty

A ConjugatedPortDefinition must not itself have a conjugatedPortDefinition.

```ocl
conjugatedPortDefinition = null
```

### validateConjugatedPortDefinitionOriginalPortDefinition

The originalPortDefinition of the ownedPortConjugator of a ConjugatedPortDefinition must be the originalPortDefinition of the ConjugatedPortDefinition.

```ocl
ownedPortConjugator.originalPortDefinition = originalPortDefinition
```

