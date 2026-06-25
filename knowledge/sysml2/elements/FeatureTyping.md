---
name: FeatureTyping
package: Features
isAbstract: false
generalizes: [Specialization]
specializedBy: [ConjugatedPortTyping]
---

# FeatureTyping

`Features` package · concrete metaclass

FeatureTyping is Specialization in which the specific Type is a Feature. This means the set of instances of the (specific) typedFeature is a subset of the set of instances of the (general) type. In the simplest case, the type is a Classifier, whereupon the typedFeature has values that are instances of the Classifier.

## Generalizations

- [Specialization](Specialization.md)

## Specializations

- [ConjugatedPortTyping](ConjugatedPortTyping.md)

## Owned features

### owningFeature : Feature [0..1] {derived}

A typedFeature that is also the owningRelatedElement of this FeatureTyping.

Redefines: `owningType`

Subsets: `typedFeature`

### type : Type [1..1]

The Type that is being applied by this FeatureTyping.

Redefines: `general`

### typedFeature : Feature [1..1]

The Feature that has a type determined by this FeatureTyping.

Redefines: `specific`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| general | Type | [1..1] | [Specialization](Specialization.md) |  |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelatedElement | Element | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | Element | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| owningType | Type | [0..1] | [Specialization](Specialization.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| specific | Type | [1..1] | [Specialization](Specialization.md) |  |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
