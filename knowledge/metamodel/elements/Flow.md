---
name: Flow
package: Interactions
fully qualified name: KerML::Kernel::Interactions::Flow
isAbstract: false
visibility: public
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

### flowEnd

`+` [FlowEnd](FlowEnd.md) · `[0..2]` · *derived, ordered*

The connectorEnds of this Flow that are FlowEnds.

Subsets [connectorEnd](Connector.md#connectorend)

### interaction

`+` [Interaction](Interaction.md) · `[0..*]` · *derived, ordered*

The Interactions that type this Flow. Interactions are both Associations and Behaviors, which can type Connectors and Steps, respectively.

Redefines [association](Connector.md#association), [behavior](Step.md#behavior)

### payloadFeature

`+` [PayloadFeature](PayloadFeature.md) · `[0..1]` · *derived*

The ownedFeature of the Flow that is a PayloadFeature (if any).

Subsets [ownedFeature](Type.md#ownedfeature)

### payloadType

`+` [Classifier](Classifier.md) · `[0..*]` · *derived, ordered*

The type of values transferred, which is the type of the payloadFeature of the Flow.

### sourceOutputFeature

`+` [Feature](Feature.md) · `[0..1]` · *derived, ordered*

The Feature that provides the items carried by the Flow. It must be a feature of the source of the Flow.

### targetInputFeature

`+` [Feature](Feature.md) · `[0..1]` · *derived, ordered*

The Feature that receives the values carried by the Flow. It must be a feature of the target of the Flow.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| association | [Association](Association.md) | [0..*] | [Connector](Connector.md) | derived, ordered |
| behavior | [Behavior](Behavior.md) | [0..*] | [Step](Step.md) | derived, ordered |
| chainingFeature | [Feature](Feature.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| connectorEnd | [Feature](Feature.md) | [0..*] | [Connector](Connector.md) | derived, ordered |
| crossFeature | [Feature](Feature.md) | [0..1] | [Feature](Feature.md) | derived |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| defaultFeaturingType | [Type](Type.md) | [0..1] | [Connector](Connector.md) | derived |
| differencingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
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
| isImplied | [Boolean](Boolean.md) | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| isOrdered | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| isPortion | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| isSufficient | [Boolean](Boolean.md) | [1..1] | [Type](Type.md) |  |
| isUnique | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| isVariable | [Boolean](Boolean.md) | [1..1] | [Feature](Feature.md) |  |
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | [Multiplicity](Multiplicity.md) | [0..1] | [Type](Type.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
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
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | [Specialization](Specialization.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubsetting | [Subsetting](Subsetting.md) | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedTypeFeaturing | [TypeFeaturing](TypeFeaturing.md) | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedTyping | [FeatureTyping](FeatureTyping.md) | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedUnioning | [Unioning](Unioning.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..1] | [Feature](Feature.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| parameter | [Feature](Feature.md) | [0..*] | [Step](Step.md) | derived, ordered |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| relatedFeature | [Feature](Feature.md) | [0..*] | [Connector](Connector.md) | derived, ordered |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| sourceFeature | [Feature](Feature.md) | [0..1] | [Connector](Connector.md) | derived, ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| targetFeature | [Feature](Feature.md) | [0..*] | [Connector](Connector.md) | derived, ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| type | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |

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

