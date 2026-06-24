# Usage

`DefinitionAndUsage` package · concrete metaclass

A Usage is a usage of a Definition.A Usage may have nestedUsages that model features that apply in the context of the owningUsage. A Usage may also have Definitions nested in it, but this has no semantic significance, other than the nested scoping resulting from the Usage being considered as a Namespace for any nested Definitions.However, if a Usage has isVariation = true, then it represents a variation point Usage. In this case, all of its members must be variant Usages, related to the Usage by VariantMembership Relationships. Rather than being features of the Usage, variant Usages model different concrete alternatives that can be chosen to fill in for the variation point Usage.

## Owned features

### definition : &#171;untyped&#187; [0..*]

The Classifiers that are the types of this Usage. Nominally, these are Definitions, but other kinds of Kernel Classifiers are also allowed, to permit use of Classifiers from the Kernel Model Libraries.

### directedUsage : Usage [0..*]

The usages of this Usage that are directedFeatures.

### isReference : Boolean [1..1]

Whether this Usage is a referential Usage, that is, it has isComposite = false.

### isVariation : Boolean [1..1]

Whether this Usage is for a variation point or not. If true, then all the memberships of the Usage must be VariantMemberships.

### mayTimeVary : Boolean [1..1]

Whether this Usage may be time varying (that is, whether it is featured by the snapshots of its owningType, rather than being featured by the owningType itself). However, if isConstant is also true, then the value of the Usage is nevertheless constant over the entire duration of an instance of its owningType (that is, it has the same value on all snapshots).The property mayTimeVary redefines the KerML property Feature::isVariable, making it derived. The property isConstant is inherited from Feature.

### nestedAction : ActionUsage [0..*]

The ActionUsages that are nestedUsages of this Usage.

Subsets: `nestedOccurrence`

### nestedAllocation : AllocationUsage [0..*]

The AllocationUsages that are nestedUsages of this Usage.

Subsets: `nestedConnection`

### nestedAnalysisCase : AnalysisCaseUsage [0..*]

The AnalysisCaseUsages that are nestedUsages of this Usage.

Subsets: `nestedCase`

### nestedAttribute : AttributeUsage [0..*]

The code>AttributeUsages that are nestedUsages of this Usage.

Subsets: `nestedUsage`

### nestedCalculation : CalculationUsage [0..*]

The CalculationUsage that are nestedUsages of this Usage.

Subsets: `nestedAction`

### nestedCase : CaseUsage [0..*]

The CaseUsages that are nestedUsages of this Usage.

Subsets: `nestedCalculation`

### nestedConcern : ConcernUsage [0..*]

The ConcernUsages that are nestedUsages of this Usage.

Subsets: `nestedRequirement`

### nestedConnection : ConnectorAsUsage [0..*]

The ConnectorAsUsages that are nestedUsages of this Usage. Note that this list includes BindingConnectorAsUsages, SuccessionAsUsages, and FlowUsages because these are ConnectorAsUsages even though they are not ConnectionUsages.

Subsets: `nestedUsage`

### nestedConstraint : ConstraintUsage [0..*]

The ConstraintUsages that are nestedUsages of this Usage.

Subsets: `nestedOccurrence`

### nestedEnumeration : EnumerationUsage [0..*]

The code>EnumerationUsages that are nestedUsages of this Usage.

Subsets: `nestedAttribute`

### nestedFlow : FlowUsage [0..*]

The code>FlowUsages that are nestedUsages of this Usage.

Subsets: `nestedConnection`

### nestedInterface : InterfaceUsage [0..*]

The InterfaceUsages that are nestedUsages of this Usage.

Subsets: `nestedConnection`

### nestedItem : ItemUsage [0..*]

The ItemUsages that are nestedUsages of this Usage.

Subsets: `nestedOccurrence`

### nestedMetadata : MetadataUsage [0..*]

The MetadataUsages that are ownedMembers of this of this Usage.

Subsets: `nestedItem`

### nestedOccurrence : OccurrenceUsage [0..*]

The OccurrenceUsages that are nestedUsages of this Usage.

