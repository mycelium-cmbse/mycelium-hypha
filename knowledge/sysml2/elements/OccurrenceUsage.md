---
name: OccurrenceUsage
package: Occurrences
isAbstract: false
generalizes: [Usage]
specializedBy: [ActionUsage, ConstraintUsage, EventOccurrenceUsage, ItemUsage, PortUsage]
---

# OccurrenceUsage

`Occurrences` package · concrete metaclass

An OccurrenceUsage is a Usage whose types are all Classes. Nominally, if a type is an OccurrenceDefinition, an OccurrenceUsage is a Usage of that OccurrenceDefinition within a system. However, other types of Kernel Classes are also allowed, to permit use of Classes from the Kernel Model Libraries.

## Generalizations

- [Usage](Usage.md)

## Specializations

- [ActionUsage](ActionUsage.md)
- [ConstraintUsage](ConstraintUsage.md)
- [EventOccurrenceUsage](EventOccurrenceUsage.md)
- [ItemUsage](ItemUsage.md)
- [PortUsage](PortUsage.md)

## Owned features

### individualDefinition : OccurrenceDefinition [0..1] {derived}

The at most one occurrenceDefinition that has isIndividual = true.

Subsets: `occurrenceDefinition`

### isIndividual : Boolean [1..1]

Whether this OccurrenceUsage represents the usage of the specific individual represented by its individualDefinition.

### occurrenceDefinition : Class [0..*] {derived, ordered}

The Classes that are the types of this OccurrenceUsage. Nominally, these are OccurrenceDefinitions, but other kinds of kernel Classes are also allowed, to permit use of Classes from the Kernel Model Libraries.

Redefines: `definition`

### portionKind : PortionKind [0..1]

The kind of temporal portion (time slice or snapshot) is represented by this OccurrenceUsage. If portionKind is not null, then the owningType of the OccurrenceUsage must be non-null, and the OccurrenceUsage represents portions of the featuring instance of the owningType.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| chainingFeature | Feature | [0..*] | [Feature](Feature.md) | derived, ordered |
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
| importedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
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
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
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
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| type | Type | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| usage | Usage | [0..*] | [Usage](Usage.md) | derived, ordered |
| variant | Usage | [0..*] | [Usage](Usage.md) | derived |
| variantMembership | VariantMembership | [0..*] | [Usage](Usage.md) | derived, composite |

## Constraints

### checkOccurrenceUsageSnapshotSpecialization

If an OccurrenceUsage has portionKind = snapshot, then it must directly or indirectly specialize Occurrences::Occurrence::snapshots from the Kernel Semantic Library.

```ocl
portionKind = PortionKind::snapshot implies
    specializesFromLibrary('Occurrences::Occurrence::snapshots')
```

### checkOccurrenceUsageSpecialization

An OccurrenceUsage must directly or indirectly specialize Occurrences::occurrences from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Occurrences::occurrences')
```

### checkOccurrenceUsageSuboccurrenceSpecialization

A composite OccurrenceUsage, whose ownedType is a Class, another OccurrenceUsage, or any kind of Feature typed by a Class, must directly or indirectly specialize Occurrences::Occurrence::suboccurrences.

```ocl
isComposite and
owningType <> null and
(owningType.oclIsKindOf(Class) or
 owningType.oclIsKindOf(OccurrenceUsage) or
 owningType.oclIsKindOf(Feature) and
    owningType.oclAsType(Feature).type->
        exists(oclIsKind(Class))) implies
    specializesFromLibrary('Occurrences::Occurrence::suboccurrences')
```

### checkOccurrenceUsageTimeSliceSpecialization

If an OccurrenceUsage has portionKind = timeslice, then it must directly or indirectly specialize Occurrences::Occurrence::timeSlices from the Kernel Semantic Library.

```ocl
portionKind = PortionKind::timeslice implies 
    specializesFromLibrary('Occurrences::Occurrence::timeSlices')
```

### deriveOccurrenceUsageIndividualDefinition

The individualDefinition of an OccurrenceUsage is the occurrenceDefinition that is an OccurrenceDefinition with isIndividual = true, if any.

```ocl
individualDefinition =
    let individualDefinitions : OrderedSet(OccurrenceDefinition) = 
        occurrenceDefinition->
            selectByKind(OccurrenceDefinition)->
            select(isIndividual) in
    if individualDefinitions->isEmpty() then null
    else individualDefinitions->first() endif
```

### validateOccurrenceUsageIndividualDefinition

An OccurrenceUsage must have at most one occurrenceDefinition with isIndividual = true.

```ocl
occurrenceDefinition->
    selectByKind(OccurrenceDefinition)->
    select(isIndividual).size() <= 1
```

### validateOccurrenceUsageIndividualUsage

If an OccurrenceUsage has isIndividual = true, then it must have an individualDefinition.

```ocl
isIndividual implies individualDefinition <> null
```

### validateOccurrenceUsageIsPortion

If an OccurrenceUsage has a non-null portionKind, then it must have isPortion = true.

```ocl
portionKind <> null implies isPortion
```

### validateOccurrenceUsagePortionKind

If an OccurrenceUsage has a non-null portionKind, then its owningType must be an OccurrenceDefinition or an OccurrenceUsage.

```ocl
portionKind <> null implies
    owningType <> null and
    (owningType.oclIsKindOf(OccurrenceDefinition) or
     owningType.oclIsKindOf(OccurrenceUsage))
```

