---
name: Documentation
package: Annotations
isAbstract: false
generalizes: [Comment]
specializedBy: []
---

# Documentation

`Annotations` package · concrete metaclass

Documentation is a Comment that specifically documents a documentedElement, which must be its owner.

## Generalizations

- [Comment](Comment.md)

## Owned features

### documentedElement : Element [1..1] {derived}

The Element that is documented by this Documentation.

Redefines: `annotatedElement`

Subsets: `owner`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| annotatedElement | Element | [1..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| annotation | Annotation | [0..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| body | String | [1..1] | [Comment](Comment.md) |  |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| locale | String | [0..1] | [Comment](Comment.md) |  |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotatingRelationship | Annotation | [0..*] | [AnnotatingElement](AnnotatingElement.md) | derived, composite, ordered |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningAnnotatingRelationship | Annotation | [0..1] | [AnnotatingElement](AnnotatingElement.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
