---
name: Comment
package: Annotations
isAbstract: false
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

### body : String [1..1]

The annotation text for the Comment.

### locale : String [0..1]

Identification of the language of the body text and, optionally, the region and/or encoding. The format shall be a POSIX locale conformant to ISO/IEC 15897, with the format [language[_territory][.codeset][@modifier]].


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| annotatedElement | Element | [1..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| annotation | Annotation | [0..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
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