Subsets: `nestedUsage`

### nestedPart : PartUsage [0..*]

The PartUsages that are nestedUsages of this Usage.

Subsets: `nestedItem`

### nestedPort : PortUsage [0..*]

The PortUsages that are nestedUsages of this Usage.

Subsets: `nestedUsage`

### nestedReference : ReferenceUsage [0..*]

The ReferenceUsages that are nestedUsages of this Usage.

Subsets: `nestedUsage`

### nestedRendering : RenderingUsage [0..*]

The RenderingUsages that are nestedUsages of this Usage.

Subsets: `nestedPart`

### nestedRequirement : RequirementUsage [0..*]

The RequirementUsages that are nestedUsages of this Usage.

Subsets: `nestedConstraint`

### nestedState : StateUsage [0..*]

The StateUsages that are nestedUsages of this Usage.

Subsets: `nestedAction`

### nestedTransition : TransitionUsage [0..*]

The TransitionUsages that are nestedUsages of this Usage.

Subsets: `nestedUsage`

### nestedUsage : Usage [0..*]

The Usages that are ownedFeatures of this Usage.

### nestedUseCase : UseCaseUsage [0..*]

The UseCaseUsages that are nestedUsages of this Usage.

Subsets: `nestedCase`

### nestedVerificationCase : VerificationCaseUsage [0..*]

The VerificationCaseUsages that are nestedUsages of this Usage.

Subsets: `nestedCase`

### nestedView : ViewUsage [0..*]

The ViewUsages that are nestedUsages of this Usage.

Subsets: `nestedPart`

### nestedViewpoint : ViewpointUsage [0..*]

The ViewpointUsages that are nestedUsages of this Usage.

Subsets: `nestedRequirement`

### owningDefinition : Definition [0..1]

The Definition that owns this Usage (if any).

### owningUsage : Usage [0..1]

The Usage in which this Usage is nested (if any).

### usage : Usage [0..*]

The Usages that are features of this Usage (not necessarily owned).

### variant : Usage [0..*]

The Usages which represent the variants of this Usage as a variation point Usage, if isVariation = true. If isVariation = false, then there must be no variants.

### variantMembership : VariantMembership [0..*]

The ownedMemberships of this Usage that are VariantMemberships. If isVariation = true, then this must be all memberships of the Usage. If isVariation = false, then variantMembershipmust be empty.


## Constraints

- **checkUsageVariationDefinitionSpecialization**
- **checkUsageVariationUsageSpecialization**
- **checkUsageVariationUsageTypeFeaturing**
- **deriveUsageDirectedUsage**
- **deriveUsageIsReference**
- **deriveUsageMayTimeVary**
- **deriveUsageNestedAction**
- **deriveUsageNestedAllocation**
- **deriveUsageNestedAnalysisCase**
- **deriveUsageNestedAttribute**
- **deriveUsageNestedCalculation**
- **deriveUsageNestedCase**
- **deriveUsageNestedConcern**
- **deriveUsageNestedConnection**
- **deriveUsageNestedConstraint**
- **deriveUsageNestedEnumeration**
- **deriveUsageNestedFlow**
- **deriveUsageNestedInterface**
- **deriveUsageNestedItem**
- **deriveUsageNestedMetadata**
- **deriveUsageNestedOccurrence**
- **deriveUsageNestedPart**
- **deriveUsageNestedPort**
- **deriveUsageNestedReference**
- **deriveUsageNestedRendering**
- **deriveUsageNestedRequirement**
- **deriveUsageNestedState**
- **deriveUsageNestedTransition**
- **deriveUsageNestedUsage**
- **deriveUsageNestedUseCase**
- **deriveUsageNestedVerificationCase**
- **deriveUsageNestedView**
- **deriveUsageNestedViewpoint**
- **deriveUsageUsage**
- **deriveUsageVariant**
- **deriveUsageVariantMembership**
- **validateUsageIsReferential**
- **validateUsageVariationIsAbstract**
- **validateUsageVariationOwnedFeatureMembership**
- **validateUsageVariationSpecialization**
