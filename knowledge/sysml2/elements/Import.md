---
name: Import
package: Namespaces
fully qualified name: KerML::Root::Namespaces::Import
isAbstract: true
visibility: public
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

### importOwningNamespace

`+` [Namespace](Namespace.md) · `[1..1]` · *derived*

The Namespace into which Memberships are imported by this Import, which must be the owningRelatedElement of the Import.

Redefines [source](Relationship.md#source)

Subsets [owningRelatedElement](Relationship.md#owningrelatedelement)

### importedElement

`+` [Element](Element.md) · `[1..1]` · *derived*

The effectively imported Element for this Import. For a MembershipImport, this is the memberElement of the importedMembership. For a NamespaceImport, it is the importedNamespace.

### isImportAll

`+` Boolean · `[1..1]`

Whether to import memberships without regard to declared visibility.

### isRecursive

`+` Boolean · `[1..1]`

Whether to recursively import Memberships from visible, owned sub-Namespaces.

### visibility

`+` VisibilityKind · `[1..1]`

The visibility level of the imported members from this Import relative to the importOwningNamespace. The default is private.


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

### validateImportTopLevelVisibility

A top-level Import (that is, one that is owned by a root Namespace) must have a visibility of private.

```ocl
importOwningNamespace.owner = null implies 
    visibility = VisibilityKind::private
```

