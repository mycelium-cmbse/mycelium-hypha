---
name: SatisfyRequirementUsage
package: Requirements
isAbstract: false
generalizes: [AssertConstraintUsage, RequirementUsage]
specializedBy: []
---

# SatisfyRequirementUsage

`Requirements` package · concrete metaclass

A SatisfyRequirementUsage is an AssertConstraintUsage that asserts, by default, that a satisfied RequirementUsage is true for a specific satisfyingFeature, or, if isNegated = true, that the RequirementUsage is false. The satisfied RequirementUsage is related to the SatisfyRequirementUsage by a ReferenceSubsetting Relationship.

## Generalizations

- [AssertConstraintUsage](AssertConstraintUsage.md)
- [RequirementUsage](RequirementUsage.md)

## Owned features

### satisfiedRequirement : RequirementUsage [1..1] {derived}

The RequirementUsage that is satisfied by the satisfyingSubject of this SatisfyRequirementUsage. It is the assertedConstraint of the SatisfyRequirementUsage considered as an AssertConstraintUsage, which must be a RequirementUsage.

Redefines: `assertedConstraint`

### satisfyingFeature : Feature [1..1] {derived}

The Feature that represents the actual subject that is asserted to satisfy the satisfiedRequirement. The satisfyingFeature is bound to the subjectParameter of the SatisfyRequirementUsage.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| actorParameter | PartUsage | [0..*] | [RequirementUsage](RequirementUsage.md) | derived, ordered |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| assertedConstraint | ConstraintUsage | [1..1] | [AssertConstraintUsage](AssertConstraintUsage.md) | derived |
| assumedConstraint | ConstraintUsage | [0..*] | [RequirementUsage](RequirementUsage.md) | derived, ordered |
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
| framedConcern | ConcernUsage | [0..*] | [RequirementUsage](RequirementUsage.md) | derived, ordered |
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
| isNegated | Boolean | [1..1] | [Invariant](Invariant.md) |  |
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
| reqId | String | [0..1] | [RequirementUsage](RequirementUsage.md) |  |
| requiredConstraint | ConstraintUsage | [0..*] | [RequirementUsage](RequirementUsage.md) | derived, ordered |
| requirementDefinition | RequirementDefinition | [0..1] | [RequirementUsage](RequirementUsage.md) | derived |
| result | Feature | [1..1] | [Expression](Expression.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| stakeholderParameter | PartUsage | [0..*] | [RequirementUsage](RequirementUsage.md) | derived, ordered |
| subjectParameter | Usage | [1..1] | [RequirementUsage](RequirementUsage.md) | derived |
| text | String | [0..*] | [RequirementUsage](RequirementUsage.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| type | Type | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| usage | Usage | [0..*] | [Usage](Usage.md) | derived, ordered |
| variant | Usage | [0..*] | [Usage](Usage.md) | derived |
| variantMembership | VariantMembership | [0..*] | [Usage](Usage.md) | derived, composite |

## Constraints

### checkSatisfyRequirementUsageBindingConnector

A SatisfyRequirementUsage must have exactly one ownedMember that is a BindingConnector between its subjectParameter and some Feature other than the subjectParameter.

```ocl
ownedMember->selectByKind(BindingConnector)->
    select(b |
        b.relatedElement->includes(subjectParameter) and
        b.relatedElement->exists(r | r <> subjectParameter))->
    size() = 1
```

### checkSatisfyRequirementUsageSpecialization

If a SatisfyRequirementUsage is negated, then it must directly or indirectly specialize the RequirementUsage Requirements::notSatisfiedRequirementChecks. Otherwise, it must directly or indirectly specialize the RequirementUsage Requirements::satisfiedRequirementChecks.

```ocl
if isNegated then
    specializesFromLibrary('Requirements::notSatisfiedRequirementChecks')
else
    specializesFromLibrary('Requirements::satisfiedRequirementChecks')
endif
```

### deriveSatisfyRequirementUsageSatisfyingFeature

The satisfyingFeature of a SatisfyRequirementUsage is the Feature to which the subjectParameter is bound.

```ocl
satisfyingFeature =
    let bindings: BindingConnector = ownedMember->
        selectByKind(BindingConnector)->
        select(b | b.relatedElement->includes(subjectParameter)) in
    if bindings->isEmpty() or 
       not bindings->first().relatedElement->exits(r | r <> subjectParameter) 
    then null
    else bindings->first().relatedElement->any(r | r <> subjectParameter)
    endif
```

### validateSatisfyRequirementUsageReference

If a SatisfyRequirementUsage has an ownedReferenceSubsetting, then the featureTarget of its referencedFeature must be a RequirementUsage.

```ocl
referencedFeatureTarget() <> null implies
    referencedFeatureTarget().oclIsKindOf(RequirementUsage)
```

