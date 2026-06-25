---
name: MembershipImport
package: Namespaces
isAbstract: false
generalizes: [Import]
specializedBy: [MembershipExpose]
---

# MembershipImport

`Namespaces` package · concrete metaclass

A MembershipImport is an Import that imports its importedMembership into the importOwningNamespace. If isRecursive = true and the memberElement of the importedMembership is a Namespace, then the equivalent of a recursive NamespaceImport is also performed on that Namespace.

## Generalizations

- [Import](Import.md)

## Specializations

- [MembershipExpose](MembershipExpose.md)

## Owned features

### importedMembership : Membership [1..1]

The Membership to be imported.

Redefines: `target`


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
| isImportAll | Boolean | [1..1] | [Import](Import.md) |  |
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
| visibility | VisibilityKind | [1..1] | [Import](Import.md) |  |

## Constraints

### deriveMembershipImportImportedElement

The importedElement of a MembershipImport is the memberElement of its importedMembership.

```ocl
importedElement = importedMembership.memberElement
```

