---
name: PartUsage
package: Parts
isAbstract: false
generalizes: [ItemUsage]
specializedBy: [ConnectionUsage, RenderingUsage, ViewUsage]
---

# PartUsage

`Parts` package · concrete metaclass

A PartUsage is a usage of a PartDefinition to represent a system or a part of a system. At least one of the itemDefinitions of the PartUsage must be a PartDefinition.A PartUsage must subset, directly or indirectly, the base PartUsage parts from the Systems Model Library.

## Generalizations

- [ItemUsage](ItemUsage.md)

## Specializations

- [ConnectionUsage](ConnectionUsage.md)
- [RenderingUsage](RenderingUsage.md)
- [ViewUsage](ViewUsage.md)

## Owned features

### partDefinition : PartDefinition [0..*] {derived, ordered}

The itemDefinitions of this PartUsage that are PartDefinitions.

Subsets: `itemDefinition`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| definition | «untyped» | [0..*] | [Usage](Usage.md) | derived, ordered |
| directedUsage | Usage | [0..*] | [Usage](Usage.md) | derived, ordered |
| individualDefinition | OccurrenceDefinition | [0..1] | [OccurrenceUsage](OccurrenceUsage.md) | derived |
| isIndividual | Boolean | [1..1] | [OccurrenceUsage](OccurrenceUsage.md) |  |
| isReference | Boolean | [1..1] | [Usage](Usage.md) | derived |
| isVariation | Boolean | [1..1] | [Usage](Usage.md) |  |
| itemDefinition | «untyped» | [0..*] | [ItemUsage](ItemUsage.md) | derived, ordered |
| mayTimeVary | Boolean | [1..1] | [Usage](Usage.md) | derived |
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
| occurrenceDefinition | «untyped» | [0..*] | [OccurrenceUsage](OccurrenceUsage.md) | derived, ordered |
| owningDefinition | Definition | [0..1] | [Usage](Usage.md) | derived |
| owningUsage | Usage | [0..1] | [Usage](Usage.md) | derived |
| portionKind | PortionKind | [0..1] | [OccurrenceUsage](OccurrenceUsage.md) |  |
| usage | Usage | [0..*] | [Usage](Usage.md) | derived, ordered |
| variant | Usage | [0..*] | [Usage](Usage.md) | derived |
| variantMembership | VariantMembership | [0..*] | [Usage](Usage.md) | derived, composite |

## Constraints

### checkPartUsageActorSpecialization

If a PartUsage is owned via an ActorMembership, then it must directly or indirectly specialize either Requirements::RequirementCheck::actors (if its owningType is a RequirementDefinition or RequirementUsage or Cases::Case::actors (otherwise).

```ocl
owningFeatureMembership <> null and
owningFeatureMembership.oclIsKindOf(ActorMembership) implies
    if owningType.oclIsKindOf(RequirementDefinition) or 
       owningType.oclIsKindOf(RequirementUsage)
    then specializesFromLibrary('Requirements::RequirementCheck::actors')
    else specializesFromLibrary('Cases::Case::actors')
```

### checkPartUsageSpecialization

A PartUsage must directly or indirectly specialize the PartUsage Parts::parts from the Systems Model Library.

```ocl
specializesFromLibrary('Parts::parts')
```

### checkPartUsageStakeholderSpecialization

If a PartUsage is owned via a StakeholderMembership, then it must directly or indirectly specialize Requirements::RequirementCheck::stakeholders.

```ocl
owningFeatureMembership <> null and
owningFeatureMembership.oclIsKindOf(StakeholderMembership) implies
    specializesFromLibrary('Requirements::RequirementCheck::stakeholders')
```

### checkPartUsageSubpartSpecialization

A composite PartUsage whose owningType is a ItemDefinition or ItemUsage must directly or indirectly specialize the PartUsage Items::Item::subparts from the Systems Model Library.

```ocl
isComposite and owningType <> null and
(owningType.oclIsKindOf(ItemDefinition) or
 owningType.oclIsKindOf(ItemUsage)) implies
    specializesFromLibrary('Items::Item::subparts')
```

### derivePartUsagePartDefinition

The partDefinitions of an PartUsage are those itemDefinitions that are PartDefinitions.

```ocl
itemDefinition->selectByKind(PartDefinition)
```

### validatePartUsagePartDefinition

At least one of the itemDefinitions of a PartUsage must be a PartDefinition.

```ocl
partDefinition->notEmpty()
```

