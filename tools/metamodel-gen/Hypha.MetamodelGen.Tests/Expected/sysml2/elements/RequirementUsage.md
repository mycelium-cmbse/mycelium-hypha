---
name: RequirementUsage
package: Requirements
isAbstract: false
generalizes: [ConstraintUsage]
specializedBy: [ConcernUsage, SatisfyRequirementUsage, ViewpointUsage]
---

# RequirementUsage

`Requirements` package · concrete metaclass

A RequirementUsage is a Usage of a RequirementDefinition.

## Generalizations

- [ConstraintUsage](ConstraintUsage.md)

## Specializations

- [ConcernUsage](ConcernUsage.md)
- [SatisfyRequirementUsage](SatisfyRequirementUsage.md)
- [ViewpointUsage](ViewpointUsage.md)

## Owned features

### actorParameter : PartUsage [0..*] {derived, ordered}

The parameters of this RequirementUsage that represent actors involved in the requirement.

Subsets: `parameter`

### assumedConstraint : ConstraintUsage [0..*] {derived, ordered}

The owned ConstraintUsages that represent assumptions of this RequirementUsage, derived as the ownedConstraints of the RequirementConstraintMemberships of the RequirementUsage with kind = assumption.

Subsets: `ownedFeature`

### framedConcern : ConcernUsage [0..*] {derived, ordered}

The ConcernUsages framed by this RequirementUsage, which are the ownedConcerns of all FramedConcernMemberships of the RequirementUsage.

Subsets: `requiredConstraint`

### reqId : String [0..1]

An optional modeler-specified identifier for this RequirementUsage (used, e.g., to link it to an original requirement text in some source document), which is the declaredShortName for the RequirementUsage.

Redefines: `declaredShortName`

### requiredConstraint : ConstraintUsage [0..*] {derived, ordered}

The owned ConstraintUsages that represent requirements of this RequirementUsage, which are the ownedConstraints of the RequirementConstraintMemberships of the RequirementUsage with kind = requirement.

Subsets: `ownedFeature`

### requirementDefinition : RequirementDefinition [0..1] {derived}

The RequirementDefinition that is the single definition of this RequirementUsage.

Redefines: `constraintDefinition`

### stakeholderParameter : PartUsage [0..*] {derived, ordered}

The parameters of this RequirementUsage that represent stakeholders for the requirement.

Subsets: `parameter`

### subjectParameter : Usage [1..1] {derived}

The parameter of this RequirementUsage that represents its subject.

Subsets: `parameter`

### text : String [0..*] {derived}

