---
name: Usage
package: DefinitionAndUsage
fully qualified name: SysML::Systems::DefinitionAndUsage::Usage
isAbstract: false
visibility: public
generalizes: [Feature]
specializedBy: [AttributeUsage, ConnectorAsUsage, OccurrenceUsage, ReferenceUsage]
---

# Usage

`DefinitionAndUsage` package · concrete metaclass

A Usage is a usage of a Definition.A Usage may have nestedUsages that model features that apply in the context of the owningUsage. A Usage may also have Definitions nested in it, but this has no semantic significance, other than the nested scoping resulting from the Usage being considered as a Namespace for any nested Definitions.However, if a Usage has isVariation = true, then it represents a variation point Usage. In this case, all of its members must be variant Usages, related to the Usage by VariantMembership Relationships. Rather than being features of the Usage, variant Usages model different concrete alternatives that can be chosen to fill in for the variation point Usage.

## Generalizations

- [Feature](Feature.md)

## Specializations

- [AttributeUsage](AttributeUsage.md)
- [ConnectorAsUsage](ConnectorAsUsage.md)
- [OccurrenceUsage](OccurrenceUsage.md)
- [ReferenceUsage](ReferenceUsage.md)

## Owned features

### definition

`+` [Classifier](Classifier.md) · `[0..*]` · *derived, ordered*

The Classifiers that are the types of this Usage. Nominally, these are Definitions, but other kinds of Kernel Classifiers are also allowed, to permit use of Classifiers from the Kernel Model Libraries.

