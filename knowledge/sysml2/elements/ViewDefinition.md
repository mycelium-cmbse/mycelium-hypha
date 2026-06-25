---
name: ViewDefinition
package: Views
isAbstract: false
generalizes: [PartDefinition]
specializedBy: []
---

# ViewDefinition

`Views` package · concrete metaclass

A ViewDefinition is a PartDefinition that specifies how a view artifact is constructed to satisfy a viewpoint. It specifies a viewConditions to define the model content to be presented and a viewRendering to define how the model content is presented.

## Generalizations

- [PartDefinition](PartDefinition.md)

## Owned features

### satisfiedViewpoint : ViewpointUsage [0..*] {derived, ordered}

The composite ownedRequirements of this ViewDefinition that are ViewpointUsages for viewpoints satisfied by the ViewDefinition.

Subsets: `ownedRequirement`

### view : ViewUsage [0..*] {derived, ordered}

The usages of this ViewDefinition that are ViewUsages.

Subsets: `usage`

### viewCondition : Expression [0..*] {derived, ordered}

The Expressions related to this ViewDefinition by ElementFilterMemberships, which specify conditions on Elements to be rendered in a view.

Subsets: `ownedMember`

### viewRendering : RenderingUsage [0..1] {derived}

The RenderingUsage to be used to render views defined by this ViewDefinition, which is the referencedRendering of the ViewRenderingMembership of the ViewDefinition.


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
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| usage | Usage | [0..*] | [Definition](Definition.md) | derived, ordered |
| variant | Usage | [0..*] | [Definition](Definition.md) | derived |
| variantMembership | VariantMembership | [0..*] | [Definition](Definition.md) | derived, composite |

## Constraints

### checkViewDefinitionSpecialization

A ViewDefinition must directly or indirectly specialize the base ViewDefinition Views::View from the Systems Model Library.

```ocl
specializesFromLibrary('Views::View')
```

### deriveViewDefinitionSatisfiedViewpoint

The satisfiedViewpoints of a ViewDefinition are its ownedRequirements that are composite ViewpointUsages.

```ocl
satisfiedViewpoint = ownedRequirement->
    selectByKind(ViewpointUsage)->
    select(isComposite)
```

### deriveViewDefinitionView

The views of a ViewDefinition are all its usages that are ViewUsages.

```ocl
view = usage->selectByKind(ViewUsage)
```

### deriveViewDefinitionViewCondition

The viewConditions of a ViewDefinition are the conditions of its owned ElementFilterMemberships.

```ocl
viewCondition = ownedMembership->
    selectByKind(ElementFilterMembership).
    condition
```

### deriveViewDefinitionViewRendering

The viewRendering of a ViewDefinition is the referencedRendering of its owned ViewRenderingMembership, if any.

```ocl
viewRendering =
    let renderings: OrderedSet(ViewRenderingMembership) =
        featureMembership->selectByKind(ViewRenderingMembership) in
    if renderings->isEmpty() then null
    else renderings->first().referencedRendering
    endif
```

### validateViewDefinitionOnlyOneViewRendering

A ViewDefinition must have at most one ViewRenderingMembership.

```ocl
featureMembership->
    selectByKind(ViewRenderingMembership)->
    size() <= 1
```

