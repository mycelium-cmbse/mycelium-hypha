---
name: Subsetting
package: Features
fully qualified name: KerML::Core::Features::Subsetting
isAbstract: false
visibility: public
generalizes: [Specialization]
specializedBy: [CrossSubsetting, Redefinition, ReferenceSubsetting]
---

# Subsetting

`Features` package · concrete metaclass

Subsetting is Specialization in which the specific and general Types are Features. This means all values of the subsettingFeature (on instances of its domain, i.e., the intersection of its featuringTypes) are values of the subsettedFeature on instances of its domain. To support this the domain of the subsettingFeature must be the same or specialize (at least indirectly) the domain of the subsettedFeature (via Specialization), and the co-domain (intersection of the types) of the subsettingFeature must specialize the co-domain of the subsettedFeature.

## Generalizations

- [Specialization](Specialization.md)

## Specializations

- [CrossSubsetting](CrossSubsetting.md)
- [Redefinition](Redefinition.md)
- [ReferenceSubsetting](ReferenceSubsetting.md)

## Owned features

### owningFeature

`+` [Feature](Feature.md) · `[0..1]` · *derived*

A subsettingFeature that is also the owningRelatedElement of this Subsetting.

Redefines [owningType](Specialization.md#owningtype)

Subsets [subsettingFeature](#subsettingfeature)

### subsettedFeature

`+` [Feature](Feature.md) · `[1..1]`

The Feature that is subsetted by the subsettingFeature of this Subsetting.

Redefines [general](Specialization.md#general)

### subsettingFeature

`+` [Feature](Feature.md) · `[1..1]`

The Feature that is a subset of the subsettedFeature of this Subsetting.

Redefines [specific](Specialization.md#specific)


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
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |

## Constraints

### validateSubsettingConstantConformance

If the subsettedFeature of a Subsetting has isConstant = true and the subsettingFeature has isVariable = true, then the subsettingFeature must have isConstant = true.

```ocl
subsettedFeature.isConstant and subsettingFeature.isVariable implies 
    subsettingFeature.isConstant
```

### validateSubsettingFeaturingTypes

The subsettedFeature must be accessible by the subsettingFeature.

```ocl
subsettingFeature.canAccess(subsettedFeature)
```

### validateSubsettingUniquenessConformance

If the subsettedFeature of a Subsetting has isUnique = true, then the subsettingFeature must have isUnique = true.

```ocl
subsettedFeature.isUnique implies subsettingFeature.isUnique
```

