---
name: AttributeUsage
package: Attributes
fully qualified name: SysML::Systems::Attributes::AttributeUsage
isAbstract: false
visibility: public
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

### attributeDefinition

`+` [DataType](DataType.md) · `[0..*]` · *derived, ordered*

The DataTypes that are the types of this AttributeUsage. Nominally, these are AttributeDefinitions, but other kinds of kernel DataTypes are also allowed, to permit use of DataTypes from the Kernel Model Libraries.

Redefines [definition](Usage.md#definition)

### isReference

`+` [Boolean](Boolean.md) · `[1..1]` · *derived*

Always true for an AttributeUsage.

Redefines [isReference](#isreference)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| chainingFeature | [Feature](Feature.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| crossFeature | [Feature](Feature.md) | [0..1] | [Feature](Feature.md) | derived |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| definition | [Classifier](Classifier.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| differencingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedUsage | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
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
| isVariation | [Boolean](Boolean.md) | [1..1] | [Usage](Usage.md) |  |
| mayTimeVary | [Boolean](Boolean.md) | [1..1] | [Usage](Usage.md) | derived |
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | [Multiplicity](Multiplicity.md) | [0..1] | [Type](Type.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| nestedAction | [ActionUsage](ActionUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAllocation | [AllocationUsage](AllocationUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAnalysisCase | [AnalysisCaseUsage](AnalysisCaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAttribute | [AttributeUsage](AttributeUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedCalculation | [CalculationUsage](CalculationUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedCase | [CaseUsage](CaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedConcern | [ConcernUsage](ConcernUsage.md) | [0..*] | [Usage](Usage.md) | derived |
| nestedConnection | [ConnectorAsUsage](ConnectorAsUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedConstraint | [ConstraintUsage](ConstraintUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedEnumeration | [EnumerationUsage](EnumerationUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedFlow | [FlowUsage](FlowUsage.md) | [0..*] | [Usage](Usage.md) | derived |
| nestedInterface | [InterfaceUsage](InterfaceUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedItem | [ItemUsage](ItemUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedMetadata | [MetadataUsage](MetadataUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedOccurrence | [OccurrenceUsage](OccurrenceUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedPart | [PartUsage](PartUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedPort | [PortUsage](PortUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedReference | [ReferenceUsage](ReferenceUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedRendering | [RenderingUsage](RenderingUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedRequirement | [RequirementUsage](RequirementUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedState | [StateUsage](StateUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedTransition | [TransitionUsage](TransitionUsage.md) | [0..*] | [Usage](Usage.md) | derived |
| nestedUsage | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedUseCase | [UseCaseUsage](UseCaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedVerificationCase | [VerificationCaseUsage](VerificationCaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedView | [ViewUsage](ViewUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedViewpoint | [ViewpointUsage](ViewpointUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
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
| owningDefinition | [Definition](Definition.md) | [0..1] | [Usage](Usage.md) | derived |
| owningFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..1] | [Feature](Feature.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| owningUsage | [Usage](Usage.md) | [0..1] | [Usage](Usage.md) | derived |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| type | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| usage | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| variant | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived |
| variantMembership | [VariantMembership](VariantMembership.md) | [0..*] | [Usage](Usage.md) | derived, composite |

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

