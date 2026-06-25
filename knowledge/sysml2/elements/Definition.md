---
name: Definition
package: DefinitionAndUsage
isAbstract: false
generalizes: [Classifier]
specializedBy: [AttributeDefinition, OccurrenceDefinition]
---

# Definition

`DefinitionAndUsage` package · concrete metaclass

A Definition is a Classifier of Usages. The actual kinds of Definition that may appear in a model are given by the subclasses of Definition (possibly as extended with user-defined SemanticMetadata).Normally, a Definition has owned Usages that model features of the thing being defined. A Definition may also have other Definitions nested in it, but this has no semantic significance, other than the nested scoping resulting from the Definition being considered as a Namespace for any nested Definitions.However, if a Definition has isVariation = true, then it represents a variation point Definition. In this case, all of its members must be variant Usages, related to the Definition by VariantMembership Relationships. Rather than being features of the Definition, variant Usages model different concrete alternatives that can be chosen to fill in for an abstract Usage of the variation point Definition.

## Generalizations

- [Classifier](Classifier.md)

## Specializations

- [AttributeDefinition](AttributeDefinition.md)
- [OccurrenceDefinition](OccurrenceDefinition.md)

## Owned features

### directedUsage : Usage [0..*] {derived, ordered}

The usages of this Definition that are directedFeatures.

Subsets: `directedFeature`

### isVariation : Boolean [1..1]

Whether this Definition is for a variation point or not. If true, then all the memberships of the Definition must be VariantMemberships.

### ownedAction : ActionUsage [0..*] {derived, ordered}

The ActionUsages that are ownedUsages of this Definition.

Subsets: `ownedOccurrence`

### ownedAllocation : AllocationUsage [0..*] {derived, ordered}

The AllocationUsages that are ownedUsages of this Definition.

Subsets: `ownedConnection`

### ownedAnalysisCase : AnalysisCaseUsage [0..*] {derived, ordered}

The AnalysisCaseUsages that are ownedUsages of this Definition.

Subsets: `ownedCase`

### ownedAttribute : AttributeUsage [0..*] {derived, ordered}

The AttributeUsages that are ownedUsages of this Definition.

Subsets: `ownedUsage`

### ownedCalculation : CalculationUsage [0..*] {derived, ordered}

The CalculationUsages that are ownedUsages of this Definition.

Subsets: `ownedAction`

### ownedCase : CaseUsage [0..*] {derived, ordered}

The code>CaseUsages that are ownedUsages of this Definition.

Subsets: `ownedCalculation`

### ownedConcern : ConcernUsage [0..*] {derived}

The ConcernUsages that are ownedUsages of this Definition.

Subsets: `ownedRequirement`

### ownedConnection : ConnectorAsUsage [0..*] {derived, ordered}

The ConnectorAsUsages that are ownedUsages of this Definition. Note that this list includes BindingConnectorAsUsages, SuccessionAsUsages, and FlowUsages because these are ConnectorAsUsages even though they are not ConnectionUsages.

Subsets: `ownedUsage`

### ownedConstraint : ConstraintUsage [0..*] {derived, ordered}

The ConstraintUsages that are ownedUsages of this Definition.

Subsets: `ownedOccurrence`

### ownedEnumeration : EnumerationUsage [0..*] {derived, ordered}

The EnumerationUsages that are ownedUsages of this Definition.

Subsets: `ownedAttribute`

### ownedFlow : FlowUsage [0..*] {derived}

The FlowUsages that are ownedUsages of this Definition.

Subsets: `ownedConnection`

### ownedInterface : InterfaceUsage [0..*] {derived, ordered}

The InterfaceUsages that are ownedUsages of this Definition.

Subsets: `ownedConnection`

### ownedItem : ItemUsage [0..*] {derived, ordered}

The ItemUsages that are ownedUsages of this Definition.

Subsets: `ownedOccurrence`

### ownedMetadata : MetadataUsage [0..*] {derived, ordered}

The MetadataUsages that are ownedMembers of this Definition.

Subsets: `ownedItem`

### ownedOccurrence : OccurrenceUsage [0..*] {derived, ordered}

The OccurrenceUsages that are ownedUsages of this Definition.

Subsets: `ownedUsage`

### ownedPart : PartUsage [0..*] {derived, ordered}

The PartUsages that are ownedUsages of this Definition.

Subsets: `ownedItem`

### ownedPort : PortUsage [0..*] {derived, ordered}

The PortUsages that are ownedUsages of this Definition.

Subsets: `ownedUsage`

### ownedReference : ReferenceUsage [0..*] {derived, ordered}

The ReferenceUsages that are ownedUsages of this Definition.

Subsets: `ownedUsage`

### ownedRendering : RenderingUsage [0..*] {derived, ordered}

The RenderingUsages that are ownedUsages of this Definition.

Subsets: `ownedPart`

### ownedRequirement : RequirementUsage [0..*] {derived, ordered}

