---
name: OwningMembership
package: Namespaces
isAbstract: false
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

### ownedMemberElement : Element [1..1] {derived, composite}

The Element that becomes an ownedMember of the membershipOwningNamespace due to this OwningMembership.

Redefines: `memberElement`

Subsets: `ownedRelatedElement`

### ownedMemberElementId : String [1..1] {derived}

The elementId of the ownedMemberElement.

Redefines: `memberElementId`

### ownedMemberName : String [0..1] {derived}

The name of the ownedMemberElement.

Redefines: `memberName`

### ownedMemberShortName : String [0..1] {derived}

The shortName of the ownedMemberElement.

Redefines: `memberShortName`


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
| memberElement | Element | [1..1] | [Membership](Membership.md) |  |
| memberElementId | String | [1..1] | [Membership](Membership.md) | derived |
| memberName | String | [0..1] | [Membership](Membership.md) |  |
| memberShortName | String | [0..1] | [Membership](Membership.md) |  |
| membershipOwningNamespace | Namespace | [1..1] | [Membership](Membership.md) | derived |
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

