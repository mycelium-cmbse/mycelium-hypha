---
name: Type
package: Types
fully qualified name: KerML::Core::Types::Type
isAbstract: false
visibility: public
generalizes: [Namespace]
specializedBy: [Classifier, Feature]
---

# Type

`Types` package · concrete metaclass

A Type is a Namespace that is the most general kind of Element supporting the semantics of classification. A Type may be a Classifier or a Feature, defining conditions on what is classified by the Type (see also the description of isSufficient).

## Generalizations

- [Namespace](Namespace.md)

## Specializations

- [Classifier](Classifier.md)
- [Feature](Feature.md)

## Owned features

### differencingType

`+` [Type](Type.md) · `[0..*]` · *derived, ordered*

The interpretations of a Type with differencingTypes are asserted to be those of the first of those Types, but not including those of the remaining Types. For example, a Classifier might be the difference of a Classifier for people and another for people of a particular nationality, leaving people who are not of that nationality. Similarly, a feature of people might be the difference between a feature for their children and a Classifier for people of a particular sex, identifying their children not of that sex (because the interpretations of the children Feature that identify those of that sex are also interpretations of the Classifier for that sex).

### directedFeature

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

The features of this Type that have a non-null direction.

Subsets [feature](#feature)

### endFeature

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

All features of this Type with isEnd = true.

Subsets [feature](#feature)

### feature

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

The ownedMemberFeatures of the featureMemberships of this Type.

Subsets [member](Namespace.md#member)

### featureMembership

`+` [FeatureMembership](FeatureMembership.md) · `[0..*]` · *derived, ordered*

The FeatureMemberships for features of this Type, which include all ownedFeatureMemberships and those inheritedMemberships that are FeatureMemberships (but does not include any importedMemberships).

### inheritedFeature

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

All the memberFeatures of the inheritedMemberships of this Type that are FeatureMemberships.

Subsets [feature](#feature)

### inheritedMembership

`+` [Membership](Membership.md) · `[0..*]` · *derived, ordered*

All Memberships inherited by this Type via Specialization or Conjugation. These are included in the derived union for the memberships of the Type.

Subsets [membership](Namespace.md#membership)

### input

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

All features related to this Type by FeatureMemberships that have direction in or inout.

Subsets [directedFeature](#directedfeature)

### intersectingType

`+` [Type](Type.md) · `[0..*]` · *derived, ordered*

The interpretations of a Type with intersectingTypes are asserted to be those in common among the intersectingTypes, which are the Types derived from the intersectingType of the ownedIntersectings of this Type. For example, a Classifier might be an intersection of Classifiers for people of a particular sex and of a particular nationality. Similarly, a feature for people&#39;s children of a particular sex might be the intersection of a Feature for their children and a Classifier for people of that sex (because the interpretations of the children Feature that identify those of that sex are also interpretations of the Classifier for that sex).

### isAbstract

`+` Boolean · `[1..1]`

Indicates whether instances of this Type must also be instances of at least one of its specialized Types.

### isConjugated

`+` Boolean · `[1..1]` · *derived*

Indicates whether this Type has an ownedConjugator.

### isSufficient

`+` Boolean · `[1..1]`

Whether all things that meet the classification conditions of this Type must be classified by the Type.(A Type&nbsp;gives conditions that must be met by whatever it classifies, but when isSufficient is false, things may meet those conditions but still not be classified by the Type. For example, a Type Car that is not sufficient could require everything it classifies to have four wheels, but not all four wheeled things would classify as cars. However, if the Type Car were sufficient, it would classify all four-wheeled things.)

### multiplicity

`+` [Multiplicity](Multiplicity.md) · `[0..1]` · *derived*

An ownedMember of this Type that is a Multiplicity, which constraints the cardinality of the Type. If there is no such ownedMember, then the cardinality of this Type is constrained by all the Multiplicity constraints applicable to any direct supertypes.

Subsets [ownedMember](Namespace.md#ownedmember)

### output

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

All features related to this Type by FeatureMemberships that have direction out or inout.

Subsets [directedFeature](#directedfeature)

### ownedConjugator

`+` [Conjugation](Conjugation.md) · `[0..1]` · *derived, composite*

A Conjugation owned by this Type for which the Type is the originalType.

Subsets `conjugator`, [ownedRelationship](Element.md#ownedrelationship)

### ownedDifferencing

`+` [Differencing](Differencing.md) · `[0..*]` · *derived, composite, ordered*

The ownedRelationships of this Type that are Differencings, having this Type as their typeDifferenced.

Subsets `sourceRelationship`, [ownedRelationship](Element.md#ownedrelationship)

### ownedDisjoining

`+` [Disjoining](Disjoining.md) · `[0..*]` · *derived, composite*

The ownedRelationships of this Type that are Disjoinings, for which the Type is the typeDisjoined Type.

Subsets [ownedRelationship](Element.md#ownedrelationship), `disjoiningTypeDisjoining`

### ownedEndFeature

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

All endFeatures of this Type that are ownedFeatures.

Subsets [endFeature](#endfeature), [ownedFeature](#ownedfeature)

### ownedFeature

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

The ownedMemberFeatures of the ownedFeatureMemberships of this Type.

Subsets [ownedMember](Namespace.md#ownedmember)

### ownedFeatureMembership

`+` [FeatureMembership](FeatureMembership.md) · `[0..*]` · *derived, composite, ordered*

The ownedMemberships of this Type that are FeatureMemberships, for which the Type is the owningType. Each such FeatureMembership identifies an ownedFeature of the Type.

Subsets [ownedMembership](Namespace.md#ownedmembership), [featureMembership](#featuremembership)

### ownedIntersecting

`+` [Intersecting](Intersecting.md) · `[0..*]` · *derived, composite, ordered*

The ownedRelationships of this Type that are Intersectings, have the Type as their typeIntersected.

Subsets `sourceRelationship`, [ownedRelationship](Element.md#ownedrelationship)

### ownedSpecialization

`+` [Specialization](Specialization.md) · `[0..*]` · *derived, composite, ordered*

The ownedRelationships of this Type that are Specializations, for which the Type is the specific Type.

Subsets [ownedRelationship](Element.md#ownedrelationship), `specialization`

### ownedUnioning

`+` [Unioning](Unioning.md) · `[0..*]` · *derived, composite, ordered*

The ownedRelationships of this Type that are Unionings, having the Type as their typeUnioned.

Subsets [ownedRelationship](Element.md#ownedrelationship), `sourceRelationship`

### unioningType

`+` [Type](Type.md) · `[0..*]` · *derived, ordered*

The interpretations of a Type with unioningTypes are asserted to be the same as those of all the unioningTypes together, which are the Types derived from the unioningType of the ownedUnionings of this Type. For example, a Classifier for people might be the union of Classifiers for all the sexes. Similarly, a feature for people&#39;s children might be the union of features dividing them in the same ways as people in general.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| importedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedImport | [Import](Import.md) | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedMember | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |

## Constraints

### checkTypeSpecialization

A Type must directly or indirectly specialize Base::Anything from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Base::Anything')
```

### deriveTypeDifferencingType

The differencingTypes of a Type are the differencingTypes of its ownedDifferencings, in the same order.

```ocl
differencingType = ownedDifferencing.differencingType
```

### deriveTypeDirectedFeature

The directedFeatures of a Type are those features for which the direction is non-null.

```ocl
directedFeature = feature->select(f | directionOf(f) <> null)
```

### deriveTypeEndFeature

The endFeatures of a Type are all its features for which isEnd = true.

```ocl
endFeature = feature->select(isEnd)
```

### deriveTypeFeature

The features of a Type are the ownedMemberFeatures of its featureMemberships.

```ocl
feature = featureMembership.ownedMemberFeature
```

### deriveTypeFeatureMembership

The featureMemberships of a Type is the union of the ownedFeatureMemberships and those inheritedMemberships that are FeatureMemberships.

```ocl
featureMembership = ownedFeatureMembership->union(
    inheritedMembership->selectByKind(FeatureMembership))
```

### deriveTypeInheritedFeature

The inheritedFeatures of this Type are the memberFeatures of the inheritedMemberships that are FeatureMemberships.

```ocl
inheritedFeature = inheritedMemberships->
    selectByKind(FeatureMembership).memberFeature
```

### deriveTypeInheritedMembership

The inheritedMemberships of a Type are determined by the inheritedMemberships() operation.

```ocl
inheritedMembership = inheritedMemberships(Set{}, Set{}, false)
```

### deriveTypeInput

The inputs of a Type are those of its features that have a direction of in or inout relative to the Type, taking conjugation into account.

```ocl
input = feature->select(f | 
    let direction: FeatureDirectionKind = directionOf(f) in
    direction = FeatureDirectionKind::_'in' or
    direction = FeatureDirectionKind::inout)
```

### deriveTypeIntersectingType

The intersectingTypes of a Type are the intersectingTypes of its ownedIntersectings.

```ocl
intersectingType = ownedIntersecting.intersectingType
```

### deriveTypeMultiplicity

If a Type has an owned Multiplicity, then that is its multiplicity. Otherwise, if the Type has an ownedSpecialization, then its multiplicity is the multiplicity of the general Type of that Specialization.

```ocl
multiplicity = 
    let ownedMultiplicities: Sequence(Multiplicity) =
        ownedMember->selectByKind(Multiplicity) in
    if ownedMultiplicities->isEmpty() then null
    else ownedMultiplicities->first()
    endif
```

### deriveTypeOutput

The outputs of a Type are those of its features that have a direction of <ode>out or inout relative to the Type, taking conjugation into account.</ode>

```ocl
output = feature->select(f | 
    let direction: FeatureDirectionKind = directionOf(f) in
    direction = FeatureDirectionKind::out or
    direction = FeatureDirectionKind::inout)
```

### deriveTypeOwnedConjugator

The ownedConjugator of a Type is the its single ownedRelationship that is a Conjugation.

```ocl
ownedConjugator =
    let ownedConjugators: Sequence(Conjugator) = 
        ownedRelationship->selectByKind(Conjugation) in
    if ownedConjugators->isEmpty() then null 
    else ownedConjugators->at(1) endif
```

### deriveTypeOwnedDifferencing

The ownedDifferencings of a Type are its ownedRelationships that are Differencings.

```ocl
ownedDifferencing =
    ownedRelationship->selectByKind(Differencing)
```

### deriveTypeOwnedDisjoining

The ownedDisjoinings of a Type are the ownedRelationships that are Disjoinings.

```ocl
ownedDisjoining =
    ownedRelationship->selectByKind(Disjoining)
```

### deriveTypeOwnedEndFeature

The ownedEndFeatures of a Type are all its ownedFeatures for which isEnd = true.

```ocl
ownedEndFeature = ownedFeature->select(isEnd)
```

### deriveTypeOwnedFeature

The ownedFeatures of a Type are the ownedMemberFeatures of its ownedFeatureMemberships.

```ocl
ownedFeature = ownedFeatureMembership.ownedMemberFeature
```

### deriveTypeOwnedFeatureMembership

The ownedFeatureMemberships of a Type are its ownedMemberships that are FeatureMemberships.

```ocl
ownedFeatureMembership = ownedRelationship->selectByKind(FeatureMembership)
```

### deriveTypeOwnedIntersecting

The ownedIntersectings of a Type are the ownedRelationships that are Intersectings.

```ocl
ownedRelationship->selectByKind(Intersecting)
```

### deriveTypeOwnedSpecialization

The ownedSpecializations of a Type are the ownedRelationships that are Specializations whose special Type is the owning Type.

```ocl
ownedSpecialization = ownedRelationship->selectByKind(Specialization)->
    select(s | s.special = self)
```

### deriveTypeOwnedUnioning

The ownedUnionings of a Type are the ownedRelationships that are Unionings.

```ocl
ownedUnioning =
    ownedRelationship->selectByKind(Unioning)
```

### deriveTypeUnioningType

The unioningTypes of a Type are the unioningTypes of its ownedUnionings.

```ocl
unioningType = ownedUnioning.unioningType
```

### validateTypeAtMostOneConjugator

A Type must have at most one owned Conjugation Relationship.

```ocl
ownedRelationship->selectByKind(Conjugation)->size() <= 1
```

### validateTypeDifferencingTypesNotSelf

A Type cannot be one of its own differencingTypes.

```ocl
differencingType->excludes(self)
```

### validateTypeIntersectingTypesNotSelf

A Type cannot be one of its own intersectingTypes.

```ocl
intersectingType->excludes(self)
```

### validateTypeOwnedDifferencingNotOne

A Type must not have exactly one ownedDifferencing.

```ocl
ownedDifferencing->size() <> 1
```

### validateTypeOwnedIntersectingNotOne

A Type must not have exactly one ownedIntersecting.

```ocl
ownedIntersecting->size() <> 1
```

### validateTypeOwnedMultiplicity

A Type may have at most one ownedMember that is a Multiplicity.

```ocl
ownedMember->selectByKind(Multiplicity)->size() <= 1
```

### validateTypeOwnedUnioningNotOne

A Type must not have exactly one ownedUnioning.

```ocl
ownedUnioning->size() <> 1
```

### validateTypeUnioningTypesNotSelf

A Type cannot be one of its own unioningTypes.

```ocl
unioningType->excludes(self)
```

