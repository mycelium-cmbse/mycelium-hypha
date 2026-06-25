---
name: Expose
package: Views
fully qualified name: SysML::Systems::Views::Expose
isAbstract: true
visibility: public
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

### isImportAll

`+` Boolean · `[1..1]`

An Expose always imports all Elements, regardless of visibility (isImportAll = true).

Redefines [isImportAll](#isimportall)

### visibility

`+` VisibilityKind · `[1..1]`

An Expose always has protected visibility.

Redefines [visibility](#visibility)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| importOwningNamespace | [Namespace](Namespace.md) | [1..1] | [Import](Import.md) | derived |
| importedElement | [Element](Element.md) | [1..1] | [Import](Import.md) | derived |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isRecursive | Boolean | [1..1] | [Import](Import.md) |  |
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

