---
name: Import
package: Namespaces
isAbstract: true
generalizes: [Relationship]
specializedBy: [Expose, MembershipImport, NamespaceImport]
---

# Import

`Namespaces` package · abstract metaclass

An Import is an Relationship between its importOwningNamespace and either a Membership (for a MembershipImport) or another Namespace (for a NamespaceImport), which determines a set of Memberships that become importedMemberships of the importOwningNamespace. If isImportAll = false (the default), then only public Memberships are considered &quot;visible&quot;. If isImportAll = true, then all Memberships are considered &quot;visible&quot;, regardless of their declared visibility. If isRecursive = true, then visible Memberships are also recursively imported from owned sub-Namespaces.

## Generalizations

- [Relationship](Relationship.md)

## Specializations

- [Expose](Expose.md)
- [MembershipImport](MembershipImport.md)
- [NamespaceImport](NamespaceImport.md)

## Owned features

### importOwningNamespace : Namespace [1..1] {derived}

The Namespace into which Memberships are imported by this Import, which must be the owningRelatedElement of the Import.

Redefines: `source`

Subsets: `owningRelatedElement`

### importedElement : Element [1..1] {derived}

The effectively imported Element for this Import. For a MembershipImport, this is the memberElement of the importedMembership. For a NamespaceImport, it is the importedNamespace.

### isImportAll : Boolean [1..1]

Whether to import memberships without regard to declared visibility.

### isRecursive : Boolean [1..1]

Whether to recursively import Memberships from visible, owned sub-Namespaces.

### visibility : VisibilityKind [1..1]

The visibility level of the imported members from this Import relative to the importOwningNamespace. The default is private.


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

### validateImportTopLevelVisibility

A top-level Import (that is, one that is owned by a root Namespace) must have a visibility of private.

```ocl
importOwningNamespace.owner = null implies 
    visibility = VisibilityKind::private
```

