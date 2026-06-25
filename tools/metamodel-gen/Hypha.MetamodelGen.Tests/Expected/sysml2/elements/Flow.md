---
name: Flow
package: Interactions
isAbstract: false
generalizes: [Connector, Step]
specializedBy: [FlowUsage, SuccessionFlow]
---

# Flow

`Interactions` package · concrete metaclass

An Flow is a Step that represents the transfer of values from one Feature to another. Flows can take non-zero time to complete.

## Generalizations

- [Connector](Connector.md)
- [Step](Step.md)

## Specializations

- [FlowUsage](FlowUsage.md)
- [SuccessionFlow](SuccessionFlow.md)

## Owned features

### flowEnd : FlowEnd [0..2] {derived, ordered}

The connectorEnds of this Flow that are FlowEnds.

Subsets: `connectorEnd`

### interaction : Interaction [0..*] {derived, ordered}

The Interactions that type this Flow. Interactions are both Associations and Behaviors, which can type Connectors and Steps, respectively.

Redefines: `association`, `behavior`

### payloadFeature : PayloadFeature [0..1] {derived}

The ownedFeature of the Flow that is a PayloadFeature (if any).

Subsets: `ownedFeature`

### payloadType : Classifier [0..*] {derived, ordered}

The type of values transferred, which is the type of the payloadFeature of the Flow.

### sourceOutputFeature : Feature [0..1] {derived, ordered}

The Feature that provides the items carried by the Flow. It must be a feature of the source of the Flow.

### targetInputFeature : Feature [0..1] {derived, ordered}

The Feature that receives the values carried by the Flow. It must be a feature of the target of the Flow.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| association | Association | [0..*] | [Connector](Connector.md) | derived, ordered |
| behavior | Behavior | [0..*] | [Step](Step.md) | derived, ordered |
| chainingFeature | Feature | [0..*] | [Feature](Feature.md) | derived, ordered |
| connectorEnd | Feature | [0..*] | [Connector](Connector.md) | derived, ordered |
| crossFeature | Feature | [0..1] | [Feature](Feature.md) | derived |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| defaultFeaturingType | Type | [0..1] | [Connector](Connector.md) | derived |
| differencingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
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
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isOrdered | Boolean | [1..1] | [Feature](Feature.md) |  |
| isPortion | Boolean | [1..1] | [Feature](Feature.md) |  |
| isSufficient | Boolean | [1..1] | [Type](Type.md) |  |
| isUnique | Boolean | [1..1] | [Feature](Feature.md) |  |
| isVariable | Boolean | [1..1] | [Feature](Feature.md) |  |
| member | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | Multiplicity | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
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
| ownedRelatedElement | Element | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | Specialization | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubsetting | Subsetting | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedTypeFeaturing | TypeFeaturing | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedTyping | FeatureTyping | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedUnioning | Unioning | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningFeatureMembership | FeatureMembership | [0..1] | [Feature](Feature.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | Element | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| owningType | Type | [0..1] | [Feature](Feature.md) | derived |
| parameter | Feature | [0..*] | [Step](Step.md) | derived, ordered |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| relatedFeature | Feature | [0..*] | [Connector](Connector.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| sourceFeature | Feature | [0..1] | [Connector](Connector.md) | derived, ordered |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| targetFeature | Feature | [0..*] | [Connector](Connector.md) | derived, ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| type | Type | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkFlowSpecialization

A Flow must directly or indirectly specialize the Step Transfers::transfers from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Transfers::transfers')
```

### checkFlowWithEndsSpecialization

A Flow with ownedEndFeatures must specialize the Step Transfers::flowTransfers from the Kernel Semantic Library.

```ocl
ownedEndFeatures->notEmpty() implies
    specializesFromLibrary('Transfers::flowTransfers')
```

### deriveFlowFlowEnd

The flowEnds of a Flow are all its connectorEnds that are FlowEnds.

```ocl
flowEnd = connectorEnd->selectByKind(FlowEnd)
```

### deriveFlowPayloadFeature

The payloadFeature of a Flow is the single one of its ownedFeatures that is a PayloadFeature.

```ocl
payloadFeature =
    let payloadFeatures : Sequence(PayloadFeature) =
        ownedFeature->selectByKind(PayloadFeature) in
    if payloadFeatures->isEmpty() then null
    else payloadFeatures->first()
    endif
```

### deriveFlowPayloadType

The payloadTypes of a Flow are the types of the payloadFeature of the Flow (if any).

```ocl
payloadType =
    if payloadFeature = null then Sequence{}
    else payloadFeature.type
    endif
```

### deriveFlowSourceOutputFeature

The sourceOutputFeature of a Flow is the first ownedFeature of the first connectorEnd of the Flow.

```ocl
sourceOutputFeature =
    if connectorEnd->isEmpty() or 
        connectorEnd.ownedFeature->isEmpty()
    then null
    else connectorEnd.ownedFeature->first()
    endif
```

### deriveFlowTargetInputFeature

The targetInputFeature of a Flow is the first ownedFeature of the second connectorEnd of the Flow.

```ocl
targetInputFeature =
    if connectorEnd->size() < 2 or 
        connectorEnd->at(2).ownedFeature->isEmpty()
    then null
    else connectorEnd->at(2).ownedFeature->first()
    endif
```

### validateFlowPayloadFeature

A Flow must have at most one ownedFeature that is an PayloadFeature.

```ocl
ownedFeature->selectByKind(PayloadFeature)->size() <= 1
```

