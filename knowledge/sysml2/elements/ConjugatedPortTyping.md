---
name: ConjugatedPortTyping
package: Ports
fully qualified name: SysML::Systems::Ports::ConjugatedPortTyping
isAbstract: false
visibility: public
generalizes: [FeatureTyping]
specializedBy: []
---

# ConjugatedPortTyping

`Ports` package · concrete metaclass

A ConjugatedPortTyping is a FeatureTyping whose type is a ConjugatedPortDefinition. (This relationship is intended to be an abstract-syntax marker for a special surface notation for conjugated typing of ports.)

## Generalizations

- [FeatureTyping](FeatureTyping.md)

## Owned features

### conjugatedPortDefinition

`+` [ConjugatedPortDefinition](ConjugatedPortDefinition.md) · `[1..1]`

The type of this ConjugatedPortTyping considered as a FeatureTyping, which must be a ConjugatedPortDefinition.

Redefines [type](FeatureTyping.md#type)

### portDefinition

`+` [PortDefinition](PortDefinition.md) · `[1..1]` · *derived*

The originalPortDefinition of the conjugatedPortDefinition of this ConjugatedPortTyping.


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
| owningFeature | [Feature](Feature.md) | [0..1] | [FeatureTyping](FeatureTyping.md) | derived |
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
| type | [Type](Type.md) | [1..1] | [FeatureTyping](FeatureTyping.md) |  |
| typedFeature | [Feature](Feature.md) | [1..1] | [FeatureTyping](FeatureTyping.md) |  |

## Constraints

### deriveConjugatedPortTypingPortDefinition

The portDefinition of a ConjugatedPortTyping is the originalPortDefinition of the conjugatedPortDefinition of the ConjugatedPortTyping.

```ocl
portDefinition = conjugatedPortDefinition.originalPortDefinition
```

