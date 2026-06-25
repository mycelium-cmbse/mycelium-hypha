---
name: OwningMembership
package: Namespaces
fully qualified name: KerML::Root::Namespaces::OwningMembership
isAbstract: false
visibility: public
generalizes: [Membership]
specializedBy: [ElementFilterMembership, FeatureMembership, FeatureValue, VariantMembership]
---

# OwningMembership

`Namespaces` package · concrete metaclass

An OwningMembership is a Membership that owns its memberElement as a ownedRelatedElement. The ownedMemberElement becomes an ownedMember of the membershipOwningNamespace.

## Generalizations

- [Membership](Membership.md)

## Specializations

- [ElementFilterMembership](ElementFilterMembership.md)
- [FeatureMembership](FeatureMembership.md)
- [FeatureValue](FeatureValue.md)
- [VariantMembership](VariantMembership.md)

## Owned features

### ownedMemberElement

`+` [Element](Element.md) · `[1..1]` · *derived, composite*

The Element that becomes an ownedMember of the membershipOwningNamespace due to this OwningMembership.

Redefines [memberElement](Membership.md#memberelement)

Subsets [ownedRelatedElement](Relationship.md#ownedrelatedelement)

### ownedMemberElementId

`+` String · `[1..1]` · *derived*

The elementId of the ownedMemberElement.

Redefines [memberElementId](Membership.md#memberelementid)

### ownedMemberName

`+` String · `[0..1]` · *derived*

The name of the ownedMemberElement.

Redefines [memberName](Membership.md#membername)

### ownedMemberShortName

`+` String · `[0..1]` · *derived*

The shortName of the ownedMemberElement.

Redefines [memberShortName](Membership.md#membershortname)


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
| memberElement | [Element](Element.md) | [1..1] | [Membership](Membership.md) |  |
| memberElementId | String | [1..1] | [Membership](Membership.md) | derived |
| memberName | String | [0..1] | [Membership](Membership.md) |  |
| memberShortName | String | [0..1] | [Membership](Membership.md) |  |
| membershipOwningNamespace | [Namespace](Namespace.md) | [1..1] | [Membership](Membership.md) | derived |
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
| visibility | VisibilityKind | [1..1] | [Membership](Membership.md) |  |

## Constraints

### deriveOwningMembershipOwnedMemberName

The ownedMemberName of an OwningMembership is the name of its ownedMemberElement.

```ocl
ownedMemberName = ownedMemberElement.name
```

### deriveOwningMembershipOwnedMemberShortName

The ownedMemberShortName of an OwningMembership is the shortName of its ownedMemberElement.

```ocl
ownedMemberShortName = ownedMemberElement.shortName
```

