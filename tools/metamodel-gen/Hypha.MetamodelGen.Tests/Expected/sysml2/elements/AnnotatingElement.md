---
name: AnnotatingElement
package: Annotations
isAbstract: false
generalizes: [Element]
specializedBy: [Comment, MetadataFeature, TextualRepresentation]
---

# AnnotatingElement

`Annotations` package · concrete metaclass

An AnnotatingElement is an Element that provides additional description of or metadata on some other Element. An AnnotatingElement is either attached to its annotatedElements by Annotation Relationships, or it implicitly annotates its owningNamespace.

## Generalizations

- [Element](Element.md)

## Specializations

- [Comment](Comment.md)
- [MetadataFeature](MetadataFeature.md)
- [TextualRepresentation](TextualRepresentation.md)

## Owned features

### annotatedElement : Element [1..*] {derived, ordered}

The Elements that are annotated by this AnnotatingElement. If annotation is not empty, these are the annotatedElements of the annotations. If annotation is empty, then it is the owningNamespace of the AnnotatingElement.

### annotation : Annotation [0..*] {derived, ordered}

The Annotations that relate this AnnotatingElement to its annotatedElements. This includes the owningAnnotatingRelationship (if any) followed by all the ownedAnnotatingRelationshps.

Subsets: `sourceRelationship`

### ownedAnnotatingRelationship : Annotation [0..*] {derived, composite, ordered}

The ownedRelationships of this AnnotatingElement that are Annotations, for which this AnnotatingElement is the annotatingElement.

Subsets: `annotation`, `ownedRelationship`

### owningAnnotatingRelationship : Annotation [0..1] {derived}

The owningRelationship of this AnnotatingRelationship, if it is an Annotation

Subsets: `owningRelationship`, `annotation`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |

## Constraints

### deriveAnnotatingElementAnnotatedElement

If an AnnotatingElement has annotations, then its annotatedElements are the annotatedElements of all its annotations. Otherwise, it's single annotatedElement is its owningNamespace.

```ocl
annotatedElement = 
 if annotation->notEmpty() then annotation.annotatedElement
 else Sequence{owningNamespace} endif
```

### deriveAnnotatingElementAnnotation

The annotations of an AnnotatingElement are its owningAnnotatingRelationship (if any) followed by all its ownedAnnotatingRelationships.

```ocl
annotation = 
    if owningAnnotatingRelationship = null then ownedAnnotatingRelationship
    else owningAnnotatingRelationship->prepend(owningAnnotatingRelationship)
    endif
```

### deriveAnnotatingElementOwnedAnnotatingRelationship

The ownedAnnotatingRelationships of an AnnotatingElement are its ownedRelationships that are Annotations, for which the AnnotatingElement is not the annotatedElement.

```ocl
ownedAnnotatingRelationship = ownedRelationship->
    selectByKind(Annotation)->
    select(a | a.annotatedElement <> self)
```

