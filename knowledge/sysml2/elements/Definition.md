# Definition

`DefinitionAndUsage` package · concrete metaclass

A Definition is a Classifier of Usages. The actual kinds of Definition that may appear in a model are given by the subclasses of Definition (possibly as extended with user-defined SemanticMetadata).Normally, a Definition has owned Usages that model features of the thing being defined. A Definition may also have other Definitions nested in it, but this has no semantic significance, other than the nested scoping resulting from the Definition being considered as a Namespace for any nested Definitions.However, if a Definition has isVariation = true, then it represents a variation point Definition. In this case, all of its members must be variant Usages, related to the Definition by VariantMembership Relationships. Rather than being features of the Definition, variant Usages model different concrete alternatives that can be chosen to fill in for an abstract Usage of the variation point Definition.

## Owned features

### directedUsage : Usage [0..*]

The usages of this Definition that are directedFeatures.

### isVariation : Boolean [1..1]

Whether this Definition is for a variation point or not. If true, then all the memberships of the Definition must be VariantMemberships.

### ownedAction : ActionUsage [0..*]

The ActionUsages that are ownedUsages of this Definition.

Subsets: `ownedOccurrence`

### ownedAllocation : AllocationUsage [0..*]

The AllocationUsages that are ownedUsages of this Definition.

Subsets: `ownedConnection`

### ownedAnalysisCase : AnalysisCaseUsage [0..*]

The AnalysisCaseUsages that are ownedUsages of this Definition.

Subsets: `ownedCase`

### ownedAttribute : AttributeUsage [0..*]

The AttributeUsages that are ownedUsages of this Definition.

Subsets: `ownedUsage`

### ownedCalculation : CalculationUsage [0..*]

The CalculationUsages that are ownedUsages of this Definition.

Subsets: `ownedAction`

### ownedCase : CaseUsage [0..*]

The code>CaseUsages that are ownedUsages of this Definition.

Subsets: `ownedCalculation`

### ownedConcern : ConcernUsage [0..*]

The ConcernUsages that are ownedUsages of this Definition.

Subsets: `ownedRequirement`

### ownedConnection : ConnectorAsUsage [0..*]

The ConnectorAsUsages that are ownedUsages of this Definition. Note that this list includes BindingConnectorAsUsages, SuccessionAsUsages, and FlowUsages because these are ConnectorAsUsages even though they are not ConnectionUsages.

Subsets: `ownedUsage`

### ownedConstraint : ConstraintUsage [0..*]

The ConstraintUsages that are ownedUsages of this Definition.

Subsets: `ownedOccurrence`

### ownedEnumeration : EnumerationUsage [0..*]

The EnumerationUsages that are ownedUsages of this Definition.

Subsets: `ownedAttribute`

### ownedFlow : FlowUsage [0..*]

The FlowUsages that are ownedUsages of this Definition.

Subsets: `ownedConnection`

### ownedInterface : InterfaceUsage [0..*]

The InterfaceUsages that are ownedUsages of this Definition.

Subsets: `ownedConnection`

### ownedItem : ItemUsage [0..*]

The ItemUsages that are ownedUsages of this Definition.

Subsets: `ownedOccurrence`

### ownedMetadata : MetadataUsage [0..*]

The MetadataUsages that are ownedMembers of this Definition.

Subsets: `ownedItem`

### ownedOccurrence : OccurrenceUsage [0..*]

The OccurrenceUsages that are ownedUsages of this Definition.

Subsets: `ownedUsage`

### ownedPart : PartUsage [0..*]

The PartUsages that are ownedUsages of this Definition.

Subsets: `ownedItem`

### ownedPort : PortUsage [0..*]

The PortUsages that are ownedUsages of this Definition.

Subsets: `ownedUsage`

### ownedReference : ReferenceUsage [0..*]

The ReferenceUsages that are ownedUsages of this Definition.

Subsets: `ownedUsage`

### ownedRendering : RenderingUsage [0..*]

The RenderingUsages that are ownedUsages of this Definition.

Subsets: `ownedPart`

### ownedRequirement : RequirementUsage [0..*]

The RequirementUsages that are ownedUsages of this Definition.

Subsets: `ownedConstraint`

### ownedState : StateUsage [0..*]

The StateUsages that are ownedUsages of this Definition.

Subsets: `ownedAction`

### ownedTransition : TransitionUsage [0..*]

The TransitionUsages that are ownedUsages of this Definition.

Subsets: `ownedUsage`

### ownedUsage : Usage [0..*]

The Usages that are ownedFeatures of this Definition.

### ownedUseCase : UseCaseUsage [0..*]

The UseCaseUsages that are ownedUsages of this Definition.

Subsets: `ownedCase`

### ownedVerificationCase : VerificationCaseUsage [0..*]

The VerificationCaseUsages that are ownedUsages of this Definition.

Subsets: `ownedCase`

### ownedView : ViewUsage [0..*]

The ViewUsages that are ownedUsages of this Definition.

Subsets: `ownedPart`

### ownedViewpoint : ViewpointUsage [0..*]

The ViewpointUsages that are ownedUsages of this Definition.

Subsets: `ownedRequirement`

### usage : Usage [0..*]

The Usages that are features of this Definition (not necessarily owned).

### variant : Usage [0..*]

The Usages which represent the variants of this Definition as a variation point Definition, if isVariation = true. If isVariation = false, the there must be no variants.

### variantMembership : VariantMembership [0..*]

The ownedMemberships of this Definition that are VariantMemberships. If isVariation = true, then this must be all ownedMemberships of the Definition. If isVariation = false, then variantMembershipmust be empty.


## Constraints

- **deriveDefinitionDirectedUsage**
- **deriveDefinitionOwnedAction**
- **deriveDefinitionOwnedAllocation**
- **deriveDefinitionOwnedAnalysisCase**
- **deriveDefinitionOwnedAttribute**
- **deriveDefinitionOwnedCalculation**
- **deriveDefinitionOwnedCase**
- **deriveDefinitionOwnedConcern**
- **deriveDefinitionOwnedConnection**
- **deriveDefinitionOwnedConstraint**
- **deriveDefinitionOwnedEnumeration**
- **deriveDefinitionOwnedFlow**
- **deriveDefinitionOwnedInterface**
- **deriveDefinitionOwnedItem**
- **deriveDefinitionOwnedMetadata**
- **deriveDefinitionOwnedOccurrence**
- **deriveDefinitionOwnedPart**
- **deriveDefinitionOwnedPort**
- **deriveDefinitionOwnedReference**
- **deriveDefinitionOwnedRendering**
- **deriveDefinitionOwnedRequirement**
- **deriveDefinitionOwnedState**
- **deriveDefinitionOwnedTransition**
- **deriveDefinitionOwnedUsage**
- **deriveDefinitionOwnedUseCase**
- **deriveDefinitionOwnedVerificationCase**
- **deriveDefinitionOwnedView**
- **deriveDefinitionOwnedViewpoint**
- **deriveDefinitionUsage**
- **deriveDefinitionVariant**
- **deriveDefinitionVariantMembership**
- **validateDefinitionVariationIsAbstract**
- **validateDefinitionVariationOwnedFeatureMembership**
- **validateDefinitionVariationSpecialization**
