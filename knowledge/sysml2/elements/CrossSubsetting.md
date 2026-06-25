---
name: CrossSubsetting
package: Features
fully qualified name: KerML::Core::Features::CrossSubsetting
isAbstract: false
visibility: public
generalizes: [Subsetting]
specializedBy: []
---

# CrossSubsetting

`Features` package · concrete metaclass

CrossSubsetting is a kind of Subsetting for end Features, as identified by crossingFeature, to subset a chained Feature, identified by crossedFeature. It navigates to instances of the end Feature’s type from instances of other end Feature types on the same owningType (at least two end Features are required for any of them to have a CrossSubsetting).The crossedFeature of a CrossSubsetting must have a feature chain of exactly two Features. The second Feature in the chain is the crossFeature of the crossingFeature (end Feature), which has the same type as the crossingFeature. When the owningType of the crossingFeature has exactly two end Features, the first Feature in the chain of the crossedFeature is the other end Feature. The crossFeature’s featuringType in this case is the other end Feature. When the owningType has more than two end Features, the first Feature in the chain is a Feature that CrossMultiplies all the other end Features, which is also the featuringType of the crossFeature.A crossFeature must be owned by its featureCrossing (end Feature) when the featureCrossing owningType has more than two end Features. Otherwise, for exactly two end Features, the crossFeatures of each the ends can instead optionally be inherited by the other end from one of its types or a subsetted Feature.

## Generalizations

- [Subsetting](Subsetting.md)

## Owned features

### crossedFeature

`+` [Feature](Feature.md) · `[1..1]`

The chained Feature that is cross subset by the crossingFeature of this CrossSubsetting.

Redefines [subsettedFeature](Subsetting.md#subsettedfeature)

### crossingFeature

`+` [Feature](Feature.md) · `[1..1]` · *derived*

The end Feature that owns this CrossSubsetting relationship and is also its subsettingFeature.

Redefines [owningFeature](Subsetting.md#owningfeature), [subsettingFeature](Subsetting.md#subsettingfeature)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| general | [Type](Type.md) | [1..1] | [Specialization](Specialization.md) |  |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningFeature | [Feature](Feature.md) | [0..1] | [Subsetting](Subsetting.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Specialization](Specialization.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| specific | [Type](Type.md) | [1..1] | [Specialization](Specialization.md) |  |
| subsettedFeature | [Feature](Feature.md) | [1..1] | [Subsetting](Subsetting.md) |  |
| subsettingFeature | [Feature](Feature.md) | [1..1] | [Subsetting](Subsetting.md) |  |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |

## Constraints

### validateCrossSubsettingCrossedFeature

The crossedFeature of a CrossSubsetting must have exactly two chainingFeatures. If the crossingFeature of the CrossSubsetting is one of two end Features, then the first chainingFeature must be the other end Feature.

```ocl
crossingFeature.isEnd and crossingFeature.owningType <> null implies
    let endFeatures: Sequence(Feature) = crossingFeature.owningType.endFeature in
    let chainingFeatures: Sequence(Feature) = crossedFeature.chainingFeature in
    chainingFeatures->size() = 2 and
    endFeatures->size() = 2 implies 
        chainingFeatures->at(1) = endFeatures->excluding(crossingFeature)->at(1)
```

### validateCrossSubsettingCrossingFeature

The crossingFeature of a CrossSubsetting must be an end Feature that is owned by a Type with at least two end Features.

```ocl
crossingFeature.isEnd and
crossingFeature.owningType<>null and
crossingFeature.owningType.endFeature ->size() > 1
```

