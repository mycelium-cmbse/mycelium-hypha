---
name: Multiplicity
package: Types
fully qualified name: KerML::Core::Types::Multiplicity
isAbstract: false
visibility: public
generalizes: [Feature]
specializedBy: [MultiplicityRange]
---

# Multiplicity

`Types` package · concrete metaclass

A Multiplicity is a Feature whose co-domain is a set of natural numbers giving the allowed cardinalities of each typeWithMultiplicity. The cardinality of a Type is defined as follows, depending on whether the Type is a Classifier or Feature.<ul><li>Classifier – The number of basic instances of the Classifier, that is, those instances representing things, which are not instances of any subtypes of the Classifier that are Features.<li>Features – The number of instances with the same featuring instances. In the case of a Feature with a Classifier as its featuringType, this is the number of values of Feature for each basic instance of the Classifier. Note that, for non-unique Features, all duplicate values are included in this count.</li></ul>Multiplicity co-domains (in models) can be specified by Expression that might vary in their results. If the typeWithMultiplicity is a Classifier, the domain of the Multiplicity shall be Base::Anything. If the typeWithMultiplicity is a Feature, the Multiplicity shall have the same domain as the typeWithMultiplicity.

## Generalizations

- [Feature](Feature.md)

## Specializations

- [MultiplicityRange](MultiplicityRange.md)

## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| chainingFeature | [Feature](Feature.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| crossFeature | [Feature](Feature.md) | [0..1] | [Feature](Feature.md) | derived |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
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
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| type | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkMultiplicitySpecialization

A Multiplicity must directly or indirectly specialize the Feature Base::naturals from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Base::naturals')
```

### checkMultiplicityTypeFeaturing

If the owningNamespace of a Multiplicity is a Feature, then the Multiplicity must have the same featuringTypes as that Feature. Otherwise, it must have no featuringTypes (meaning that it is implicitly featured by the base Classifier Anything).

```ocl
if owningNamespace <> null and owningNamespace.oclIsKindOf(Feature) then
    featuringType = 
        owningNamespace.oclAsType(Feature).featuringType
else
    featuringType->isEmpty()
endif
```

