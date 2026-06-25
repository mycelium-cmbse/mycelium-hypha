---
name: Disjoining
package: Types
fully qualified name: KerML::Core::Types::Disjoining
isAbstract: false
visibility: public
generalizes: [Relationship]
specializedBy: []
---

# Disjoining

`Types` package · concrete metaclass

A Disjoining is a Relationship between Types asserted to have interpretations that are not shared (disjoint) between them, identified as typeDisjoined and disjoiningType. For example, a Classifier for mammals is disjoint from a Classifier for minerals, and a Feature for people&#39;s parents is disjoint from a Feature for their children.

## Generalizations

- [Relationship](Relationship.md)

## Owned features

### disjoiningType

`+` [Type](Type.md) · `[1..1]`

Type asserted to be disjoint with the typeDisjoined.

Redefines [target](Relationship.md#target)

### owningType

`+` [Type](Type.md) · `[0..1]` · *derived*

A typeDisjoined that is also an owningRelatedElement.

Subsets [owningRelatedElement](Relationship.md#owningrelatedelement), [typeDisjoined](#typedisjoined)

### typeDisjoined

`+` [Type](Type.md) · `[1..1]`

Type asserted to be disjoint with the disjoiningType.

Redefines [source](Relationship.md#source)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
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
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