Redefines [type](Feature.md#type)

### directedUsage

`+` [Usage](Usage.md) · `[0..*]` · *derived, ordered*

The usages of this Usage that are directedFeatures.

Subsets [directedFeature](Type.md#directedfeature)

### isReference

`+` [Boolean](Boolean.md) · `[1..1]` · *derived*

Whether this Usage is a referential Usage, that is, it has isComposite = false.

### isVariation

`+` [Boolean](Boolean.md) · `[1..1]`

Whether this Usage is for a variation point or not. If true, then all the memberships of the Usage must be VariantMemberships.

### mayTimeVary

`+` [Boolean](Boolean.md) · `[1..1]` · *derived*

Whether this Usage may be time varying (that is, whether it is featured by the snapshots of its owningType, rather than being featured by the owningType itself). However, if isConstant is also true, then the value of the Usage is nevertheless constant over the entire duration of an instance of its owningType (that is, it has the same value on all snapshots).The property mayTimeVary redefines the KerML property Feature::isVariable, making it derived. The property isConstant is inherited from Feature.

Redefines [isVariable](Feature.md#isvariable)

### nestedAction

`+` [ActionUsage](ActionUsage.md) · `[0..*]` · *derived, ordered*

The ActionUsages that are nestedUsages of this Usage.

Subsets [nestedOccurrence](#nestedoccurrence)

### nestedAllocation

`+` [AllocationUsage](AllocationUsage.md) · `[0..*]` · *derived, ordered*

The AllocationUsages that are nestedUsages of this Usage.

Subsets [nestedConnection](#nestedconnection)

### nestedAnalysisCase

`+` [AnalysisCaseUsage](AnalysisCaseUsage.md) · `[0..*]` · *derived, ordered*

The AnalysisCaseUsages that are nestedUsages of this Usage.

Subsets [nestedCase](#nestedcase)

### nestedAttribute

`+` [AttributeUsage](AttributeUsage.md) · `[0..*]` · *derived, ordered*

The code>AttributeUsages that are nestedUsages of this Usage.

Subsets [nestedUsage](#nestedusage)

### nestedCalculation

`+` [CalculationUsage](CalculationUsage.md) · `[0..*]` · *derived, ordered*

The CalculationUsage that are nestedUsages of this Usage.

Subsets [nestedAction](#nestedaction)

### nestedCase

`+` [CaseUsage](CaseUsage.md) · `[0..*]` · *derived, ordered*

The CaseUsages that are nestedUsages of this Usage.

Subsets [nestedCalculation](#nestedcalculation)

### nestedConcern

`+` [ConcernUsage](ConcernUsage.md) · `[0..*]` · *derived*

The ConcernUsages that are nestedUsages of this Usage.

Subsets [nestedRequirement](#nestedrequirement)

### nestedConnection

`+` [ConnectorAsUsage](ConnectorAsUsage.md) · `[0..*]` · *derived, ordered*

The ConnectorAsUsages that are nestedUsages of this Usage. Note that this list includes BindingConnectorAsUsages, SuccessionAsUsages, and FlowUsages because these are ConnectorAsUsages even though they are not ConnectionUsages.

Subsets [nestedUsage](#nestedusage)

### nestedConstraint

`+` [ConstraintUsage](ConstraintUsage.md) · `[0..*]` · *derived, ordered*

The ConstraintUsages that are nestedUsages of this Usage.

Subsets [nestedOccurrence](#nestedoccurrence)

### nestedEnumeration

`+` [EnumerationUsage](EnumerationUsage.md) · `[0..*]` · *derived, ordered*

The code>EnumerationUsages that are nestedUsages of this Usage.

Subsets [nestedAttribute](#nestedattribute)

### nestedFlow

`+` [FlowUsage](FlowUsage.md) · `[0..*]` · *derived*

The code>FlowUsages that are nestedUsages of this Usage.

Subsets [nestedConnection](#nestedconnection)

### nestedInterface

`+` [InterfaceUsage](InterfaceUsage.md) · `[0..*]` · *derived, ordered*

The InterfaceUsages that are nestedUsages of this Usage.

Subsets [nestedConnection](#nestedconnection)

### nestedItem

`+` [ItemUsage](ItemUsage.md) · `[0..*]` · *derived, ordered*

The ItemUsages that are nestedUsages of this Usage.

Subsets [nestedOccurrence](#nestedoccurrence)

### nestedMetadata

`+` [MetadataUsage](MetadataUsage.md) · `[0..*]` · *derived, ordered*

The MetadataUsages that are ownedMembers of this of this Usage.

Subsets [nestedItem](#nesteditem)

### nestedOccurrence

`+` [OccurrenceUsage](OccurrenceUsage.md) · `[0..*]` · *derived, ordered*

The OccurrenceUsages that are nestedUsages of this Usage.

Subsets [nestedUsage](#nestedusage)

### nestedPart

`+` [PartUsage](PartUsage.md) · `[0..*]` · *derived, ordered*

The PartUsages that are nestedUsages of this Usage.

Subsets [nestedItem](#nesteditem)

### nestedPort

`+` [PortUsage](PortUsage.md) · `[0..*]` · *derived, ordered*

The PortUsages that are nestedUsages of this Usage.

Subsets [nestedUsage](#nestedusage)

### nestedReference

`+` [ReferenceUsage](ReferenceUsage.md) · `[0..*]` · *derived, ordered*

The ReferenceUsages that are nestedUsages of this Usage.

Subsets [nestedUsage](#nestedusage)

### nestedRendering

`+` [RenderingUsage](RenderingUsage.md) · `[0..*]` · *derived, ordered*

The RenderingUsages that are nestedUsages of this Usage.

Subsets [nestedPart](#nestedpart)

### nestedRequirement

`+` [RequirementUsage](RequirementUsage.md) · `[0..*]` · *derived, ordered*

The RequirementUsages that are nestedUsages of this Usage.

Subsets [nestedConstraint](#nestedconstraint)

### nestedState

`+` [StateUsage](StateUsage.md) · `[0..*]` · *derived, ordered*

The StateUsages that are nestedUsages of this Usage.

Subsets [nestedAction](#nestedaction)

### nestedTransition

`+` [TransitionUsage](TransitionUsage.md) · `[0..*]` · *derived*

The TransitionUsages that are nestedUsages of this Usage.

Subsets [nestedUsage](#nestedusage)

### nestedUsage

`+` [Usage](Usage.md) · `[0..*]` · *derived, ordered*

The Usages that are ownedFeatures of this Usage.

Subsets [ownedFeature](Type.md#ownedfeature)

### nestedUseCase

`+` [UseCaseUsage](UseCaseUsage.md) · `[0..*]` · *derived, ordered*

The UseCaseUsages that are nestedUsages of this Usage.

Subsets [nestedCase](#nestedcase)

### nestedVerificationCase

`+` [VerificationCaseUsage](VerificationCaseUsage.md) · `[0..*]` · *derived, ordered*

The VerificationCaseUsages that are nestedUsages of this Usage.

Subsets [nestedCase](#nestedcase)

### nestedView

`+` [ViewUsage](ViewUsage.md) · `[0..*]` · *derived, ordered*

The ViewUsages that are nestedUsages of this Usage.

Subsets [nestedPart](#nestedpart)

### nestedViewpoint

`+` [ViewpointUsage](ViewpointUsage.md) · `[0..*]` · *derived, ordered*

The ViewpointUsages that are nestedUsages of this Usage.

Subsets [nestedRequirement](#nestedrequirement)

### owningDefinition

`+` [Definition](Definition.md) · `[0..1]` · *derived*

The Definition that owns this Usage (if any).

Subsets [owningType](Feature.md#owningtype)

### owningUsage

`+` [Usage](Usage.md) · `[0..1]` · *derived*

The Usage in which this Usage is nested (if any).

Subsets [owningType](Feature.md#owningtype)

### usage

`+` [Usage](Usage.md) · `[0..*]` · *derived, ordered*

The Usages that are features of this Usage (not necessarily owned).

Subsets [feature](Type.md#feature)

### variant

`+` [Usage](Usage.md) · `[0..*]` · *derived*

The Usages which represent the variants of this Usage as a variation point Usage, if isVariation = true. If isVariation = false, then there must be no variants.

Subsets [ownedMember](Namespace.md#ownedmember)

### variantMembership

`+` [VariantMembership](VariantMembership.md) · `[0..*]` · *derived, composite*

The ownedMemberships of this Usage that are VariantMemberships. If isVariation = true, then this must be all memberships of the Usage. If isVariation = false, then variantMembershipmust be empty.

Subsets [ownedMembership](Namespace.md#ownedmembership)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| chainingFeature | [Feature](Feature.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| crossFeature | [Feature](Feature.md) | [0..1] | [Feature](Feature.md) | derived |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| differencingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| direction | [FeatureDirectionKind](FeatureDirectionKind.md) | [0..1] | [Feature](Feature.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| endFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| endOwningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| feature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | [FeatureMembership](FeatureMembership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| featureTarget | [Feature](Feature.md) | [1..1] | [Feature](Feature.md) | derived |
| featuringType | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| importedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | [Membership](Membership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| input | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | [Boolean](Boolean.md) | [1..1] | [Type](Type.md) |  |
| isComposite | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| isConjugated | [Boolean](Boolean.md) | [1..1] | [Type](Type.md) | derived |
| isConstant | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| isDerived | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| isEnd | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| isOrdered | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| isPortion | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| isSufficient | [Boolean](Boolean.md) | [1..1] | [Type](Type.md) |  |
| isUnique | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| isVariable | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | [Multiplicity](Multiplicity.md) | [0..1] | [Type](Type.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
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
| owningFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..1] | [Feature](Feature.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| type | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkUsageVariationDefinitionSpecialization

If a Usage has an owningVariationDefinition, then it must directly or indirectly specialize that Definition.

```ocl
owningVariationDefinition <> null implies
    specializes(owningVariationDefinition)
```

### checkUsageVariationUsageSpecialization

If a Usage has an owningVariationUsage, then it must directly or indirectly specialize that Usage.

```ocl
owningVariationUsage <> null implies
    specializes(owningVariationUsage)
```

### checkUsageVariationUsageTypeFeaturing

If a Usage has an owningVariationUsage, then it must have the same featuringTypes as that Usage.

```ocl
owningVariationUsage <> null implies
    featuringType->asSet() = owningVariationUsage.featuringType->asSet()
```

### deriveUsageDirectedUsage

The directedUsages of a Usage are all its directedFeatures that are Usages.

```ocl
directedUsage = directedFeature->selectByKind(Usage)
```

### deriveUsageIsReference

A Usage is referential if it is not composite.

```ocl
isReference = not isComposite
```

### deriveUsageMayTimeVary

A Usage mayTimeVary if and only if all of the following are true<ul> <li>It has an owningType that specializes Occurrences::Occurrence (from the Kernel Semantic Library).</li> <li>It is not a portion.</li> <li>It does not specialize Links::SelfLink or Occurrences::HappensLink (from the Kernel Semantic Library).</li> <li>If isComposite = true, it does not specialize Actions::Action (from the Systems Model Library).</li></ul>

```ocl
mayTimeVary =
    owningType <> null and
    owningType.specializesFromLibrary('Occurrences::Occurrence') and
    not (
        isPortion or
        specializesFromLibrary('Links::SelfLink') or
        specializesFromLibrary('Occurrences::HappensLink') or
        isComposite and specializesFromLibrary('Actions::Action')
    )
```

### deriveUsageNestedAction

The ownedActions of a Usage are all its ownedUsages that are ActionUsages.

```ocl
nestedAction = nestedUsage->selectByKind(ActionUsage)
```

### deriveUsageNestedAllocation

The ownedAllocations of a Usage are all its ownedUsages that are AllocationUsages.

```ocl
nestedAllocation = nestedUsage->selectByKind(AllocationUsage)
```

### deriveUsageNestedAnalysisCase

The ownedAnalysisCases of a Usage are all its ownedUsages that are AnalysisCaseUsages.

```ocl
nestedAnalysisCase = nestedUsage->selectByKind(AnalysisCaseUsage)
```

### deriveUsageNestedAttribute

The ownedAttributes of a Usage are all its ownedUsages that are AttributeUsages.

```ocl
nestedAttribute = nestedUsage->selectByKind(AttributeUsage)
```

### deriveUsageNestedCalculation

The ownedCalculations of a Usage are all its ownedUsages that are CalculationUsages.

```ocl
nestedCalculation = nestedUsage->selectByKind(CalculationUsage)
```

### deriveUsageNestedCase

The ownedCases of a Usage are all its ownedUsages that are CaseUsages.

```ocl
nestedCase = nestedUsage->selectByKind(CaseUsage)
```

### deriveUsageNestedConcern

The ownedConcerns of a Usage are all its ownedUsages that are ConcernUsages.

```ocl
nestedConcern = nestedUsage->selectByKind(ConcernUsage)
```

### deriveUsageNestedConnection

The ownedConnections of a Usage are all its ownedUsages that are ConnectorAsUsages.

```ocl
nestedConnection = nestedUsage->selectByKind(ConnectorAsUsage)
```

### deriveUsageNestedConstraint

The ownedConstraints of a Usage are all its ownedUsages that are ConstraintUsages.

```ocl
nestedConstraint = nestedUsage->selectByKind(ConstraintUsage)
```

### deriveUsageNestedEnumeration

The ownedEnumerations of a Usage are all its ownedUsages that are EnumerationUsages.

```ocl
ownedNested = nestedUsage->selectByKind(EnumerationUsage)
```

### deriveUsageNestedFlow

The ownedFlows of a Usage are all its ownedUsages that are FlowConnectionUsages.

```ocl
nestedFlow = nestedUsage->selectByKind(FlowUsage)
```

### deriveUsageNestedInterface

The ownedInterfaces of a Usage are all its ownedUsages that are InterfaceUsages.

```ocl
nestedInterface = nestedUsage->selectByKind(ReferenceUsage)
```

### deriveUsageNestedItem

The ownedItems of a Usage are all its ownedUsages that are ItemUsages.

```ocl
nestedItem = nestedUsage->selectByKind(ItemUsage)
```

### deriveUsageNestedMetadata

The ownedMetadata of a Usage are all its ownedMembers that are MetadataUsages.

```ocl
nestedMetadata = nestedUsage->selectByKind(MetadataUsage)
```

### deriveUsageNestedOccurrence

The ownedOccurrences of a Usage are all its ownedUsages that are OccurrenceUsages.

```ocl
nestedOccurrence = nestedUsage->selectByKind(OccurrenceUsage)
```

### deriveUsageNestedPart

The ownedParts of a Usage are all its ownedUsages that are PartUsages.

```ocl
nestedPart = nestedUsage->selectByKind(PartUsage)
```

### deriveUsageNestedPort

The ownedPorts of a Usage are all its ownedUsages that are PortUsages.

```ocl
nestedPort = nestedUsage->selectByKind(PortUsage)
```

### deriveUsageNestedReference

The ownedReferences of a Usage are all its ownedUsages that are ReferenceUsages.

```ocl
nestedReference = nestedUsage->selectByKind(ReferenceUsage)
```

### deriveUsageNestedRendering

The ownedRenderings of a Usage are all its ownedUsages that are RenderingUsages.

```ocl
nestedRendering = nestedUsage->selectByKind(RenderingUsage)
```

### deriveUsageNestedRequirement

The ownedRequirements of a Usage are all its ownedUsages that are RequirementUsages.

```ocl
nestedRequirement = nestedUsage->selectByKind(RequirementUsage)
```

### deriveUsageNestedState

The ownedStates of a Usage are all its ownedUsages that are StateUsages.

```ocl
nestedState = nestedUsage->selectByKind(StateUsage)
```

### deriveUsageNestedTransition

The ownedTransitions of a Usage are all its ownedUsages that are TransitionUsages.

```ocl
nestedTransition = nestedUsage->selectByKind(TransitionUsage)
```

### deriveUsageNestedUsage

The ownedUsages of a Usage are all its ownedFeatures that are Usages.

```ocl
nestedUsage = ownedFeature->selectByKind(Usage)
```

### deriveUsageNestedUseCase

The ownedUseCases of a Usage are all its ownedUsages that are UseCaseUsages.

```ocl
nestedUseCase = nestedUsage->selectByKind(UseCaseUsage)
```

### deriveUsageNestedVerificationCase

The ownedValidationCases of a Usage are all its ownedUsages that are ValidationCaseUsages.

```ocl
nestedVerificationCase = nestedUsage->selectByKind(VerificationCaseUsage)
```

### deriveUsageNestedView

The ownedViews of a Usage are all its ownedUsages that are ViewUsages.

```ocl
nestedView = nestedUsage->selectByKind(ViewUsage)
```

### deriveUsageNestedViewpoint

The ownedViewpoints of a Usage are all its ownedUsages that are ViewpointUsages.

```ocl
nestedViewpoint = nestedUsage->selectByKind(ViewpointUsage)
```

### deriveUsageUsage

The usages of a Usage are all its features that are Usages.

```ocl
usage = feature->selectByKind(Usage)
```

### deriveUsageVariant

The variants of a Usage are the ownedVariantUsages of its variantMemberships.

```ocl
variant = variantMembership.ownedVariantUsage
```

### deriveUsageVariantMembership

The variantMemberships of a Usage are those ownedMemberships that are VariantMemberships.

```ocl
variantMembership = ownedMembership->selectByKind(VariantMembership)
```

### validateUsageIsReferential

A Usage that is directed, an end feature or has no featuringTypes must be referential.

```ocl
direction <> null or isEnd or featuringType->isEmpty() implies
    isReference
```

### validateUsageVariationIsAbstract

If a Usage is a variation, then it must be abstract.

```ocl
isVariation implies isAbstract
```

### validateUsageVariationOwnedFeatureMembership

If a Usage is a variation, then it must not have any ownedFeatureMemberships.

```ocl
isVariation implies ownedFeatureMembership->isEmpty()
```

### validateUsageVariationSpecialization

A variation Usage may not specialize any variation Definition or Usage.

```ocl
isVariation implies
    not ownedSpecialization.specific->exists(
        oclIsKindOf(Definition) and
        oclAsType(Definition).isVariation or
        oclIsKindOf(Usage) and
        oclAsType(Usage).isVariation)
```

