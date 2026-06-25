---
name: Conjugation
package: Types
fully qualified name: KerML::Core::Types::Conjugation
isAbstract: false
visibility: public
generalizes: [Relationship]
specializedBy: [PortConjugation]
---

# Conjugation

`Types` package · concrete metaclass

Conjugation is a Relationship between two types in which the conjugatedType inherits all the Features of the originalType, but with all input and output Features reversed. That is, any Features with a direction in relative to the originalType are considered to have an effective direction of out relative to the conjugatedType and, similarly, Features with direction out in the originalType are considered to have an effective direction of in in the conjugatedType. Features with direction inout, or with no direction, in the originalType, are inherited without change.A Type may participate as a conjugatedType in at most one Conjugation relationship, and such a Type may not also be the specific Type in any Specialization relationship.

## Generalizations

- [Relationship](Relationship.md)

## Specializations

- [PortConjugation](PortConjugation.md)

## Owned features

### conjugatedType

`+` [Type](Type.md) · `[1..1]`

The Type that is the result of applying Conjugation to the originalType.

Redefines [source](Relationship.md#source)

### originalType

`+` [Type](Type.md) · `[1..1]`

The Type to be conjugated.

Redefines [target](Relationship.md#target)

### owningType

`+` [Type](Type.md) · `[0..1]` · *derived*

The conjugatedType of this Conjugation that is also its owningRelatedElement.

Subsets [conjugatedType](#conjugatedtype), [owningRelatedElement](Relationship.md#owningrelatedelement)


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
