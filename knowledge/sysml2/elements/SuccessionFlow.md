---
name: SuccessionFlow
package: Interactions
isAbstract: false
generalizes: [Flow, Succession]
specializedBy: [SuccessionFlowUsage]
---

# SuccessionFlow

`Interactions` package · concrete metaclass

A SuccessionFlow is a Flow that also provides temporal ordering. It classifies Transfers that cannot start until the source Occurrence has completed and that must complete before the target Occurrence can start.

## Generalizations

- [Flow](Flow.md)
- [Succession](Succession.md)

## Specializations

- [SuccessionFlowUsage](SuccessionFlowUsage.md)

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
| flowEnd | FlowEnd | [0..2] | [Flow](Flow.md) | derived, ordered |
| importedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | Membership | [0..*] | [Type](Type.md) | derived, ordered |
| input | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| interaction | Interaction | [0..*] | [Flow](Flow.md) | derived, ordered |
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
| payloadFeature | PayloadFeature | [0..1] | [Flow](Flow.md) | derived |
| payloadType | Classifier | [0..*] | [Flow](Flow.md) | derived, ordered |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| relatedFeature | Feature | [0..*] | [Connector](Connector.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| sourceFeature | Feature | [0..1] | [Connector](Connector.md) | derived, ordered |
| sourceOutputFeature | Feature | [0..1] | [Flow](Flow.md) | derived, ordered |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| targetFeature | Feature | [0..*] | [Connector](Connector.md) | derived, ordered |
| targetInputFeature | Feature | [0..1] | [Flow](Flow.md) | derived, ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| type | Type | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkSuccessionFlowSpecialization

A SuccessionFlow must directly or indirectly specialize the Step Transfers::flowTransfersBefore from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Transfers::flowTransfersBefore')
```

