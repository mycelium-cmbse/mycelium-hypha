---
name: Class
package: Classes
isAbstract: false
generalizes: [Classifier]
specializedBy: [Behavior, OccurrenceDefinition, Structure]
---

# Class

`Classes` package · concrete metaclass

A Class is a Classifier of things (in the universe) that can be distinguished without regard to how they are related to other things (via Features). This means multiple things classified by the same Class can be distinguished, even when they are related other things in exactly the same way.

## Generalizations

- [Classifier](Classifier.md)

## Specializations

- [Behavior](Behavior.md)
- [OccurrenceDefinition](OccurrenceDefinition.md)
- [Structure](Structure.md)

## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| differencingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| endFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| feature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, ordered |
| importedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | Membership | [0..*] | [Type](Type.md) | derived, ordered |
| input | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | Boolean | [1..1] | [Type](Type.md) |  |
| isConjugated | Boolean | [1..1] | [Type](Type.md) | derived |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isSufficient | Boolean | [1..1] | [Type](Type.md) |  |
| member | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | Multiplicity | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| output | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedConjugator | Conjugation | [0..1] | [Type](Type.md) | derived, composite |
| ownedDifferencing | Differencing | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedDisjoining | Disjoining | [0..*] | [Type](Type.md) | derived, composite |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedEndFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeatureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedImport | Import | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedIntersecting | Intersecting | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedMember | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | Specialization | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubclassification | Subclassification | [0..*] | [Classifier](Classifier.md) | derived, composite |
| ownedUnioning | Unioning | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkClassSpecialization

A Class must directly or indirectly specialize the base Class Occurrences::Occurrence from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Occurrences::Occurrence')
```

### validateClassSpecialization

A Class must not specialize a DataType and it can only specialize an Association if it is also itself a kind of Association (such as an AssociationStructure or Interaction).

```ocl
ownedSpecialization.general->
    forAll(not oclIsKindOf(DataType)) and
not oclIsKindOf(Association) implies
    ownedSpecialization.general->
        forAll(not oclIsKindOf(Association))
```

