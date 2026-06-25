---
name: RequirementDefinition
package: Requirements
isAbstract: false
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

### actorParameter : PartUsage [0..*] {derived, ordered}

The parameters of this RequirementDefinition that represent actors involved in the requirement.

Subsets: `parameter`

### assumedConstraint : ConstraintUsage [0..*] {derived, ordered}

The owned ConstraintUsages that represent assumptions of this RequirementDefinition, which are the ownedConstraints of the RequirementConstraintMemberships of the RequirementDefinition with kind = assumption.

Subsets: `ownedFeature`

### framedConcern : ConcernUsage [0..*] {derived, ordered}

The ConcernUsages framed by this RequirementDefinition, which are the ownedConcerns of all FramedConcernMemberships of the RequirementDefinition.

Subsets: `requiredConstraint`

### reqId : String [0..1]

An optional modeler-specified identifier for this RequirementDefinition (used, e.g., to link it to an original requirement text in some source document), which is the declaredShortName for the RequirementDefinition.

Redefines: `declaredShortName`

### requiredConstraint : ConstraintUsage [0..*] {derived, ordered}

The owned ConstraintUsages that represent requirements of this RequirementDefinition, derived as the ownedConstraints of the RequirementConstraintMemberships of the RequirementDefinition with kind = requirement.

Subsets: `ownedFeature`

### stakeholderParameter : PartUsage [0..*] {derived, ordered}

The parameters of this RequirementDefinition that represent stakeholders for the requirement.

Subsets: `parameter`

### subjectParameter : Usage [1..1] {derived}

The parameter of this RequirementDefinition that represents its subject.

Subsets: `parameter`

### text : String [0..*] {derived}

An optional textual statement of the requirement represented by this RequirementDefinition, derived from the bodies of the documentation of the RequirementDefinition.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| differencingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| directedUsage | Usage | [0..*] | [Definition](Definition.md) | derived, ordered |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| endFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| expression | Expression | [0..*] | [Function](Function.md) | derived |
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
| isIndividual | Boolean | [1..1] | [OccurrenceDefinition](OccurrenceDefinition.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isModelLevelEvaluable | Boolean | [1..1] | [Function](Function.md) | derived |
| isSufficient | Boolean | [1..1] | [Type](Type.md) |  |
| isVariation | Boolean | [1..1] | [Definition](Definition.md) |  |
| member | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | Multiplicity | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| output | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedAction | ActionUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedAllocation | AllocationUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedAnalysisCase | AnalysisCaseUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedAttribute | AttributeUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedCalculation | CalculationUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedCase | CaseUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedConcern | ConcernUsage | [0..*] | [Definition](Definition.md) | derived |
| ownedConjugator | Conjugation | [0..1] | [Type](Type.md) | derived, composite |
| ownedConnection | ConnectorAsUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedConstraint | ConstraintUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedDifferencing | Differencing | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedDisjoining | Disjoining | [0..*] | [Type](Type.md) | derived, composite |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedEndFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedEnumeration | EnumerationUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeatureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedFlow | FlowUsage | [0..*] | [Definition](Definition.md) | derived |
| ownedImport | Import | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedInterface | InterfaceUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedIntersecting | Intersecting | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedItem | ItemUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedMember | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedMetadata | MetadataUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedOccurrence | OccurrenceUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedPart | PartUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedPort | PortUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedReference | ReferenceUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| ownedRendering | RenderingUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedRequirement | RequirementUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedSpecialization | Specialization | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedState | StateUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedSubclassification | Subclassification | [0..*] | [Classifier](Classifier.md) | derived, composite |
| ownedTransition | TransitionUsage | [0..*] | [Definition](Definition.md) | derived |
| ownedUnioning | Unioning | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedUsage | Usage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedUseCase | UseCaseUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedVerificationCase | VerificationCaseUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedView | ViewUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| ownedViewpoint | ViewpointUsage | [0..*] | [Definition](Definition.md) | derived, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| parameter | Feature | [0..*] | [Behavior](Behavior.md) | derived, ordered |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| result | Feature | [1..1] | [Function](Function.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| step | Step | [0..*] | [Behavior](Behavior.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| usage | Usage | [0..*] | [Definition](Definition.md) | derived, ordered |
| variant | Usage | [0..*] | [Definition](Definition.md) | derived |
| variantMembership | VariantMembership | [0..*] | [Definition](Definition.md) | derived, composite |

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

