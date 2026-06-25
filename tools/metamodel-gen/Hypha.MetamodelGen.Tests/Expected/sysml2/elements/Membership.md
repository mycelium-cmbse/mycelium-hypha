---
name: Membership
package: Namespaces
isAbstract: false
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

### memberElement : Element [1..1]

The Element that becomes a member of the membershipOwningNamespace due to this Membership.

Redefines: `target`

### memberElementId : String [1..1] {derived}

The elementId of the memberElement.

### memberName : String [0..1]

The name of the memberElement relative to the membershipOwningNamespace.

### memberShortName : String [0..1]

The short name of the memberElement relative to the membershipOwningNamespace.

### membershipOwningNamespace : Namespace [1..1] {derived}

The Namespace of which the memberElement becomes a member due to this Membership.

Redefines: `source`

Subsets: `membershipNamespace`, `owningRelatedElement`

### visibility : VisibilityKind [1..1]

Whether or not the Membership of the memberElement in the membershipOwningNamespace is publicly visible outside that Namespace.


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

### deriveMembershipMemberElementId

The memberElementId of a Membership is the elementId of its memberElement.

```ocl
memberElementId = memberElement.elementId
```

