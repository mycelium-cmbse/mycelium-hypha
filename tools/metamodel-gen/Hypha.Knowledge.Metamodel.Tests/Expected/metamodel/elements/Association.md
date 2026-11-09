---
name: Association
package: Associations
fully qualified name: KerML::Kernel::Associations::Association
isAbstract: false
visibility: public
generalizes: [Classifier, Relationship]
specializedBy: [AssociationStructure, Interaction]
---

# Association

`Associations` package · concrete metaclass

An Association is a Relationship and a Classifier to enable classification of links between things (in the universe). The co-domains (types) of the associationEnd Features are the relatedTypes, as co-domain and participants (linked things) of an Association identify each other.

## Generalizations

- [Classifier](Classifier.md)
- [Relationship](Relationship.md)

## Specializations

- [AssociationStructure](AssociationStructure.md)
- [Interaction](Interaction.md)

## Owned features

### associationEnd

`+` [Feature](Feature.md) · `[0..*]` · *derived*

The features of the Association that identify the things that can be related by it. A concrete Association must have at least two associationEnds. When it has exactly two, the Association is called a binary Association.

Redefines [endFeature](Type.md#endfeature)

### relatedType

`+` [Type](Type.md) · `[0..*]` · *derived, ordered*

The types of the associationEnds of the Association, which are the relatedElements of the Association considered as a Relationship.

Redefines [relatedElement](Relationship.md#relatedelement)

### sourceType

`+` [Type](Type.md) · `[0..1]` · *derived*

The source relatedType for this Association. It is the first relatedType of the Association.

Redefines [source](Relationship.md#source)

Subsets [relatedType](#relatedtype)

### targetType

`+` [Type](Type.md) · `[0..*]` · *derived*

The target relatedTypes for this Association. This includes all the relatedTypes other than the sourceType.

Redefines [target](Relationship.md#target)

Subsets [relatedType](#relatedtype)


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
| isImplied | [Boolean](Boolean.md) | [1..1] | [Relationship](Relationship.md) |  |
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
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | [Specialization](Specialization.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubclassification | [Subclassification](Subclassification.md) | [0..*] | [Classifier](Classifier.md) | derived, composite |
| ownedUnioning | [Unioning](Unioning.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkAssociationBinarySpecialization

A binary Association must directly or indirectly specialize the base Association Links::binaryLink from the Kernel Semantic Library.

```ocl
associationEnd->size() = 2 implies
    specializesFromLibrary('Links::BinaryLink')
```

### checkAssociationSpecialization

An Association must directly or indirectly specialize the base Association Links::Link from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Links::Link')
```

### deriveAssociationRelatedType

The relatedTypes of an Association are the types of its associationEnds.

```ocl
relatedType = associationEnd.type
```

### deriveAssociationSourceType

The sourceType of an Association is its first relatedType (if any).

```ocl
sourceType =
    if relatedType->isEmpty() then null
    else relatedType->first() endif
```

### deriveAssociationTargetType

```ocl
targetType =
    if relatedType->size() < 2 then OrderedSet{}
    else 
        relatedType->
            subSequence(2, relatedType->size())->
            asOrderedSet() 
    endif
```

### validateAssociationBinarySpecialization

If an Association has more than two associationEnds, then it must not specialize, directly or indirectly, the Association BinaryLink from the Kernel Semantic Library.

```ocl
associationEnds->size() > 2 implies
    not specializesFromLibrary('Links::BinaryLink')
```

### validateAssociationEndTypes

The ownedEndFeatures of an Association must have exactly one type.

```ocl
ownedEndFeature->forAll(type->size() = 1)
```

### validateAssociationRelatedTypes

If an Association is concrete (not abstract), then it must have at least two relatedTypes.

```ocl
not isAbstract implies relatedType->size() >= 2
```

### validateAssociationStructureIntersection

If an Association is also a kind of Structure, then it must be an AssociationStructure.

```ocl
oclIsKindOf(Structure) = oclIsKindOf(AssociationStructure)
```

