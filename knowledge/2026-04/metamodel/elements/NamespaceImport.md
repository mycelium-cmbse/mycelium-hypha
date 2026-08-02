---
name: NamespaceImport
package: Namespaces
fully qualified name: KerML::Root::Namespaces::NamespaceImport
isAbstract: false
visibility: public
generalizes: [Import]
specializedBy: [NamespaceExpose]
---

# NamespaceImport

`Namespaces` package · concrete metaclass

A NamespaceImport is an Import that imports Memberships from its importedNamespace into the importOwningNamespace. If isRecursive = false, then only the visible Memberships of the importedNamespace are imported. If isRecursive = true, then, in addition, Memberships are recursively imported from any ownedMembers of the importedNamespace that are Namespaces.

## Generalizations

- [Import](Import.md)

## Specializations

- [NamespaceExpose](NamespaceExpose.md)

## Owned features

### importedNamespace

`+` [Namespace](Namespace.md) · `[1..1]`

The Namespace whose visible Memberships are imported by this NamespaceImport.

Redefines [target](Relationship.md#target)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| importOwningNamespace | [Namespace](Namespace.md) | [1..1] | [Import](Import.md) | derived |
| importedElement | [Element](Element.md) | [1..1] | [Import](Import.md) | derived |
| isImplied | [Boolean](Boolean.md) | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isImportAll | [Boolean](Boolean.md) | [1..1] | [Import](Import.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| isRecursive | [Boolean](Boolean.md) | [1..1] | [Import](Import.md) |  |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| visibility | [VisibilityKind](VisibilityKind.md) | [1..1] | [Import](Import.md) |  |

## Constraints

### deriveNamespaceImportImportedElement

The importedElement of a NamespaceImport is its importedNamespace.

```ocl
importedElement = importedNamespace
```

