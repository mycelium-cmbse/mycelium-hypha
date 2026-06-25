---
name: Connector
package: Connectors
isAbstract: false
generalizes: [Feature, Relationship]
specializedBy: [BindingConnector, ConnectorAsUsage, Flow, Succession]
---

# Connector

`Connectors` package · concrete metaclass

A Connector is a usage of Associations, with links restricted according to instances of the Type in which they are used (domain of the Connector). The associations of the Connector restrict what kinds of things might be linked. The Connector further restricts these links to be between values of Features on instances of its domain.

## Generalizations

- [Feature](Feature.md)
- [Relationship](Relationship.md)

## Specializations

- [BindingConnector](BindingConnector.md)
- [ConnectorAsUsage](ConnectorAsUsage.md)
- [Flow](Flow.md)
- [Succession](Succession.md)

## Owned features

### association : Association [0..*] {derived, ordered}

The Associations that type the Connector.

Redefines: `type`

### connectorEnd : Feature [0..*] {derived, ordered}

The endFeatures of a Connector, which redefine the endFeatures of the associations of the Connector. The connectorEnds determine via ReferenceSubsetting Relationships which Features are related by the Connector.

Redefines: `endFeature`

### defaultFeaturingType : Type [0..1] {derived}

The innermost Type that is a common direct or indirect featuringType of the relatedFeatures, such that, if it exists and was the featuringType of this Connector, the Connector would satisfy the checkConnectorTypeFeaturing constraint.

### relatedFeature : Feature [0..*] {derived, ordered}

The Features that are related by this Connector considered as a Relationship and that restrict the links it identifies, given by the referenced Features of the connectorEnds of the Connector.

Redefines: `relatedElement`

### sourceFeature : Feature [0..1] {derived, ordered}

The source relatedFeature for this Connector. It is the first relatedFeature.

Redefines: `source`

Subsets: `relatedFeature`

### targetFeature : Feature [0..*] {derived, ordered}

The target relatedFeatures for this Connector. This includes all the relatedFeatures other than the sourceFeature.

Redefines: `target`

Subsets: `relatedFeature`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| chainingFeature | Feature | [0..*] | [Feature](Feature.md) | derived, ordered |
| crossFeature | Feature | [0..1] | [Feature](Feature.md) | derived |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
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
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| type | Type | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkConnectorBinaryObjectSpecialization

A binary Connector for an AssociationStructure must directly or indirectly specialize the base Connector Objects::binaryLinkObjects from the Kernel Semantic Library.

```ocl
connectorEnds->size() = 2 and
association->exists(oclIsKindOf(AssociationStructure)) implies
    specializesFromLibrary('Objects::binaryLinkObjects')
```

### checkConnectorBinarySpecialization

A binary Connector must directly or indirectly specialize the base Connector Links::binaryLinks from the Kernel Semantic Library.

```ocl
connectorEnd->size() = 2 implies
    specializesFromLibrary('Links::binaryLinks')
```

### checkConnectorObjectSpecialization

A Connector for an AssociationStructure must directly or indirectly specialize the base Connector Objects::linkObjects from the Kernel Semantic Library.

```ocl
association->exists(oclIsKindOf(AssociationStructure)) implies
    specializesFromLibrary('Objects::linkObjects')
```

### checkConnectorSpecialization

A Connector must directly or indirectly specialize the base Connector Links::links from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Links::links')
```

### checkConnectorTypeFeaturing

Each relatedFeature of a Connector must have each featuringType of the Connector as a direct or indirect featuringType (where a Feature with no featuringType is treated as if the Classifier Base::Anything was its featuringType).

```ocl
relatedFeature->forAll(f | 
    if featuringType->isEmpty() then f.isFeaturedWithin(null)
    else featuringType->forAll(t | f.isFeaturedWithin(t))
    endif)
```

### deriveConnectorDefaultFeaturingType

The defaultFeaturingType of a Connector is the innermost common direct or indirect featuringType of the relatedFeatures of the Connector, so that each relatedElement is featured within the defaultFeaturingType, if such exists.

```ocl
let commonFeaturingTypes : OrderedSet(Type) = 
    relatedFeature->closure(featuringType)->select(t | 
        relatedFeature->forAll(f | f.isFeaturedWithin(t))
    ) in
let nearestCommonFeaturingTypes : OrderedSet(Type) =
    commonFeaturingTypes->reject(t1 | 
        commonFeaturingTypes->exists(t2 | 
            t2 <> t1 and t2->closure(featuringType)->contains(t1)
    )) in
if nearestCommonFeaturingTypes->isEmpty() then null
else nearestCommonFeaturingTypes->first()
endif
```

### deriveConnectorRelatedFeature

The relatedFeatures of a Connector are the referenced Features of its connectorEnds.

```ocl
relatedFeature = connectorEnd.ownedReferenceSubsetting->
    select(s | s <> null).subsettedFeature
```

### deriveConnectorSourceFeature

The sourceFeature of a Connector is its first relatedFeature (if any).

```ocl
sourceFeature = 
    if relatedFeature->isEmpty() then null 
    else relatedFeature->first() 
    endif
```

### deriveConnectorTargetFeature

The targetFeatures of a Connector are the relatedFeatures other than the sourceFeature.

```ocl
targetFeature =
    if relatedFeature->size() < 2 then OrderedSet{}
    else 
        relatedFeature->
            subSequence(2, relatedFeature->size())->
            asOrderedSet()
    endif
```

### validateConnectorBinarySpecialization

If a Connector has more than two connectorEnds, then it must not specialize, directly or indirectly, the Association BinaryLink from the Kernel Semantic Library.

```ocl
connectorEnds->size() > 2 implies
    not specializesFromLibrary('Links::BinaryLink')
```

### validateConnectorRelatedFeatures

If a Connector is concrete (not abstract), then it must have at least two relatedFeatures.

```ocl
not isAbstract implies relatedFeature->size() >= 2
```