The RequirementUsages that are ownedUsages of this Definition.

Subsets: `ownedConstraint`

### ownedState : StateUsage [0..*] {derived, ordered}

The StateUsages that are ownedUsages of this Definition.

Subsets: `ownedAction`

### ownedTransition : TransitionUsage [0..*] {derived}

The TransitionUsages that are ownedUsages of this Definition.

Subsets: `ownedUsage`

### ownedUsage : Usage [0..*] {derived, ordered}

The Usages that are ownedFeatures of this Definition.

Subsets: `ownedFeature`

### ownedUseCase : UseCaseUsage [0..*] {derived, ordered}

The UseCaseUsages that are ownedUsages of this Definition.

Subsets: `ownedCase`

### ownedVerificationCase : VerificationCaseUsage [0..*] {derived, ordered}

The VerificationCaseUsages that are ownedUsages of this Definition.

Subsets: `ownedCase`

### ownedView : ViewUsage [0..*] {derived, ordered}

The ViewUsages that are ownedUsages of this Definition.

Subsets: `ownedPart`

### ownedViewpoint : ViewpointUsage [0..*] {derived, ordered}

The ViewpointUsages that are ownedUsages of this Definition.

Subsets: `ownedRequirement`

### usage : Usage [0..*] {derived, ordered}

The Usages that are features of this Definition (not necessarily owned).

Subsets: `feature`

### variant : Usage [0..*] {derived}

The Usages which represent the variants of this Definition as a variation point Definition, if isVariation = true. If isVariation = false, the there must be no variants.

Subsets: `ownedMember`

### variantMembership : VariantMembership [0..*] {derived, composite}

The ownedMemberships of this Definition that are VariantMemberships. If isVariation = true, then this must be all ownedMemberships of the Definition. If isVariation = false, then variantMembershipmust be empty.

Subsets: `ownedMembership`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| differencingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| endFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| feature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, ordered |
| importedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | Membership | [0..*] | [Type](Type.md) | derived, ordered |
| input | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | Boolean | [1..1] | [Type](Type.md) |  |
| isConjugated | Boolean | [1..1] | [Type](Type.md) | derived |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isSufficient | Boolean | [1..1] | [Type](Type.md) |  |
| member | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | Multiplicity | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| output | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedConjugator | Conjugation | [0..1] | [Type](Type.md) | derived, composite |
| ownedDifferencing | Differencing | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedDisjoining | Disjoining | [0..*] | [Type](Type.md) | derived, composite |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedEndFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeatureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedImport | Import | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedIntersecting | Intersecting | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedMember | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | Specialization | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubclassification | Subclassification | [0..*] | [Classifier](Classifier.md) | derived, composite |
| ownedUnioning | Unioning | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### deriveDefinitionDirectedUsage

The directedUsages of a Definition are all its directedFeatures that are Usages.

```ocl
directedUsage = directedFeature->selectByKind(Usage)
```

### deriveDefinitionOwnedAction

The ownedActions of a Definition are all its ownedUsages that are ActionUsages.

```ocl
ownedAction = ownedUsage->selectByKind(ActionUsage)
```

### deriveDefinitionOwnedAllocation

The ownedAllocations of a Definition are all its ownedUsages that are AllocationUsages.

```ocl
ownedAllocation = ownedUsage->selectByKind(AllocationUsage)
```

### deriveDefinitionOwnedAnalysisCase

The ownedAnalysisCases of a Definition are all its ownedUsages that are AnalysisCaseUsages.

```ocl
ownedAnalysisCase = ownedUsage->selectByKind(AnalysisCaseUsage)
```

### deriveDefinitionOwnedAttribute

The ownedAttributes of a Definition are all its ownedUsages that are AttributeUsages.

```ocl
ownedAttribute = ownedUsage->selectByKind(AttributeUsage)
```

### deriveDefinitionOwnedCalculation

The ownedCalculations of a Definition are all its ownedUsages that are CalculationUsages.

```ocl
ownedCalculation = ownedUsage->selectByKind(CalculationUsage)
```

### deriveDefinitionOwnedCase

The ownedCases of a Definition are all its ownedUsages that are CaseUsages.

```ocl
ownedCase = ownedUsage->selectByKind(CaseUsage)
```

### deriveDefinitionOwnedConcern

The ownedConcerns of a Definition are all its ownedUsages that are ConcernUsages.

```ocl
ownedConcern = ownedUsage->selectByKind(ConcernUsage)
```

### deriveDefinitionOwnedConnection

The ownedConnections of a Definition are all its ownedUsages that are ConnectorAsUsages.

```ocl
ownedConnection = ownedUsage->selectByKind(ConnectorAsUsage)
```

### deriveDefinitionOwnedConstraint

The ownedConstraints of a Definition are all its ownedUsages that are ConstraintUsages.

```ocl
ownedConstraint = ownedUsage->selectByKind(ConstraintUsage)
```

### deriveDefinitionOwnedEnumeration

