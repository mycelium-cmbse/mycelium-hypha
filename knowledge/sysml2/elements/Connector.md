---
name: Connector
package: Connectors
fully qualified name: KerML::Kernel::Connectors::Connector
isAbstract: false
visibility: public
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

### association

`+` [Association](Association.md) · `[0..*]` · *derived, ordered*

The Associations that type the Connector.

Redefines [type](Feature.md#type)

### connectorEnd

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

The endFeatures of a Connector, which redefine the endFeatures of the associations of the Connector. The connectorEnds determine via ReferenceSubsetting Relationships which Features are related by the Connector.

Redefines [endFeature](Type.md#endfeature)

### defaultFeaturingType

`+` [Type](Type.md) · `[0..1]` · *derived*

The innermost Type that is a common direct or indirect featuringType of the relatedFeatures, such that, if it exists and was the featuringType of this Connector, the Connector would satisfy the checkConnectorTypeFeaturing constraint.

### relatedFeature

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

The Features that are related by this Connector considered as a Relationship and that restrict the links it identifies, given by the referenced Features of the connectorEnds of the Connector.

Redefines [relatedElement](Relationship.md#relatedelement)

### sourceFeature

`+` [Feature](Feature.md) · `[0..1]` · *derived, ordered*

The source relatedFeature for this Connector. It is the first relatedFeature.

Redefines [source](Relationship.md#source)

Subsets [relatedFeature](#relatedfeature)

### targetFeature

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

The target relatedFeatures for this Connector. This includes all the relatedFeatures other than the sourceFeature.

Redefines [target](Relationship.md#target)

Subsets [relatedFeature](#relatedfeature)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| chainingFeature | [Feature](Feature.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| crossFeature | [Feature](Feature.md) | [0..1] | [Feature](Feature.md) | derived |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| differencingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| direction | FeatureDirectionKind | [0..1] | [Feature](Feature.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
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
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | [Multiplicity](Multiplicity.md) | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
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
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| type | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |

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

