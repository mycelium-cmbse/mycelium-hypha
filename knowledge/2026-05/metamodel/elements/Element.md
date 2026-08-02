---
name: Element
package: Elements
fully qualified name: KerML::Root::Elements::Element
isAbstract: true
visibility: public
generalizes: []
specializedBy: [AnnotatingElement, Namespace, Relationship]
---

# Element

`Elements` package · abstract metaclass

An Element is a constituent of a model that is uniquely identified relative to all other Elements. It can have Relationships with other Elements. Some of these Relationships might imply ownership of other Elements, which means that if an Element is deleted from a model, then so are all the Elements that it owns.

## Specializations

- [AnnotatingElement](AnnotatingElement.md)
- [Namespace](Namespace.md)
- [Relationship](Relationship.md)

## Owned features

### aliasIds

`+` [String](String.md) · `[0..*]` · *ordered*

Various alternative identifiers for this Element. Generally, these will be set by tools.

### declaredName

`+` [String](String.md) · `[0..1]`

The declared name of this Element.

### declaredShortName

`+` [String](String.md) · `[0..1]`

An optional alternative name for the Element that is intended to be shorter or in some way more succinct than its primary name. It may act as a modeler-specified identifier for the Element, though it is then the responsibility of the modeler to maintain the uniqueness of this identifier within a model or relative to some other context.

### documentation

`+` [Documentation](Documentation.md) · `[0..*]` · *derived, ordered*

The Documentation owned by this Element.

