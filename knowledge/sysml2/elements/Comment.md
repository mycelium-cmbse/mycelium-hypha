---
name: Comment
package: Annotations
fully qualified name: KerML::Root::Annotations::Comment
isAbstract: false
visibility: public
generalizes: [AnnotatingElement]
specializedBy: [Documentation]
---

# Comment

`Annotations` package · concrete metaclass

A Comment is an AnnotatingElement whose body in some way describes its annotatedElements.

## Generalizations

- [AnnotatingElement](AnnotatingElement.md)

## Specializations

- [Documentation](Documentation.md)

## Owned features

### body

`+` [String](String.md) · `[1..1]`

The annotation text for the Comment.

### locale

`+` [String](String.md) · `[0..1]`

Identification of the language of the body text and, optionally, the region and/or encoding. The format shall be a POSIX locale conformant to ISO/IEC 15897, with the format [language[_territory][.codeset][@modifier]].


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| annotatedElement | [Element](Element.md) | [1..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| annotation | [Annotation](Annotation.md) | [0..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
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
