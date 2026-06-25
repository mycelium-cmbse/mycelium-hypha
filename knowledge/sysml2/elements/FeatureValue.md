---
name: FeatureValue
package: FeatureValues
isAbstract: false
generalizes: [OwningMembership]
specializedBy: []
---

# FeatureValue

`FeatureValues` package · concrete metaclass

A FeatureValue is a Membership that identifies a particular member Expression that provides the value of the Feature that owns the FeatureValue. The value is specified as either a bound value or an initial value, and as either a concrete or default value. A Feature can have at most one FeatureValue.The result of the value Expression is bound to the featureWithValue using a BindingConnector. If isInitial = false, then the featuringType of the BindingConnector is the same as the featuringType of the featureWithValue. If isInitial = true, then the featuringType of the BindingConnector is restricted to its startShot.If isDefault = false, then the above semantics of the FeatureValue are realized for the given featureWithValue. Otherwise, the semantics are realized for any individual of the featuringType of the featureWithValue, unless another value is explicitly given for the featureWithValue for that individual.

## Generalizations

- [OwningMembership](OwningMembership.md)

## Owned features

### featureWithValue : Feature [1..1] {derived}

The Feature to be provided a value.

Subsets: `membershipOwningNamespace`

### isDefault : Boolean [1..1]

Whether this FeatureValue is a concrete specification of the bound or initial value of the featureWithValue, or just a default value that may be overridden.

### isInitial : Boolean [1..1]

Whether this FeatureValue specifies a bound value or an initial value for the featureWithValue.

### value : Expression [1..1] {derived, composite}

The Expression that provides the value as a result.

Redefines: `ownedMemberElement`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| memberElement | Element | [1..1] | [Membership](Membership.md) |  |
| memberElementId | String | [1..1] | [Membership](Membership.md) | derived |
| memberName | String | [0..1] | [Membership](Membership.md) |  |
| memberShortName | String | [0..1] | [Membership](Membership.md) |  |
| membershipOwningNamespace | Namespace | [1..1] | [Membership](Membership.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedMemberElement | Element | [1..1] | [OwningMembership](OwningMembership.md) | derived, composite |
| ownedMemberElementId | String | [1..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberName | String | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberShortName | String | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedRelatedElement | Element | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | Element | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| visibility | VisibilityKind | [1..1] | [Membership](Membership.md) |  |

## Constraints

### checkFeatureValueBindingConnector

If isDefault = false, then the featureWithValue must have an ownedMember that is a BindingConnector whose relatedElements are the featureWithValue and a feature chain consisting of the value Expression and its result. If isInitial = false, then this BindingConnector must have featuringTypes that are the same as those of the featureWithValue. If isInitial = true, then the BindingConnector must have that.startShot as its featuringType.

```ocl
not isDefault implies
    featureWithValue.ownedMember->
        selectByKind(BindingConnector)->exists(b |
            b.relatedFeature->includes(featureWithValue) and
            b.relatedFeature->exists(f | 
                f.chainingFeature = Sequence{value, value.result}) and
            if not isInitial then 
                b.featuringType = featureWithValue.featuringType
            else 
                b.featuringType->exists(t |
                    t.oclIsKindOf(Feature) and
                    t.oclAsType(Feature).chainingFeature =
                        Sequence{
                            resolveGlobal('Base::things::that').
                                memberElement,
                            resolveGlobal('Occurrences::Occurrence::startShot').
                                memberElement
                        }
                )
            endif)
```

### validateFeatureValueIsInitial

If a FeatureValue has isInitial = true, then its featureWithValue must have isVariable = true.

```ocl
isInitial implies featureWithValue.isVariable
```

### validateFeatureValueOverriding

All Features directly or indirectly redefined by the featureWithValue of a FeatureValue must have only default FeatureValues.

```ocl
featureWithValue.redefinition.redefinedFeature->
    closure(redefinition.redefinedFeature).valuation->
    forAll(isDefault)
```

