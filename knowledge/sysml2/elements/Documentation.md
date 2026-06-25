---
name: Documentation
package: Annotations
fully qualified name: KerML::Root::Annotations::Documentation
isAbstract: false
visibility: public
generalizes: [Comment]
specializedBy: []
---

# Documentation

`Annotations` package · concrete metaclass

Documentation is a Comment that specifically documents a documentedElement, which must be its owner.

## Generalizations

- [Comment](Comment.md)

## Owned features

### documentedElement

`+` [Element](Element.md) · `[1..1]` · *derived*

The Element that is documented by this Documentation.

Redefines [annotatedElement](AnnotatingElement.md#annotatedelement)

Subsets [owner](Element.md#owner)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| annotatedElement | [Element](Element.md) | [1..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| annotation | [Annotation](Annotation.md) | [0..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| body | [String](String.md) | [1..1] | [Comment](Comment.md) |  |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| locale | [String](String.md) | [0..1] | [Comment](Comment.md) |  |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| ownedAnnotatingRelationship | [Annotation](Annotation.md) | [0..*] | [AnnotatingElement](AnnotatingElement.md) | derived, composite, ordered |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningAnnotatingRelationship | [Annotation](Annotation.md) | [0..1] | [AnnotatingElement](AnnotatingElement.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