The ownedEnumerations of a Definition are all its ownedUsages that are EnumerationUsages.

```ocl
ownedEnumeration = ownedUsage->selectByKind(EnumerationUsage)
```

### deriveDefinitionOwnedFlow

The ownedFlows of a Definition are all its ownedUsages that are FlowUsages.

```ocl
ownedFlow = ownedUsage->selectByKind(FlowUsage)
```

### deriveDefinitionOwnedInterface

The ownedInterfaces of a Definition are all its ownedUsages that are InterfaceUsages.

```ocl
ownedInterface = ownedUsage->selectByKind(ReferenceUsage)
```

### deriveDefinitionOwnedItem

The ownedItems of a Definition are all its ownedUsages that are ItemUsages.

```ocl
ownedItem = ownedUsage->selectByKind(ItemUsage)
```

### deriveDefinitionOwnedMetadata

The ownedMetadata of a Definition are all its ownedMembers that are MetadataUsages.

```ocl
ownedMetadata = ownedMember->selectByKind(MetadataUsage)
```

### deriveDefinitionOwnedOccurrence

The ownedOccurrences of a Definition are all its ownedUsages that are OccurrenceUsages.

```ocl
ownedOccurrence = ownedUsage->selectByKind(OccurrenceUsage)
```

### deriveDefinitionOwnedPart

The ownedParts of a Definition are all its ownedUsages that are PartUsages.

```ocl
ownedPart = ownedUsage->selectByKind(PartUsage)
```

### deriveDefinitionOwnedPort

The ownedPorts of a Definition are all its ownedUsages that are PortUsages.

```ocl
ownedPort = ownedUsage->selectByKind(PortUsage)
```

### deriveDefinitionOwnedReference

The ownedReferences of a Definition are all its ownedUsages that are ReferenceUsages.

```ocl
ownedReference = ownedUsage->selectByKind(ReferenceUsage)
```

### deriveDefinitionOwnedRendering

The ownedRenderings of a Definition are all its ownedUsages that are RenderingUsages.

```ocl
ownedRendering = ownedUsage->selectByKind(RenderingUsage)
```

### deriveDefinitionOwnedRequirement

The ownedRequirements of a Definition are all its ownedUsages that are RequirementUsages.

```ocl
ownedRequirement = ownedUsage->selectByKind(RequirementUsage)
```

### deriveDefinitionOwnedState

The ownedStates of a Definition are all its ownedUsages that are StateUsages.

```ocl
ownedState = ownedUsage->selectByKind(StateUsage)
```

### deriveDefinitionOwnedTransition

The ownedTransitions of a Definition are all its ownedUsages that are TransitionUsages.

```ocl
ownedTransition = ownedUsage->selectByKind(TransitionUsage)
```

### deriveDefinitionOwnedUsage

The ownedUsages of a Definition are all its ownedFeatures that are Usages.

```ocl
ownedUsage = ownedFeature->selectByKind(Usage)
```

### deriveDefinitionOwnedUseCase

The ownedUseCases of a Definition are all its ownedUsages that are UseCaseUsages.

```ocl
ownedUseCase = ownedUsage->selectByKind(UseCaseUsage)
```

### deriveDefinitionOwnedVerificationCase

The ownedValidationCases of a Definition are all its ownedUsages that are ValidationCaseUsages.

```ocl
ownedVerificationCase = ownedUsage->selectByKind(VerificationCaseUsage)
```

### deriveDefinitionOwnedView

The ownedViews of a Definition are all its ownedUsages that are ViewUsages.

```ocl
ownedView = ownedUsage->selectByKind(ViewUsage)
```

### deriveDefinitionOwnedViewpoint

The ownedViewpoints of a Definition are all its ownedUsages that are ViewpointUsages.

```ocl
ownedViewpoint = ownedUsage->selectByKind(ViewpointUsage)
```

### deriveDefinitionUsage

The usages of a Definition are all its features that are Usages.

```ocl
usage = feature->selectByKind(Usage)
```

### deriveDefinitionVariant

The variants of a Definition are the ownedVariantUsages of its variantMemberships.

```ocl
variant = variantMembership.ownedVariantUsage
```

### deriveDefinitionVariantMembership

The variantMemberships of a Definition are those ownedMemberships that are VariantMemberships.

```ocl
variantMembership = ownedMembership->selectByKind(VariantMembership)
```

### validateDefinitionVariationIsAbstract

If a Definition is a variation, then it must be abstract.

```ocl
isVariation implies isAbstract
```

### validateDefinitionVariationOwnedFeatureMembership

If a Definition is a variation, then all it must not have any ownedFeatureMemberships.

```ocl
isVariation implies ownedFeatureMembership->isEmpty()
```

### validateDefinitionVariationSpecialization

A variation Definition may not specialize any other variation Definition.

```ocl
isVariation implies
    not ownedSpecialization.specific->exists(
        oclIsKindOf(Definition) and
        oclAsType(Definition).isVariation)
```

