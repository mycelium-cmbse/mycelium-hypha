---
name: Namespace
package: Namespaces
fully qualified name: KerML::Root::Namespaces::Namespace
isAbstract: false
visibility: public
generalizes: [Element]
specializedBy: [Package, Type]
---

# Namespace

`Namespaces` package · concrete metaclass

A Namespace is an Element that contains other Elements, known as its members, via Membership Relationships with those Elements. The members of a Namespace may be owned by the Namespace, aliased in the Namespace, or imported into the Namespace via Import Relationships. A Namespace can provide names for its members via the memberNames and memberShortNames specified by the Memberships in the Namespace. If a Membership specifies a memberName and/or memberShortName, then those are names of the corresponding memberElement relative to the Namespace. For an OwningMembership, the ownedMemberName and ownedMemberShortName are given by the Element name and shortName. Note that the same Element may be the memberElement of multiple Memberships in a Namespace (though it may be owned at most once), each of which may define a separate alias for the Element relative to the Namespace.

## Generalizations

- [Element](Element.md)

## Specializations

- [Package](Package.md)
- [Type](Type.md)

## Owned features

### importedMembership

`+` [Membership](Membership.md) · `[0..*]` · *derived, ordered*

The Memberships in this Namespace that result from the ownedImports of this Namespace.

