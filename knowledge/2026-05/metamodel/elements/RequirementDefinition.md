---
name: RequirementDefinition
package: Requirements
fully qualified name: SysML::Systems::Requirements::RequirementDefinition
isAbstract: false
visibility: public
generalizes: [ConstraintDefinition]
specializedBy: [ConcernDefinition, ViewpointDefinition]
---

# RequirementDefinition

`Requirements` package · concrete metaclass

A RequirementDefinition is a ConstraintDefinition that defines a requirement used in the context of a specification as a constraint that a valid solution must satisfy. The specification is relative to a specified subject, possibly in collaboration with one or more external actors.

## Generalizations

- [ConstraintDefinition](ConstraintDefinition.md)

## Specializations

- [ConcernDefinition](ConcernDefinition.md)
- [ViewpointDefinition](ViewpointDefinition.md)

## Owned features

### actorParameter

`+` [PartUsage](PartUsage.md) · `[0..*]` · *derived, ordered*

The parameters of this RequirementDefinition that represent actors involved in the requirement.

Subsets [parameter](Behavior.md#parameter), [usage](Definition.md#usage)

### assumedConstraint

`+` [ConstraintUsage](ConstraintUsage.md) · `[0..*]` · *derived, ordered*

The owned ConstraintUsages that represent assumptions of this RequirementDefinition, which are the ownedConstraints of the RequirementConstraintMemberships of the RequirementDefinition with kind = assumption.

Subsets [ownedFeature](Type.md#ownedfeature)

### framedConcern

`+` [ConcernUsage](ConcernUsage.md) · `[0..*]` · *derived, ordered*

The ConcernUsages framed by this RequirementDefinition, which are the ownedConcerns of all FramedConcernMemberships of the RequirementDefinition.

Subsets [requiredConstraint](#requiredconstraint)

### reqId

`+` [String](String.md) · `[0..1]`

An optional modeler-specified identifier for this RequirementDefinition (used, e.g., to link it to an original requirement text in some source document), which is the declaredShortName for the RequirementDefinition.

Redefines [declaredShortName](Element.md#declaredshortname)

### requiredConstraint

`+` [ConstraintUsage](ConstraintUsage.md) · `[0..*]` · *derived, ordered*

The owned ConstraintUsages that represent requirements of this RequirementDefinition, derived as the ownedConstraints of the RequirementConstraintMemberships of the RequirementDefinition with kind = requirement.

Subsets [ownedFeature](Type.md#ownedfeature)

### stakeholderParameter

`+` [PartUsage](PartUsage.md) · `[0..*]` · *derived, ordered*

The parameters of this RequirementDefinition that represent stakeholders for the requirement.

Subsets [parameter](Behavior.md#parameter), [usage](Definition.md#usage)

### subjectParameter

`+` [Usage](Usage.md) · `[1..1]` · *derived*

The parameter of this RequirementDefinition that represents its subject.

Subsets [parameter](Behavior.md#parameter), [usage](Definition.md#usage)

### text

`+` [String](String.md) · `[0..*]` · *derived*

An optional textual statement of the requirement represented by this RequirementDefinition, derived from the bodies of the documentation of the RequirementDefinition.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
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
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| usage | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived, ordered |
| variant | [Usage](Usage.md) | [0..*] | [Definition](Definition.md) | derived |
| variantMembership | [VariantMembership](VariantMembership.md) | [0..*] | [Definition](Definition.md) | derived, composite |

## Constraints

### checkRequirementDefinitionSpecialization

A RequirementDefinition must directly or indirectly specialize the base RequirementDefinition Requirements::RequirementCheck from the Systems Model Library.

```ocl
specializesFromLibrary('Requirements::RequirementCheck')
```

### deriveRequirementDefinitionActorParameter

The actorParameters of a RequirementDefinition are the ownedActorParameters of the ActorMemberships of the RequirementDefinition.

```ocl
actorParameter = featureMembership->
    selectByKind(ActorMembership).
    ownedActorParameter
```

### deriveRequirementDefinitionAssumedConstraint

The assumedConstraints of a RequirementDefinition are the ownedConstraints of the RequirementConstraintMemberships of the RequirementDefinition with kind = assumption.

```ocl
assumedConstraint = ownedFeatureMembership->
    selectByKind(RequirementConstraintMembership)->
    select(kind = RequirementConstraintKind::assumption).
    ownedConstraint
```

### deriveRequirementDefinitionFramedConcern

The framedConcerns of a RequirementDefinition are the ownedConcerns of the FramedConcernMemberships of the RequirementDefinition.

```ocl
framedConcern = featureMembership->
    selectByKind(FramedConcernMembership).
    ownedConcern
```

### deriveRequirementDefinitionRequiredConstraint

The requiredConstraints of a RequirementDefinition are the ownedConstraints of the RequirementConstraintMemberships of the RequirementDefinition with kind = requirement.

```ocl
requiredConstraint = ownedFeatureMembership->
    selectByKind(RequirementConstraintMembership)->
    select(kind = RequirementConstraintKind::requirement).
    ownedConstraint
```

### deriveRequirementDefinitionStakeholderParameter

The stakeHolderParameters of a RequirementDefinition are the ownedStakeholderParameters of the StakeholderMemberships of the RequirementDefinition.

```ocl
stakeholderParameter = featureMembership->
    selectByKind(StakholderMembership).
    ownedStakeholderParameter
```

### deriveRequirementDefinitionSubjectParameter

The subjectParameter of a RequirementDefinition is the ownedSubjectParameter of its SubjectMembership (if any).

```ocl
subjectParameter =
    let subjects : OrderedSet(SubjectMembership) = 
        featureMembership->selectByKind(SubjectMembership) in
    if subjects->isEmpty() then null
    else subjects->first().ownedSubjectParameter
    endif
```

### deriveRequirementDefinitionText

The texts of aRequirementDefinition are the bodies of the documentation of the RequirementDefinition.

```ocl
text = documentation.body
```

### validateRequirementDefinitionOnlyOneSubject

A RequirementDefinition must have at most one featureMembership that is a SubjectMembership.

```ocl
featureMembership->	
    selectByKind(SubjectMembership)->
    size() <= 1
```

### validateRequirementDefinitionSubjectParameterPosition

The subjectParameter of a RequirementDefinition must be its first input.

```ocl
input->notEmpty() and input->first() = subjectParameter
```

