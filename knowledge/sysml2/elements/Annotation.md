---
name: Annotation
package: Annotations
fully qualified name: KerML::Root::Annotations::Annotation
isAbstract: false
visibility: public
generalizes: [Relationship]
specializedBy: []
---

# Annotation

`Annotations` package · concrete metaclass

An Annotation is a Relationship between an AnnotatingElement and the Element that is annotated by that AnnotatingElement.

## Generalizations

- [Relationship](Relationship.md)

## Owned features

### annotatedElement

`+` [Element](Element.md) · `[1..1]`

The Element that is annotated by the annotatingElement of this Annotation.

Redefines [target](Relationship.md#target)

### annotatingElement

`+` [AnnotatingElement](AnnotatingElement.md) · `[1..1]` · *derived*

The AnnotatingElement that annotates the annotatedElement of this Annotation. This is always either the ownedAnnotatingElement or the owningAnnotatingElement.

Redefines [source](Relationship.md#source)

### ownedAnnotatingElement

`+` [AnnotatingElement](AnnotatingElement.md) · `[0..1]` · *derived, composite*

The annotatingElement of this Annotation, when it is an ownedRelatedElement.

Subsets [annotatingElement](#annotatingelement), [ownedRelatedElement](Relationship.md#ownedrelatedelement)

### owningAnnotatedElement

`+` [Element](Element.md) · `[0..1]` · *derived*

The annotatedElement of this Annotation, when it is also the owningRelatedElement.

Subsets [annotatedElement](#annotatedelement), [owningRelatedElement](Relationship.md#owningrelatedelement)

### owningAnnotatingElement

`+` [AnnotatingElement](AnnotatingElement.md) · `[0..1]` · *derived*

The annotatingElement of this Annotation, when it is the owningRelatedElement.

Subsets [annotatingElement](#annotatingelement), [owningRelatedElement](Relationship.md#owningrelatedelement)


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