Subsets `annotatingElement`, [ownedElement](#ownedelement)

### elementId

`+` [String](String.md) · `[1..1]`

The globally unique identifier for this Element. This is intended to be set by tooling, and it must not change during the lifetime of the Element.

### isImpliedIncluded

`+` [Boolean](Boolean.md) · `[1..1]`

Whether all necessary implied Relationships have been included in the ownedRelationships of this Element. This property may be true, even if there are not actually any ownedRelationships with isImplied = true, meaning that no such Relationships are actually implied for this Element. However, if it is false, then ownedRelationships may not contain any implied Relationships. That is, either all required implied Relationships must be included, or none of them.

### isLibraryElement

`+` [Boolean](Boolean.md) · `[1..1]` · *derived*

Whether this Element is contained in the ownership tree of a library model.

### name

`+` [String](String.md) · `[0..1]` · *derived*

The name to be used for this Element during name resolution within its owningNamespace. This is derived using the effectiveName() operation. By default, it is the same as the declaredName, but this is overridden for certain kinds of Elements to compute a name even when the declaredName is null.

### ownedAnnotation

`+` [Annotation](Annotation.md) · `[0..*]` · *derived, composite, ordered*

The ownedRelationships of this Element that are Annotations, for which this Element is the annotatedElement.

Subsets [ownedRelationship](#ownedrelationship), `annotation`

### ownedElement

`+` [Element](Element.md) · `[0..*]` · *derived, ordered*

The Elements owned by this Element, derived as the ownedRelatedElements of the ownedRelationships of this Element.

### ownedRelationship

`+` [Relationship](Relationship.md) · `[0..*]` · *composite, ordered*

The Relationships for which this Element is the owningRelatedElement.

Subsets `relationship`

### owner

`+` [Element](Element.md) · `[0..1]` · *derived*

The owner of this Element, derived as the owningRelatedElement of the owningRelationship of this Element, if any.

### owningMembership

`+` [OwningMembership](OwningMembership.md) · `[0..1]` · *derived*

The owningRelationship of this Element, if that Relationship is a Membership.

Subsets `membership`, [owningRelationship](#owningrelationship)

### owningNamespace

`+` [Namespace](Namespace.md) · `[0..1]` · *derived*

The Namespace that owns this Element, which is the membershipOwningNamespace of the owningMembership of this Element, if any.

Subsets `namespace`

### owningRelationship

`+` [Relationship](Relationship.md) · `[0..1]`

The Relationship for which this Element is an ownedRelatedElement, if any.

Subsets `relationship`

### qualifiedName

`+` [String](String.md) · `[0..1]` · *derived*

The full ownership-qualified name of this Element, represented in a form that is valid according to the KerML textual concrete syntax for qualified names (including use of unrestricted name notation and escaped characters, as necessary). The qualifiedName is null if this Element has no owningNamespace or if there is not a complete ownership chain of named Namespaces from a root Namespace to this Element. If the owningNamespace has other Elements with the same name as this one, then the qualifiedName is null for all such Elements other than the first.

### shortName

`+` [String](String.md) · `[0..1]` · *derived*

The short name to be used for this Element during name resolution within its owningNamespace. This is derived using the effectiveShortName() operation. By default, it is the same as the declaredShortName, but this is overridden for certain kinds of Elements to compute a shortName even when the declaredName is null.

### textualRepresentation

`+` [TextualRepresentation](TextualRepresentation.md) · `[0..*]` · *derived, ordered*

The TextualRepresentations that annotate this Element.

Subsets `annotatingElement`, [ownedElement](#ownedelement)


## Constraints

### deriveElementDocumentation

The documentation of an Element is its ownedElements that are Documentation.

```ocl
documentation = ownedElement->selectByKind(Documentation)
```

### deriveElementIsLibraryElement

An Element isLibraryElement if libraryNamespace() is not null.

```ocl
isLibraryElement = libraryNamespace() <> null
```

### deriveElementName

The name of an Element is given by the result of the effectiveName() operation.

```ocl
name = effectiveName()
```

### deriveElementOwnedAnnotation

The ownedAnnotations of an Element are its ownedRelationships that are Annotations, for which the Element is the annotatedElement.

```ocl
ownedAnnotation = ownedRelationship->
    selectByKind(Annotation)->
    select(a | a.annotatedElement = self)
```

### deriveElementOwnedElement

The ownedElements of an Element are the ownedRelatedElements of its ownedRelationships.

```ocl
ownedElement = ownedRelationship.ownedRelatedElement
```

### deriveElementOwner

The owner of an Element is the owningRelatedElement of its owningRelationship.

```ocl
owner = owningRelationship.owningRelatedElement
```

### deriveElementOwningNamespace

The owningNamespace of an Element is the membershipOwningNamespace of its owningMembership (if any).

```ocl
owningNamespace =
    if owningMembership = null then null
    else owningMembership.membershipOwningNamespace
    endif
```

### deriveElementQualifiedName

If this Element does not have an owningNamespace, then its qualifiedName is null. If the owningNamespace of this Element is a root Namespace, then the qualifiedName of the Element is the escaped name of the Element (if any). If the owningNamespace is non-null but not a root Namespace, then the qualifiedName of this Element is constructed from the qualifiedName of the owningNamespace and the escaped name of the Element, unless the qualifiedName of the owningNamespace is null or the escaped name is null, in which case the qualifiedName of this Element is also null. Further, if the owningNamespace has other ownedMembers with the same non-null name as this Element, and this Element is not the first, then the qualifiedName of this Element is null.

```ocl
qualifiedName =
    if owningNamespace = null then null
    else if name <> null and 
        owningNamespace.ownedMember->
        select(m | m.name = name).indexOf(self) <> 1 then null
    else if owningNamespace.owner = null then escapedName()
    else if owningNamespace.qualifiedName = null or 
            escapedName() = null then null
    else owningNamespace.qualifiedName + '::' + escapedName()
    endif endif endif endif
```

### deriveElementShortName

The shortName of an Element is given by the result of the effectiveShortName() operation.

```ocl
shortName = effectiveShortName()
```

### deriveElementTextualRepresentation

The textualRepresentations of an Element are its ownedElements that are TextualRepresentations.

```ocl
textualRepresentation = ownedElement->selectByKind(TextualRepresentation)
```

### validateElementIsImpliedIncluded

If an Element has any ownedRelationships for which isImplied = true, then the Element must also have isImpliedIncluded = true. (Note that an Element can have isImplied = true even if no ownedRelationships have isImplied = true, indicating the Element simply has no implied Relationships.

```ocl
ownedRelationship->exists(isImplied) implies isImpliedIncluded
```

