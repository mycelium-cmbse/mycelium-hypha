---
name: Membership
package: Namespaces
fully qualified name: KerML::Root::Namespaces::Membership
isAbstract: false
visibility: public
generalizes: [Relationship]
specializedBy: [OwningMembership]
---

# Membership

`Namespaces` package · concrete metaclass

A Membership is a Relationship between a Namespace and an Element that indicates the Element is a member of (i.e., is contained in) the Namespace. Any memberNames specify how the memberElement is identified in the Namespace and the visibility specifies whether or not the memberElement is publicly visible from outside the Namespace.If a Membership is an OwningMembership, then it owns its memberElement, which becomes an ownedMember of the membershipOwningNamespace. Otherwise, the memberNames of a Membership are effectively aliases within the membershipOwningNamespace for an Element with a separate OwningMembership in the same or a different Namespace.&nbsp;

## Generalizations

- [Relationship](Relationship.md)

## Specializations

- [OwningMembership](OwningMembership.md)

## Owned features

### memberElement

`+` [Element](Element.md) · `[1..1]`

The Element that becomes a member of the membershipOwningNamespace due to this Membership.

Redefines [target](Relationship.md#target)

### memberElementId

`+` [String](String.md) · `[1..1]` · *derived*

The elementId of the memberElement.

### memberName

`+` [String](String.md) · `[0..1]`

The name of the memberElement relative to the membershipOwningNamespace.

### memberShortName

`+` [String](String.md) · `[0..1]`

The short name of the memberElement relative to the membershipOwningNamespace.

### membershipOwningNamespace

`+` [Namespace](Namespace.md) · `[1..1]` · *derived*

The Namespace of which the memberElement becomes a member due to this Membership.

Redefines [source](Relationship.md#source)

Subsets `membershipNamespace`, [owningRelatedElement](Relationship.md#owningrelatedelement)

### visibility

`+` [VisibilityKind](VisibilityKind.md) · `[1..1]`

Whether or not the Membership of the memberElement in the membershipOwningNamespace is publicly visible outside that Namespace.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| isImplied | [Boolean](Boolean.md) | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
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

## Constraints

### deriveMembershipMemberElementId

The memberElementId of a Membership is the elementId of its memberElement.

```ocl
memberElementId = memberElement.elementId
```

