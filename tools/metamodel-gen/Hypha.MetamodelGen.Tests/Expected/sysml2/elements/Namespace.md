---
name: Namespace
package: Namespaces
isAbstract: false
generalizes: [Element]
specializedBy: [Package, Type]
---

# Namespace

`Namespaces` package · concrete metaclass

A Namespace is an Element that contains other Elements, known as its members, via Membership Relationships with those Elements. The members of a Namespace may be owned by the Namespace, aliased in the Namespace, or imported into the Namespace via Import Relationships.A Namespace can provide names for its members via the memberNames and memberShortNames specified by the Memberships in the Namespace. If a Membership specifies a memberName and/or memberShortName, then those are names of the corresponding memberElement relative to the Namespace. For an OwningMembership, the ownedMemberName and ownedMemberShortName are given by the Element name and shortName. Note that the same Element may be the memberElement of multiple Memberships in a Namespace (though it may be owned at most once), each of which may define a separate alias for the Element relative to the Namespace.

## Generalizations

- [Element](Element.md)

## Specializations

- [Package](Package.md)
- [Type](Type.md)

## Owned features

### importedMembership : Membership [0..*] {derived, ordered}

The Memberships in this Namespace that result from the ownedImports of this Namespace.

Subsets: `membership`

### member : Element [0..*] {derived, ordered}

The set of all member Elements of this Namespace, which are the memberElements of all memberships of the Namespace.

### membership : Membership [0..*] {derived, ordered}

All Memberships in this Namespace, including (at least) the union of ownedMemberships and importedMemberships.

### ownedImport : Import [0..*] {derived, composite, ordered}

The ownedRelationships of this Namespace that are Imports, for which the Namespace is the importOwningNamespace.

Subsets: `ownedRelationship`, `sourceRelationship`

### ownedMember : Element [0..*] {derived, ordered}

The owned members of this Namespace, which are the <cpde>ownedMemberElements of the ownedMemberships of the Namespace.</cpde>

Subsets: `member`

### ownedMembership : Membership [0..*] {derived, composite, ordered}

The ownedRelationships of this Namespace that are Memberships, for which the Namespace is the membershipOwningNamespace.

Subsets: `membership`, `sourceRelationship`, `ownedRelationship`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |

## Constraints

### deriveNamespaceImportedMembership

The importedMemberships of a Namespace are derived using the importedMemberships() operation, with no initially excluded Namespaces.

```ocl
importedMembership = importedMemberships(Set{})
```

### deriveNamespaceMembers

The members of a Namespace are the memberElements of all its memberships.

```ocl
member = membership.memberElement
```

### deriveNamespaceOwnedImport

The ownedImports of a Namespace are all its ownedRelationships that are Imports.

```ocl
ownedImport = ownedRelationship->selectByKind(Import)
```

### deriveNamespaceOwnedMember

The ownedMembers of a Namespace are the ownedMemberElements of all its ownedMemberships that are OwningMemberships.

```ocl
ownedMember = ownedMembership->selectByKind(OwningMembership).ownedMemberElement
```

### deriveNamespaceOwnedMembership

The ownedMemberships of a Namespace are all its ownedRelationships that are Memberships.

```ocl
ownedMembership = ownedRelationship->selectByKind(Membership)
```

### validateNamespaceDistinguishibility

All memberships of a Namespace must be distinguishable from each other.

```ocl
membership->forAll(m1 | 
    membership->forAll(m2 | 
        m1 <> m2 implies m1.isDistinguishableFrom(m2)))
```

