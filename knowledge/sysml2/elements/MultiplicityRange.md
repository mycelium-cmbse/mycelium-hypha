---
name: MultiplicityRange
package: Multiplicities
fully qualified name: KerML::Kernel::Multiplicities::MultiplicityRange
isAbstract: false
visibility: public
generalizes: [Multiplicity]
specializedBy: []
---

# MultiplicityRange

`Multiplicities` package · concrete metaclass

A MultiplicityRange is a Multiplicity whose value is defined to be the (inclusive) range of natural numbers given by the result of a lowerBound Expression and the result of an upperBound Expression. The result of these Expressions shall be of type Natural. If the result of the upperBound Expression is the unbounded value *, then the specified range includes all natural numbers greater than or equal to the lowerBound value. If no lowerBound Expression, then the default is that the lower bound has the same value as the upper bound, except if the upperBound evaluates to *, in which case the default for the lower bound is 0.

## Generalizations

- [Multiplicity](Multiplicity.md)

## Owned features

### bound

`+` [Expression](Expression.md) · `[1..2]` · *derived, ordered*

The owned Expressions of the MultiplicityRange whose results provide its bounds. These must be the first ownedMembers of the MultiplicityRange.

Subsets [ownedMember](Namespace.md#ownedmember)

### lowerBound

`+` [Expression](Expression.md) · `[0..1]` · *derived*

The Expression whose result provides the lower bound of the MultiplicityRange. If no lowerBound Expression is given, then the lower bound shall have the same value as the upper bound, unless the upper bound is unbounded (*), in which case the lower bound shall be 0.

Subsets [bound](#bound)

### upperBound

`+` [Expression](Expression.md) · `[1..1]` · *derived*

The Expression whose result is the upper bound of the MultiplicityRange.

Subsets [bound](#bound)


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
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| type | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkMultiplicityRangeExpressionTypeFeaturing

The bounds of a MultiplicityRange must have the same featuringTypes as the MultiplicityRange.

```ocl
bound->forAll(b | b.featuringType = self.featuringType)
```

### deriveMultiplicityRangeBound

The bounds of a MultiplicityRange are the lowerBound (if any) followed by the upperBound.

```ocl
bound =
    if upperBound = null then Sequence{}
    else if lowerBound = null then Sequence{upperBound}
    else Sequence{lowerBound, upperBound}
    endif endif
```

### deriveMultiplicityRangeLowerBound

If a MultiplicityRange has two ownedMembers that are Expressions, then the lowerBound is the first of these, otherwise it is null.

```ocl
lowerBound =
    let ownedExpressions : Sequence(Expression) =
        ownedMember->selectByKind(Expression) in
    if ownedExpressions->size() < 2 then null
    else ownedExpressions->first()
    endif
```

### deriveMultiplicityRangeUpperBound

If a MultiplicityRange has one ownedMember that is an Expression, then this is the upperBound. If it has more than one ownedMember that is an Expression, then the upperBound is the second of those. Otherwise, it is null.

```ocl
upperBound =
    let ownedExpressions : Sequence(Expression) =
        ownedMember->selectByKind(Expression) in
    if ownedExpressions->isEmpty() then null
    else if ownedExpressions->size() = 1 then ownedExpressions->at(1)
    else ownedExpressions->at(2)
    endif endif
```

### validateMultiplicityRangeBoundResultTypes

The results of the bound Expression(s) of a MultiplicityRange must be typed by ScalarValues::Intger from the Kernel Data Types Library. If a bound is model-level evaluable, then it must evaluate to a non-negative value.

```ocl
bound->forAll(b |
    b.result.specializesFromLibrary('ScalarValues::Integer') and
    let value : UnlimitedNatural = valueOf(b) in
    value <> null implies value >= 0
)
```

### validateMultiplicityRangeBounds

The lowerBound (if any) and upperBound Expressions must be the first ownedMembers of a MultiplicityRange.

```ocl
if lowerBound = null then
    ownedMember->notEmpty() and
    ownedMember->at(1) = upperBound
else
    ownedMember->size() > 1 and
    ownedMember->at(1) = lowerBound and
    ownedMember->at(2) = upperBound
endif
```

