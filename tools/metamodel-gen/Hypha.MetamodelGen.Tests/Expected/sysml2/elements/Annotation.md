---
name: Annotation
package: Annotations
isAbstract: false
generalizes: [Relationship]
specializedBy: []
---

# Annotation

`Annotations` package · concrete metaclass

An Annotation is a Relationship between an AnnotatingElement and the Element that is annotated by that AnnotatingElement.

## Generalizations

- [Relationship](Relationship.md)

## Owned features

### annotatedElement : Element [1..1]

The Element that is annotated by the annotatingElement of this Annotation.

Redefines: `target`

### annotatingElement : AnnotatingElement [1..1] {derived}

The AnnotatingElement that annotates the annotatedElement of this Annotation. This is always either the ownedAnnotatingElement or the owningAnnotatingElement.

Redefines: `source`

### ownedAnnotatingElement : AnnotatingElement [0..1] {derived, composite}

The annotatingElement of this Annotation, when it is an ownedRelatedElement.

Subsets: `annotatingElement`, `ownedRelatedElement`

### owningAnnotatedElement : Element [0..1] {derived}

The annotatedElement of this Annotation, when it is also the owningRelatedElement.

Subsets: `annotatedElement`, `owningRelatedElement`

### owningAnnotatingElement : AnnotatingElement [0..1] {derived}

The annotatingElement of this Annotation, when it is the owningRelatedElement.

Subsets: `annotatingElement`, `owningRelatedElement`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
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
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |

## Constraints

### deriveAnnotationAnnotatingElement

The annotatingElement of an Annotation is either its ownedAnnotatingElement or its owningAnnotatingElement.

```ocl
annotatingElement =
    if ownedAnnotatingElement <> null then ownedAnnotatingElement
    else owningAnnotatingElement
    endif
```

### deriveAnnotationOwnedAnnotatingElement

The ownedAnnotatingElement of an Annotation is the first ownedRelatedElement that is an AnnotatingElement, if any.

```ocl
ownedAnnotatingElement =
    let ownedAnnotatingElements : Sequence(AnnotatingElement) = 
        ownedRelatedElement->selectByKind(AnnotatingElement) in
    if ownedAnnotatingElements->isEmpty() then null
    else ownedAnnotatingElements->first()
    endif
```

### validateAnnotationAnnotatedElementOwnership

An Annotation owns its annotatingElement if and only if it is owned by its annotatedElement.

```ocl
(owningAnnotatedElement <> null) = (ownedAnnotatingElement <> null)
```

### validateAnnotationAnnotatingElement

Either the ownedAnnotatingElement of an Annotation must be non-null, or the owningAnnotatingElement must be non-null, but not both.

```ocl
ownedAnnotatingElement <> null xor owningAnnotatingElement <> null
```

