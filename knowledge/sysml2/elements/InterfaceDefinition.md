---
name: InterfaceDefinition
package: Interfaces
isAbstract: false
generalizes: [ConnectionDefinition]
specializedBy: []
---

# InterfaceDefinition

`Interfaces` package · concrete metaclass

An InterfaceDefinition is a ConnectionDefinition all of whose ends are PortUsages, defining an interface between elements that interact through such ports.

## Generalizations

- [ConnectionDefinition](ConnectionDefinition.md)

## Owned features

### interfaceEnd : PortUsage [0..*] {derived, ordered}

The PortUsages that are the connectionEnds of this InterfaceDefinition.

Redefines: `connectionEnd`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| associationEnd | Feature | [0..*] | [Association](Association.md) | derived |
| connectionEnd | Usage | [0..*] | [ConnectionDefinition](ConnectionDefinition.md) | derived, ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| differencingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| directedUsage | Usage | [0..*] | [Definition](Definition.md) | derived, ordered |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| endFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| feature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, ordered |
| importedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | Membership | [0..*] | [Type](Type.md) | derived, ordered |
| input | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | Boolean | [1..1] | [Type](Type.md) |  |
| isConjugated | Boolean | [1..1] | [Type](Type.md) | derived |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isIndividual | Boolean | [1..1] | [OccurrenceDefinition](OccurrenceDefinition.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isSufficient | Boolean | [1..1] | [ConnectionDefinition](ConnectionDefinition.md) |  |
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
| ownedRelatedElement | Element | [0..*] | [Relationship](Relationship.md) | composite, ordered |
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
| owningRelatedElement | Element | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| relatedType | Type | [0..*] | [Association](Association.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| sourceType | Type | [0..1] | [Association](Association.md) | derived |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| targetType | Type | [0..*] | [Association](Association.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| usage | Usage | [0..*] | [Definition](Definition.md) | derived, ordered |
| variant | Usage | [0..*] | [Definition](Definition.md) | derived |
| variantMembership | VariantMembership | [0..*] | [Definition](Definition.md) | derived, composite |

## Constraints

### checkInterfaceDefinitionBinarySpecialization

A binary InterfaceDefinition must directly or indirectly specialize the InterfaceDefinition Interfaces::BinaryInterface from the Systems Model Library.

```ocl
ownedEndFeature->size() = 2 implies
    specializesFromLibrary('Interfaces::BinaryInterface')
```

### checkInterfaceDefinitionSpecialization

An InterfaceDefinition must directly or indirectly specialize the InterfaceDefinition Interfaces::Interface from the Systems Model Library.

```ocl
specializesFromLibrary('Interfaces::Interface')
```

