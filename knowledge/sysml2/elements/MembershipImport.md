---
name: MembershipImport
package: Namespaces
fully qualified name: KerML::Root::Namespaces::MembershipImport
isAbstract: false
visibility: public
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

### importedMembership

`+` [Membership](Membership.md) · `[1..1]`

The Membership to be imported.

Redefines [target](Relationship.md#target)


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
| isImportAll | Boolean | [1..1] | [Import](Import.md) |  |
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
| visibility | VisibilityKind | [1..1] | [Import](Import.md) |  |

## Constraints

### deriveMembershipImportImportedElement

The importedElement of a MembershipImport is the memberElement of its importedMembership.

```ocl
importedElement = importedMembership.memberElement
```

