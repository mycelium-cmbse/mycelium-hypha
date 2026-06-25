---
name: AnnotatingElement
package: Annotations
fully qualified name: KerML::Root::Annotations::AnnotatingElement
isAbstract: false
visibility: public
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

### annotatedElement

`+` [Element](Element.md) · `[1..*]` · *derived, ordered*

The Elements that are annotated by this AnnotatingElement. If annotation is not empty, these are the annotatedElements of the annotations. If annotation is empty, then it is the owningNamespace of the AnnotatingElement.

### annotation

`+` [Annotation](Annotation.md) · `[0..*]` · *derived, ordered*

The Annotations that relate this AnnotatingElement to its annotatedElements. This includes the owningAnnotatingRelationship (if any) followed by all the ownedAnnotatingRelationshps.

Subsets `sourceRelationship`

### ownedAnnotatingRelationship

`+` [Annotation](Annotation.md) · `[0..*]` · *derived, composite, ordered*

The ownedRelationships of this AnnotatingElement that are Annotations, for which this AnnotatingElement is the annotatingElement.

Subsets [annotation](#annotation), [ownedRelationship](Element.md#ownedrelationship)

### owningAnnotatingRelationship

`+` [Annotation](Annotation.md) · `[0..1]` · *derived*

The owningRelationship of this AnnotatingRelationship, if it is an Annotation

Subsets [owningRelationship](Element.md#owningrelationship), [annotation](#annotation)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |

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

