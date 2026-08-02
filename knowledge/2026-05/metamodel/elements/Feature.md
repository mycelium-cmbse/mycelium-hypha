---
name: Feature
package: Features
fully qualified name: KerML::Core::Features::Feature
isAbstract: false
visibility: public
generalizes: [Type]
specializedBy: [Connector, FlowEnd, MetadataFeature, Multiplicity, PayloadFeature, Step, Usage]
---

# Feature

`Features` package · concrete metaclass

A Feature is a Type that classifies relations between multiple things (in the universe). The domain of the relation is the intersection of the featuringTypes of the Feature. (The domain of a Feature with no featuringTyps is implicitly the most general Type Base::Anything from the Kernel Semantic Library.) The co-domain of the relation is the intersection of the types of the Feature.In the simplest cases, the featuringTypes and types are Classifiers and the Feature relates two things, one from the domain and one from the range. Examples include cars paired with wheels, people paired with other people, and cars paired with numbers representing the car length.Since Features are Types, their featuringTypes and types can be Features. In this case, the Feature effectively classifies relations between relations, which can be interpreted as the sequence of things related by the domain Feature concatenated with the sequence of things related by the co-domain Feature.The values of a Feature for a given instance of its domain are all the instances of its co-domain that are related to that domain instance by the Feature. The values of a Feature with chainingFeatures are the same as values of the last Feature in the chain, which can be found by starting with values of the first Feature, then using those values as domain instances to obtain valus of the second Feature, and so on, to values of the last Feature.

## Generalizations

- [Type](Type.md)

## Specializations

- [Connector](Connector.md)
- [FlowEnd](FlowEnd.md)
- [MetadataFeature](MetadataFeature.md)
- [Multiplicity](Multiplicity.md)
- [PayloadFeature](PayloadFeature.md)
- [Step](Step.md)
- [Usage](Usage.md)

## Owned features

### chainingFeature

`+` [Feature](Feature.md) · `[0..*]` · *derived, ordered*

The Feature that are chained together to determine the values of this Feature, derived from the chainingFeatures of the ownedFeatureChainings of this Feature, in the same order. The values of a Feature with chainingFeatures are the same as values of the last Feature in the chain, which can be found by starting with the values of the first Feature (for each instance of the domain of the original Feature), then using each of those as domain instances to find the values of the second Feature in chainingFeatures, and so on, to values of the last Feature.

### crossFeature

`+` [Feature](Feature.md) · `[0..1]` · *derived*

The second chainingFeature of the crossedFeature of the ownedCrossSubsetting of this Feature, if it has one. Semantically, the values of the crossFeature of an end Feature must include all values of the end Feature obtained when navigating from values of the other end Features of the same owningType.

### direction

`+` [FeatureDirectionKind](FeatureDirectionKind.md) · `[0..1]`

Indicates how values of this Feature are determined or used (as specified for the FeatureDirectionKind).

### endOwningType

`+` [Type](Type.md) · `[0..1]` · *derived*

The Type that is related to this Feature by an EndFeatureMembership in which the Feature is an ownedMemberFeature.

Subsets `typeWithEndFeature`, [owningType](#owningtype)

### featureTarget

`+` [Feature](Feature.md) · `[1..1]` · *derived*

The last of the chainingFeatures of this Feature, if it has any. Otherwise, this Feature itself.

### featuringType

`+` [Type](Type.md) · `[0..*]` · *derived, ordered*

Types that feature this Feature, such that any instance in the domain of the Feature must be classified by all of these Types, including at least all the featuringTypes of its typeFeaturings. If the Feature is chained, then the featuringTypes of the first Feature in the chain are also featuringTypes of the chained Feature.

### isComposite

`+` [Boolean](Boolean.md) · `[1..1]`

Whether the Feature is a composite feature of its featuringType. If so, the values of the Feature cannot exist after its featuring instance no longer does and cannot be values of another composite feature that is not on the same featuring instance.

### isConstant

`+` [Boolean](Boolean.md) · `[1..1]`

If isVariable is true, then whether the value of this Feature nevertheless does not change over all snapshots of its owningType.

### isDerived

`+` [Boolean](Boolean.md) · `[1..1]`

Whether the values of this Feature can always be computed from the values of other Features.

### isEnd

`+` [Boolean](Boolean.md) · `[1..1]`

Whether or not this Feature is an end Feature. An end Feature always has multiplicity 1, mapping each of its domain instances to a single co-domain instance. However, it may have a crossFeature, in which case values of the crossFeature must be the same as those found by navigation across instances of the owningType from values of other end Features to values of this Feature. If the owningType has n end Features, then the multiplicity, ordering, and uniqueness declared for the crossFeature of any one of these end Features constrains the cardinality, ordering, and uniqueness of the collection of values of that Feature reached by navigation when the values of the other n-1 end Features are held fixed.

### isOrdered

`+` [Boolean](Boolean.md) · `[1..1]`

Whether an order exists for the values of this Feature or not.

### isPortion

`+` [Boolean](Boolean.md) · `[1..1]`

Whether the values of this Feature are contained in the space and time of instances of the domain of the Feature and represent the same thing as those instances.

### isUnique

`+` [Boolean](Boolean.md) · `[1..1]`

Whether or not values for this Feature must have no duplicates or not.

### isVariable

`+` [Boolean](Boolean.md) · `[1..1]`

Whether the value of this Feature might vary over time. That is, whether the Feature may have a different value for each snapshot of an owningType that is an Occurrence.

### ownedCrossSubsetting

`+` [CrossSubsetting](CrossSubsetting.md) · `[0..1]` · *derived, composite*

The one ownedSubsetting of this Feature, if any, that is a CrossSubsetting}, for which the Feature is the crossingFeature.

