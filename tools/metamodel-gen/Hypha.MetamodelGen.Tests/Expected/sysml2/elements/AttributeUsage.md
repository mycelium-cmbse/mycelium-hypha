---
name: AttributeUsage
package: Attributes
isAbstract: false
generalizes: [Usage]
specializedBy: [EnumerationUsage]
---

# AttributeUsage

`Attributes` package · concrete metaclass

An AttributeUsage is a Usage whose type is a DataType. Nominally, if the type is an AttributeDefinition, an AttributeUsage is a usage of a AttributeDefinition to represent the value of some system quality or characteristic. However, other kinds of kernel DataTypes are also allowed, to permit use of DataTypes from the Kernel Model Libraries. An AttributeUsage itself as well as all its nested features must be referential (non-composite).An AttributeUsage must specialize, directly or indirectly, the base Feature Base::dataValues from the Kernel Semantic Library.

## Generalizations

- [Usage](Usage.md)

## Specializations

- [EnumerationUsage](EnumerationUsage.md)

## Owned features

### attributeDefinition : «untyped» [0..*] {derived, ordered}

The DataTypes that are the types of this AttributeUsage. Nominally, these are AttributeDefinitions, but other kinds of kernel DataTypes are also allowed, to permit use of DataTypes from the Kernel Model Libraries.

Redefines: `definition`

### isReference : Boolean [1..1] {derived}

Always true for an AttributeUsage.

Redefines: `isReference`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| definition | «untyped» | [0..*] | [Usage](Usage.md) | derived, ordered |
| directedUsage | Usage | [0..*] | [Usage](Usage.md) | derived, ordered |
| isVariation | Boolean | [1..1] | [Usage](Usage.md) |  |
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
| owningDefinition | Definition | [0..1] | [Usage](Usage.md) | derived |
| owningUsage | Usage | [0..1] | [Usage](Usage.md) | derived |
| usage | Usage | [0..*] | [Usage](Usage.md) | derived, ordered |
| variant | Usage | [0..*] | [Usage](Usage.md) | derived |
| variantMembership | VariantMembership | [0..*] | [Usage](Usage.md) | derived, composite |

## Constraints

### checkAttributeUsageSpecialization

An AttributeUsage must directly or indirectly specialize Base::dataValues from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Base::dataValues')
```

### validateAttributeUsageFeatures

All features of an AttributeUsage must be non-composite.

```ocl
feature->forAll(not isComposite)
```

### validateAttributeUsageIsReference

An AttributeUsage is always referential.

```ocl
isReference
```

