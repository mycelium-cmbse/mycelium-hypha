---
name: Expose
package: Views
isAbstract: true
generalizes: [Import]
specializedBy: [MembershipExpose, NamespaceExpose]
---

# Expose

`Views` package · abstract metaclass

An Expose is an Import of Memberships into a ViewUsage that provide the Elements to be included in a view. Visibility is always ignored for an Expose (i.e., isImportAll = true).

## Generalizations

- [Import](Import.md)

## Specializations

- [MembershipExpose](MembershipExpose.md)
- [NamespaceExpose](NamespaceExpose.md)

## Owned features

### isImportAll : Boolean [1..1]

An Expose always imports all Elements, regardless of visibility (isImportAll = true).

Redefines: `isImportAll`

### visibility : VisibilityKind [1..1]

An Expose always has protected visibility.

Redefines: `visibility`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| importOwningNamespace | Namespace | [1..1] | [Import](Import.md) | derived |
| importedElement | Element | [1..1] | [Import](Import.md) | derived |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isRecursive | Boolean | [1..1] | [Import](Import.md) |  |
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

### validateExposeIsImportAll

An Expose always imports all Elements, regardless of visibility.

```ocl
isImportAll
```

### validateExposeOwningNamespace

The importOwningNamespace of an Expose must be a ViewUsage.

```ocl
importOwningNamespace.oclIsType(ViewUsage)
```

### validateExposeVisibility

An Expose always has protected visibility.

```ocl
visibility = VisibilityKind::protected
```