Subsets [ownedSubsetting](#ownedsubsetting)

### ownedFeatureChaining

`+` [FeatureChaining](FeatureChaining.md) · `[0..*]` · *derived, composite, ordered*

The ownedRelationships of this Feature that are FeatureChainings, for which the Feature will be the featureChained.

Subsets [ownedRelationship](Element.md#ownedrelationship), `sourceRelationship`

### ownedFeatureInverting

`+` [FeatureInverting](FeatureInverting.md) · `[0..*]` · *derived, composite*

The ownedRelationships of this Feature that are FeatureInvertings and for which the Feature is the featureInverted.

Subsets `invertingFeatureInverting`, [ownedRelationship](Element.md#ownedrelationship)

### ownedRedefinition

`+` [Redefinition](Redefinition.md) · `[0..*]` · *derived, composite*

The ownedSubsettings of this Feature that are Redefinitions, for which the Feature is the redefiningFeature.

Subsets [ownedSubsetting](#ownedsubsetting)

### ownedReferenceSubsetting

`+` [ReferenceSubsetting](ReferenceSubsetting.md) · `[0..1]` · *derived, composite*

The one ownedSubsetting of this Feature, if any, that is a ReferenceSubsetting, for which the Feature is the referencingFeature.

Subsets [ownedSubsetting](#ownedsubsetting)

### ownedSubsetting

`+` [Subsetting](Subsetting.md) · `[0..*]` · *derived, composite*

The ownedSpecializations of this Feature that are Subsettings, for which the Feature is the subsettingFeature.

Subsets [ownedSpecialization](Type.md#ownedspecialization), `subsetting`

### ownedTypeFeaturing

`+` [TypeFeaturing](TypeFeaturing.md) · `[0..*]` · *derived, composite, ordered*

The ownedRelationships of this Feature that are TypeFeaturings and for which the Feature is the featureOfType.

Subsets `typeFeaturing`, [ownedRelationship](Element.md#ownedrelationship)

### ownedTyping

`+` [FeatureTyping](FeatureTyping.md) · `[0..*]` · *derived, composite, ordered*

The ownedSpecializations of this Feature that are FeatureTypings, for which the Feature is the typedFeature.

Subsets [ownedSpecialization](Type.md#ownedspecialization), `typing`

### owningFeatureMembership

`+` [FeatureMembership](FeatureMembership.md) · `[0..1]` · *derived*

The FeatureMembership that owns this Feature as an ownedMemberFeature, determining its owningType.

Subsets [owningMembership](Element.md#owningmembership)

### owningType

`+` [Type](Type.md) · `[0..1]` · *derived*

The Type that is the owningType of the owningFeatureMembership of this Feature.

Subsets `typeWithFeature`, [owningNamespace](Element.md#owningnamespace), [featuringType](#featuringtype)

### type

`+` [Type](Type.md) · `[0..*]` · *derived, ordered*

Types that restrict the values of this Feature, such that the values must be instances of all the types. The types of a Feature are derived from its typings and the types of its subsettings. If the Feature is chained, then the types of the last Feature in the chain are also types of the chained Feature.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| differencingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| endFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| feature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | [FeatureMembership](FeatureMembership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| importedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | [Membership](Membership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| input | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | [Boolean](Boolean.md) | [1..1] | [Type](Type.md) |  |
| isConjugated | [Boolean](Boolean.md) | [1..1] | [Type](Type.md) | derived |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| isSufficient | [Boolean](Boolean.md) | [1..1] | [Type](Type.md) |  |
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | [Multiplicity](Multiplicity.md) | [0..1] | [Type](Type.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| output | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedConjugator | [Conjugation](Conjugation.md) | [0..1] | [Type](Type.md) | derived, composite |
| ownedDifferencing | [Differencing](Differencing.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedDisjoining | [Disjoining](Disjoining.md) | [0..*] | [Type](Type.md) | derived, composite |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedEndFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedImport | [Import](Import.md) | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedIntersecting | [Intersecting](Intersecting.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedMember | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | [Specialization](Specialization.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedUnioning | [Unioning](Unioning.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkFeatureCrossingSpecialization

If this Feature has isEnd = true and ownedCrossFeature returns a non-null value, then the crossFeature of the Feature must be the Feature returned from ownedCrossFeature (which implies that this Feature has an appropriate ownedCrossSubsetting to realize this).

```ocl
ownedCrossFeature() <> null implies
    crossFeature = ownedCrossFeature()
```

### checkFeatureDataValueSpecialization

If a Feature has an ownedTyping relationship to a DataType, then it must directly or indirectly specialize Base::dataValues from the Kernel Semantic Library.

```ocl
ownedTyping.type->exists(selectByKind(DataType)) implies
    specializesFromLibrary('Base::dataValues')
```

### checkFeatureEndRedefinition

If a Feature has isEnd = true and an owningType that is not empty, then, for each direct supertype of its owningType, it must redefine the endFeature at the same position, if any.

```ocl
isEnd and owningType <> null implies
    let i : Integer = 
        owningType.ownedEndFeature->indexOf(self) in
    owningType.ownedSpecialization.general->
        forAll(supertype |
             supertype.endFeature->size() >= i implies
                redefines(supertype.endFeature->at(i))
```

### checkFeatureEndSpecialization

If a Feature has isEnd = true and an owningType that is an Association or a Connector, then it must directly or indirectly specialize Links::Link::participant from the Kernel Semantic Library.

```ocl
isEnd and owningType <> null and
(owningType.oclIsKindOf(Association) or
 owningType.oclIsKindOf(Connector)) implies
    specializesFromLibrary('Links::Link::participant')
```

### checkFeatureFeatureMembershipTypeFeaturing

If a Feature is owned via a FeatureMembership, then it must have a featuringType for which the operation isFeaturingType returns true.

```ocl
owningFeatureMembership <> null implies
    featuringTypes->exists(t | isFeaturingType(t))
```

### checkFeatureFlowFeatureRedefinition

If a Feature is the first ownedFeature of a first or second FlowEnd, then it must directly or indirectly specialize either Transfers::Transfer::source::sourceOutput or Transfers::Transfer::target::targetInput, respectively, from the Kernel Semantic Library.

```ocl
owningType <> null and
owningType.oclIsKindOf(FlowEnd) and
owningType.ownedFeature->at(1) = self implies
    let flowType : Type = owningType.owningType in
    flowType <> null implies
        let i : Integer = 
            flowType.ownedFeature.indexOf(owningType) in
        (i = 1 implies 
            redefinesFromLibrary('Transfers::Transfer::source::sourceOutput')) and
        (i = 2 implies
            redefinesFromLibrary('Transfers::Transfer::target::targetInput'))
```

### checkFeatureObjectSpecialization

If a Feature has an ownedTyping relationship to a Structure, then it must directly or indirectly specialize Objects::objects from the Kernel Semantics Library.

```ocl
ownedTyping.type->exists(selectByKind(Structure)) implies
    specializesFromLibrary('Objects::objects')
```

### checkFeatureOccurrenceSpecialization

If a Feature has an ownedTyping relationship to a Class, then it must directly or indirectly specialize Occurrences::occurrences from the Kernel Semantic Library.

```ocl
ownedTyping.type->exists(selectByKind(Class)) implies
    specializesFromLibrary('Occurrences::occurrences')
```

### checkFeatureOwnedCrossFeatureRedefinitionSpecialization

If this Feature is the ownedCrossFeature of an end Feature, then, for any end Feature that is redefined by the owning end Feature of this Feature, this Feature must subset the crossFeature of the redefined end Feature, if this exists.

```ocl
isOwnedCrossFeature() implies
    ownedSubsetting.subsettedFeature->includesAll(
        owner.oclAsType(Feature).ownedRedefinition.redefinedFeature->
            select(crossFeature <> null).crossFeature)
```

### checkFeatureOwnedCrossFeatureSpecialization

If this Feature is the ownedCrossFeature of an end Feature, then it must directly or indirectly specialize the types of its owning end Feature.

```ocl
isOwnedCrossFeature() implies
    owner.oclAsType(Feature).type->forAll(t | self.specializes(t))
```

### checkFeatureOwnedCrossFeatureTypeFeaturing

If this Feature is the ownedCrossFeature of an end Feature, then it must have featuringTypes consistent with the crossing from other end Features of the owningType of its end Feature.

```ocl
isOwnedCrossFeature() implies
    let otherEnds : OrderedSet(Feature) = 
        owner.oclAsType(Feature).owningType.endFeature->excluding(self) in
    if (otherEnds->size() = 1) then
        featuringType = otherEnds->first().type
    else
        featuringType->size() = 1 and
        featuringType->first().isCartesianProduct() and
        featuringType->first().asCartesianProduct() = otherEnds.type and
        featuringType->first().allSupertypes()->includesAll(
            owner.oclAsType(Feature).ownedRedefinition.redefinedFeature->
               select(crossFeature() <> null).crossFeature().featuringType)      
    endif
```

### checkFeatureParameterRedefinition

If a Feature is a parameter of an owningType that is a Behavior or Step, but not<ul> <li>A result parameter</li> <li>A parameter of an InvocationExpression, with at least one non-implied ownedRedefinition</li></ul>then, for each direct supertype of its owningType that is also a Behavior or Step, it must redefine the parameter at the same position, if any.

```ocl
owningType <> null and
(owningType.oclIsKindOf(Behavior) or
 owningType.oclIsKindOf(Step) and
    (owningType.oclIsKindOf(InvocationExpression) implies
        not ownedRedefinition->exists(not isImplied)))
implies
    let ownerParameters : Sequence(Feature) =
        owningType.ownedFeature->select(direction <> null)->
            reject(owningFeatureMembership.
                oclIsKindOf(ReturnParameterMembership)) in
    ownerParameters->includes(self) implies
        let i : Integer = ownerParameters.indexof(self) in
        owningType.ownedSpecialization.general->
            forAll(supertype |
                supertype.oclIsKindOf(Behavior) or 
                    supertype.oclIsKindOf(Step) 
                implies
                    let ownedParameters : Sequence(Feature) =
                        supertype.ownedFeature->select(direction <> null)->
                            reject(owningFeatureMembership.
                                oclIsKindOf(ReturnParameterMembership)) in
                    ownedParameters->size() >= i implies
                        redefines(ownedParameters->at(i)))
```

### checkFeaturePortionSpecialization

If a Feature has isPortion = true, an ownedTyping relationship to a Class, and an owningType that is a Class or another Feature typed by a Class, then it must directly or indirectly specialize Occurrences::Occurrence::portions from the Kernel Semantic Library.

```ocl
isPortion and
ownedTyping.type->includes(oclIsKindOf(Class)) and
owningType <> null and
(owningType.oclIsKindOf(Class) or
 owningType.oclIsKindOf(Feature) and
    owningType.oclAsType(Feature).type->
        exists(oclIsKindOf(Class))) implies
    specializesFromLibrary('Occurrence::Occurrence::portions')
```

### checkFeatureResultRedefinition

If a Feature is a result parameter of an owningType that is a Function or Expression, then, for each direct supertype of its owningType that is also a Function or Expression, it must redefine the result parameter.

```ocl
owningType <> null and
(owningType.oclIsKindOf(Function) and
    self = owningType.oclAsType(Function).result or
 owningType.oclIsKindOf(Expression) and
    self = owningType.oclAsType(Expression).result) implies
    owningType.ownedSpecialization.general->
        select(oclIsKindOf(Function) or oclIsKindOf(Expression))->
        forAll(supertype |
            redefines(
                if superType.oclIsKindOf(Function) then
                    superType.oclAsType(Function).result
                else
                    superType.oclAsType(Expression).result
                endif)
```

### checkFeatureSpecialization

A Feature must directly or indirectly specialize Base::things from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Base::things')
```

### checkFeatureSubobjectSpecialization

A composite Feature typed by a Structure, and whose ownedType is a Structure or another Feature typed by a Structure must directly or indirectly specialize Objects::Object::subobjects.

```ocl
isComposite and
ownedTyping.type->includes(oclIsKindOf(Structure)) and
owningType <> null and
(owningType.oclIsKindOf(Structure) or
 owningType.type->includes(oclIsKindOf(Structure))) implies
    specializesFromLibrary('Occurrence::Occurrence::suboccurrences')
```

### checkFeatureSuboccurrenceSpecialization

A composite Feature that has an ownedTyping relationship to a Class, and whose ownedType is a Class or another Feature typed by a Class, must directly or indirectly specialize Occurrences::Occurrence::suboccurrences.

```ocl
isComposite and
ownedTyping.type->includes(oclIsKindOf(Class)) and
owningType <> null and
(owningType.oclIsKindOf(Class) or
 owningType.oclIsKindOf(Feature) and
    owningType.oclAsType(Feature).type->
        exists(oclIsKindOf(Class))) implies
    specializesFromLibrary('Occurrence::Occurrence::suboccurrences')
```

### checkFeatureValuationSpecialization

If a Feature has a FeatureValue, no ownedSpecializations that are not implied, and is not directed, then it must specialize the result of the value Expression of the FeatureValue.

```ocl
direction = null and
ownedSpecializations->forAll(isImplied) implies
    ownedMembership->
        selectByKind(FeatureValue)->
        forAll(fv | specializes(fv.value.result))
```

### deriveFeatureChainingFeature

The chainingFeatures of a Feature are the chainingFeatures of its ownedFeatureChainings.

```ocl
chainingFeature = ownedFeatureChaining.chainingFeature
```

### deriveFeatureCrossFeature

The crossFeature of a Feature is the second chainingFeature of the crossedFeature of the ownedCrossSubsetting of the Feature, if any.

```ocl
crossFeature =
    if ownedCrossSubsetting = null then null
    else 
        let chainingFeatures: Sequence(Feature) = 
            ownedCrossSubsetting.crossedFeature.chainingFeature in
        if chainingFeatures->size() < 2 then null
        else chainingFeatures->at(2)
    endif
```

### deriveFeatureFeatureTarget

If a Feature has no chainingFeatures, then its featureTarget is the Feature itself, otherwise the featureTarget is the last of the chainingFeatures.

```ocl
featureTarget = if chainingFeature->isEmpty() then self else chainingFeature->last() endif
```

### deriveFeatureFeaturingType

The featuringTypes of a Feature include the featuringTypes of all the typeFeaturings of the Feature. If the Feature has chainingFeatures, then its featuringTypes also include the featuringTypes of the first chainingFeature.

```ocl
featuringType =
    let featuringTypes : OrderedSet(Type) = 
        typeFeaturing.type->asOrderedSet() in
    if chainingFeature->isEmpty() then featuringTypes
    else
        featuringTypes->
            union(chainingFeature->first().featuringType)->
            asOrderedSet()
    endif
```

### deriveFeatureOwnedCrossSubsetting

The ownedCrossSubsetting of a Feature is the ownedSubsetting that is a CrossSubsetting, if any.

```ocl
ownedCrossSubsetting =
    let crossSubsettings: Sequence(CrossSubsetting) = 
        ownedSubsetting->selectByKind(CrossSubsetting) in
    if crossSubsettings->isEmpty() then null
    else crossSubsettings->first()
    endif
```

### deriveFeatureOwnedFeatureChaining

The ownedFeatureChainings of a Feature are the ownedRelationships that are FeatureChainings.

```ocl
ownedFeatureChaining = ownedRelationship->selectByKind(FeatureChaining)
```

### deriveFeatureOwnedFeatureInverting

The ownedFeatureInvertings of a Feature are its ownedRelationships that are FeatureInvertings.

```ocl
ownedFeatureInverting = ownedRelationship->selectByKind(FeatureInverting)->
    select(fi | fi.featureInverted = self)
```

### deriveFeatureOwnedRedefinition

The ownedRedefinitions of a Feature are its ownedSubsettings that are Redefinitions.

```ocl
ownedRedefinition = ownedSubsetting->selectByKind(Redefinition)
```

### deriveFeatureOwnedReferenceSubsetting

The ownedReferenceSubsetting of a Feature is the firstownedSubsetting that is a ReferenceSubsetting (if any).

```ocl
ownedReferenceSubsetting =
    let referenceSubsettings : OrderedSet(ReferenceSubsetting) =
        ownedSubsetting->selectByKind(ReferenceSubsetting) in
    if referenceSubsettings->isEmpty() then null
    else referenceSubsettings->first() endif
```

### deriveFeatureOwnedSubsetting

The ownedSubsettings of a Feature are its ownedSpecializations that are Subsettings.

```ocl
ownedSubsetting = ownedSpecialization->selectByKind(Subsetting)
```

### deriveFeatureOwnedTypeFeaturing

The ownedTypeFeaturings of a Feature are its ownedRelationships that are TypeFeaturings and which have the Feature as their featureOfType.

```ocl
ownedTypeFeaturing = ownedRelationship->selectByKind(TypeFeaturing)->
    select(tf | tf.featureOfType = self)
```

### deriveFeatureOwnedTyping

The ownedTypings of a Feature are its ownedSpecializations that are FeatureTypings.

```ocl
ownedTyping = ownedGeneralization->selectByKind(FeatureTyping)
```

### deriveFeatureType

The types of a Feature are the union of the types of its typings and the types of the Features it subsets, with all redundant supertypes removed. If the Feature has chainingFeatures, then the union also includes the types of the last chainingFeature.

```ocl
type = 
    let types : OrderedSet(Types) = OrderedSet{self}->
        -- Note: The closure operation automatically handles circular relationships.
        closure(typingFeatures()).typing.type->asOrderedSet() in
    types->reject(t1 | types->exist(t2 | t2 <> t1 and t2.specializes(t1)))
```

### validateFeatureChainingFeatureConformance

Each chainingFeature (other than the first) must be featured within the previous chainingFeature.

```ocl
Sequence{2..chainingFeature->size()}->forAll(i |
    chainingFeature->at(i).isFeaturedWithin(chainingFeature->at(i-1)))
```

### validateFeatureChainingFeatureNotOne

A Feature must have either no chainingFeatures or more than one.

```ocl
chainingFeature->size() <> 1
```

### validateFeatureChainingFeaturesNotSelf

A Feature cannot be one of its own chainingFeatures.

```ocl
chainingFeature->excludes(self)
```

### validateFeatureConstantIsVariable

A Feature with isConstant = true must have isVariable = true.

```ocl
isConstant implies isVariable
```

### validateFeatureCrossFeatureSpecialization

If this Feature has a crossFeature, then, for any Feature that is redefined by this Feature, the crossFeature must specialize the crossFeature of the redefined end Feature, if this exists.

```ocl
crossFeature <> null implies
    ownedRedefinition.redefinedFeature.crossFeature->
            forAll(f | f <> null implies crossFeature.specializes(f))
```

### validateFeatureCrossFeatureType

The crossFeature of a Feature must have the same types as the Feature.

```ocl
crossFeature <> null implies
    crossFeature.type->asSet() = type->asSet()
```

### validateFeatureEndIsConstant

A Feature with isEnd = true and isVariable = true must have isConstant = true.

```ocl
isEnd and isVariable implies isConstant
```

### validateFeatureEndMultiplicity

If a Feature has isEnd = true, then it must have multiplicity 1..1.

```ocl
isEnd implies 
    multiplicities().allSuperTypes()->flatten()->
    selectByKind(MultiplicityRange)->exists(hasBounds(1,1))
```

### validateFeatureEndNoDirection

A Feature with isEnd = true must have no direction.

```ocl
isEnd implied direction = null
```

### validateFeatureEndNotDerivedAbstractCompositeOrPortion

A Feature with isEnd = true must have all of isDerived = false, isAbstract = false, isComposite = false, and isPortion = false.

```ocl
isEnd implies not (isDerived or isAbstract or isComposite or isPortion)
```

### validateFeatureIsVariable

A Feature with isVariable = true must have an owningType that directly or indirectly specializes the Class Occurrences::Occurrence from the Kernel Semantic Library.

```ocl
isVariable implies
    owningType <> null and 
    owningType.specializes('Occurrences::Occurrence')
```

### validateFeatureMultiplicityDomain

If a Feature has a multiplicity, then the featuringTypes of the multiplicity must be the same as those of the Feature itself.

```ocl
multiplicity <> null implies multiplicity.featuringType = featuringType
```

### validateFeatureOwnedCrossSubsetting

A Feature must have at most one ownedSubsetting that is a CrossSubsetting.

```ocl
ownedSubsetting->selectByKind(CrossSubsetting)->size() <= 1
```

### validateFeatureOwnedReferenceSubsetting

A Feature must have at most one ownedSubsetting that is an ReferenceSubsetting.

```ocl
ownedSubsetting->selectByKind(ReferenceSubsetting)->size() <= 1
```

### validateFeaturePortionNotVariable

```ocl
isPortion implies not isVariable
```