An optional textual statement of the requirement represented by this RequirementUsage, derived from the bodies of the documentation of the RequirementUsage.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| behavior | Behavior | [0..*] | [Step](Step.md) | derived, ordered |
| chainingFeature | Feature | [0..*] | [Feature](Feature.md) | derived, ordered |
| constraintDefinition | Predicate | [0..1] | [ConstraintUsage](ConstraintUsage.md) | derived |
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
| function | Function | [0..1] | [Expression](Expression.md) | derived |
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
| isModelLevelEvaluable | Boolean | [1..1] | [Expression](Expression.md) | derived |
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
| predicate | Predicate | [0..1] | [BooleanExpression](BooleanExpression.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| result | Feature | [1..1] | [Expression](Expression.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| type | Type | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| usage | Usage | [0..*] | [Usage](Usage.md) | derived, ordered |
| variant | Usage | [0..*] | [Usage](Usage.md) | derived |
| variantMembership | VariantMembership | [0..*] | [Usage](Usage.md) | derived, composite |

## Constraints

### checkRequirementUsageObjectiveRedefinition

A RequirementUsage whose owningFeatureMembership is a ObjectiveMembership must redefine the objectiveRequirement of each CaseDefinition or CaseUsage that is specialized by the owningType of the RequirementUsage.

```ocl
owningfeatureMembership <> null and
owningfeatureMembership.oclIsKindOf(ObjectiveMembership) implies
    owningType.ownedSpecialization.general->forAll(gen |
        (gen.oclIsKindOf(CaseDefinition) implies
            redefines(gen.oclAsType(CaseDefinition).objectiveRequirement)) and
        (gen.oclIsKindOf(Feature) and 
         gen.oclAsType(Feature).featureTarget.oclIsKindOf(CaseUsage) implies
            redefines(gen.oclAsType(Feature).featureTarget.
                        oclAsType(CaseUsage).objectiveRequirement))
```

### checkRequirementUsageRequirementVerificationSpecialization

A RequirementUsage whose owningFeatureMembership is a RequirementVerificationMembership must directly or indirectly specialize the RequirementUsage VerificationCases::VerificationCase::obj::requirementVerifications.

```ocl
owningFeatureMembership <> null and
owningFeatureMembership.oclIsKindOf(RequirementVerificationMembership) implies
    specializesFromLibrary('VerificationCases::VerificationCase::obj::requirementVerifications')
```

### checkRequirementUsageSpecialization

A RequirementUsage must directly or indirectly specialize the base RequirementUsage Requirements::requirementChecks from the Systems Model Library.

```ocl
specializesFromLibrary('Requirements::requirementChecks')
```

### checkRequirementUsageSubrequirementSpecialization

A composite RequirementUsage whose owningType is a RequirementDefinition or RequirementUsage must directly or indirectly specialize the RequirementUsage Requirements::RequirementCheck::subrequirements from the Systems Model Library.

```ocl
isComposite and owningType <> null and
    (owningType.oclIsKindOf(RequirementDefinition) or
     owningType.oclIsKindOf(RequirementUsage)) implies
    specializesFromLibrary('Requirements::RequirementCheck::subrequirements')
```

### deriveRequirementUsageActorParameter

The actorParameters of a RequirementUsage are the ownedActorParameters of the ActorMemberships of the RequirementUsage.

```ocl
actorParameter = featureMembership->
    selectByKind(ActorMembership).
    ownedActorParameter
```

### deriveRequirementUsageAssumedConstraint

The assumedConstraints of a RequirementUsage are the ownedConstraints of the RequirementConstraintMemberships of the RequirementDefinition with kind = assumption.

```ocl
assumedConstraint = ownedFeatureMembership->
    selectByKind(RequirementConstraintMembership)->
    select(kind = RequirementConstraintKind::assumption).
    ownedConstraint
```

### deriveRequirementUsageFramedConcern

The framedConcerns of a RequirementUsage are the ownedConcerns of the FramedConcernMemberships of the RequirementUsage.

```ocl
framedConcern = featureMembership->
    selectByKind(FramedConcernMembership).
    ownedConcern
```

### deriveRequirementUsageRequiredConstraint

The requiredConstraints of a RequirementUsage are the ownedConstraints of the RequirementConstraintMemberships of the RequirementUsage with kind = requirement.

```ocl
requiredConstraint = ownedFeatureMembership->
    selectByKind(RequirementConstraintMembership)->
    select(kind = RequirementConstraintKind::requirement).
    ownedConstraint
```

### deriveRequirementUsageStakeholderParameter

The stakeHolderParameters of a RequirementUsage are the ownedStakeholderParameters of the StakeholderMemberships of the RequirementUsage.

```ocl
stakeholderParameter = featureMembership->
    selectByKind(AStakholderMembership).
    ownedStakeholderParameter
```

### deriveRequirementUsageSubjectParameter

The subjectParameter of a RequirementUsage is the ownedSubjectParameter of its SubjectMembership (if any).

```ocl
subjectParameter =
    let subjects : OrderedSet(SubjectMembership) = 
        featureMembership->selectByKind(SubjectMembership) in
    if subjects->isEmpty() then null
    else subjects->first().ownedSubjectParameter
    endif
```

### deriveRequirementUsageText

The texts of aRequirementUsage are the bodies of the documentation of the RequirementUsage.

```ocl
text = documentation.body
```

### validateRequirementUsageOnlyOneSubject

A RequirementDefinition must have at most one featureMembership that is a SubjectMembership.

```ocl
featureMembership->
    selectByKind(SubjectMembership)->
    size() <= 1
```

### validateRequirementUsageSubjectParameterPosition

The subjectParameter of a RequirementUsage must be its first input.

```ocl
input->notEmpty() and input->first() = subjectParameter
```