Subsets [membership](#membership)

### member

`+` [Element](Element.md) · `[0..*]` · *derived, ordered*

The set of all member Elements of this Namespace, which are the memberElements of all memberships of the Namespace.

### membership

`+` [Membership](Membership.md) · `[0..*]` · *derived, ordered*

All Memberships in this Namespace, including (at least) the union of ownedMemberships and importedMemberships.

### ownedImport

`+` [Import](Import.md) · `[0..*]` · *derived, composite, ordered*

The ownedRelationships of this Namespace that are Imports, for which the Namespace is the importOwningNamespace.

Subsets [ownedRelationship](Element.md#ownedrelationship), `sourceRelationship`

### ownedMember

`+` [Element](Element.md) · `[0..*]` · *derived, ordered*

The owned members of this Namespace, which are the <cpde>ownedMemberElements of the ownedMemberships of the Namespace.</cpde>

Subsets [member](#member)

### ownedMembership

`+` [Membership](Membership.md) · `[0..*]` · *derived, composite, ordered*

The ownedRelationships of this Namespace that are Memberships, for which the Namespace is the membershipOwningNamespace.

Subsets [membership](#membership), `sourceRelationship`, [ownedRelationship](Element.md#ownedrelationship)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |

## Operations

### importedMemberships

`importedMemberships(excluded : Namespace [0..*]) : Membership [0..*]`

Derive the imported Memberships of this Namespace as the importedMembership of all ownedImports, excluding those Imports whose importOwningNamespace is in the excluded set, and excluding Memberships that have distinguisibility collisions with each other or with any ownedMembership.

```ocl
ownedImport.importedMemberships(excluded->including(self))
```

### membershipsOfVisibility

`membershipsOfVisibility(visibility : VisibilityKind [0..1], excluded : Namespace [0..*]) : Membership [0..*]`

If visibility is not null, return the Memberships of this Namespace with the given visibility, including ownedMemberships with the given visibility and Memberships imported with the given visibility. If visibility is null, return all ownedMemberships and imported Memberships regardless of visibility. When computing imported Memberships, ignore this Namespace and any Namespaces in the given excluded set.

```ocl
ownedMembership->
    select(mem | visibility = null or mem.visibility = visibility)->
    union(ownedImport->
        select(imp | visibility = null or imp.visibility = visibility).
        importedMemberships(excluded->including(self)))
```

### namesOf

`namesOf(element : Element [1..1]) : String [0..*]`

Return the names of the given element as it is known in this Namespace.

```ocl
let elementMemberships : Sequence(Membership) = 
    memberships->select(memberElement = element) in
memberships.memberShortName->
    union(memberships.memberName)->
    asSet()
```

### qualificationOf

`qualificationOf(qualifiedName : String [1..1]) : String [0..1]`

Return a string with valid KerML syntax representing the qualification part of a given qualifiedName, that is, a qualified name with all the segment names of the given name except the last. If the given qualifiedName has only one segment, then return null.

```ocl
No OCL
```

### resolve

`resolve(qualifiedName : String [1..1]) : Membership [0..1]`

Resolve the given qualified name to the named Membership (if any), starting with this Namespace as the local scope. The qualified name string must conform to the concrete syntax of the KerML textual notation. According to the KerML name resolution rules every qualified name will resolve to either a single Membership, or to none.

```ocl
let qualification : String = qualificationOf(qualifiedName) in
let name : String = unqualifiedNameOf(qualifiedName) in
if qualification = null then resolveLocal(name)
else if qualification = '$' then  resolveGlobal(name)
else 
    let namespaceMembership : Membership = resolve(qualification) in
    if namespaceMembership = null or 
       not namespaceMembership.memberElement.oclIsKindOf(Namespace) 
    then null
    else 
        namespaceMembership.memberElement.oclAsType(Namespace).
        resolveVisible(name) 
    endif
endif endif
```

### resolveGlobal

`resolveGlobal(qualifiedName : String [1..1]) : Membership [0..1]`

Resolve the given qualified name to the named Membership (if any) in the effective global Namespace that is the outermost naming scope. The qualified name string must conform to the concrete syntax of the KerML textual notation.

```ocl
No OCL
```

### resolveLocal

`resolveLocal(name : String [1..1]) : Membership [0..1]`

Resolve a simple name starting with this Namespace as the local scope, and continuing with containing outer scopes as necessary. However, if this Namespace is a root Namespace, then the resolution is done directly in global scope.

```ocl
if owningNamespace = null then resolveGlobal(name)
else
    let memberships : Membership = membership->
        select(memberShortName = name or memberName = name) in
    if memberships->notEmpty() then memberships->first()
    else owningNamespace.resolveLocal(name)
    endif
endif
```

### resolveVisible

`resolveVisible(name : String [1..1]) : Membership [0..1]`

Resolve a simple name from the visible Memberships of this Namespace.

```ocl
let memberships : Sequence(Membership) =
    visibleMemberships(Set{}, false, false)->
    select(memberShortName = name or memberName = name) in
if memberships->isEmpty() then null
else memberships->first()
endif
```

### unqualifiedNameOf

`unqualifiedNameOf(qualifiedName : String [1..1]) : String [1..1]`

Return the simple name that is the last segment name of the given qualifiedName. If this segment name has the form of a KerML unrestricted name, then "unescape" it by removing the surrounding single quotes and replacing all escape sequences with the specified character.

```ocl
No OCL
```

### visibilityOf

`visibilityOf(mem : Membership [1..1]) : VisibilityKind [1..1]`

Returns this visibility of mem relative to this Namespace. If mem is an importedMembership, this is the visibility of its Import. Otherwise it is the visibility of the Membership itself.

```ocl
if importedMembership->includes(mem) then
    ownedImport->
        select(importedMemberships(Set{})->includes(mem)).
        first().visibility
else if memberships->includes(mem) then
    mem.visibility
else
    VisibilityKind::private
endif
```

### visibleMemberships

`visibleMemberships(excluded : Namespace [0..*], isRecursive : Boolean [1..1], includeAll : Boolean [1..1]) : Membership [0..*]`

If includeAll = true, then return all the Memberships of this Namespace. Otherwise, return only the publicly visible Memberships of this Namespace, including ownedMemberships that have a visibility of public and Memberships imported with a visibility of public. If isRecursive = true, also recursively include all visible Memberships of any public owned Namespaces, or, if IncludeAll = true, all Memberships of all owned Namespaces. When computing imported Memberships, ignore this Namespace and any Namespaces in the given excluded set.

```ocl
let visibleMemberships : OrderedSet(Membership) = 
    if includeAll then membershipsOfVisibility(null, excluded)
    else membershipsOfVisibility(VisibilityKind::public, excluded)
    endif in
if not isRecursive then visibleMemberships
else visibleMemberships->union(ownedMember->
    selectAsKind(Namespace).
    select(includeAll or owningMembership.visibility = VisibilityKind::public)->
    visibleMemberships(excluded->including(self), true, includeAll))
endif
```


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

