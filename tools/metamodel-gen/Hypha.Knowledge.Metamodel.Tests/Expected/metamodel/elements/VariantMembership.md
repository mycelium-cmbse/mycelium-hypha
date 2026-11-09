---
name: VariantMembership
package: DefinitionAndUsage
fully qualified name: SysML::Systems::DefinitionAndUsage::VariantMembership
isAbstract: false
visibility: public
generalizes: [OwningMembership]
specializedBy: []
---

# VariantMembership

`DefinitionAndUsage` package · concrete metaclass

A VariantMembership is a Membership between a variation point Definition or Usage and a Usage that represents a variant in the context of that variation. The membershipOwningNamespace for the VariantMembership must be either a Definition or a Usage with isVariation = true.

## Generalizations

- [OwningMembership](OwningMembership.md)

## Owned features

### ownedVariantUsage

`+` [Usage](Usage.md) · `[1..1]` · *derived, composite*

The Usage that represents a variant in the context of the owningVariationDefinition or owningVariationUsage.

Redefines [ownedMemberElement](OwningMembership.md#ownedmemberelement)


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
| memberElement | [Element](Element.md) | [1..1] | [Membership](Membership.md) |  |
| memberElementId | [String](String.md) | [1..1] | [Membership](Membership.md) | derived |
| memberName | [String](String.md) | [0..1] | [Membership](Membership.md) |  |
| memberShortName | [String](String.md) | [0..1] | [Membership](Membership.md) |  |
| membershipOwningNamespace | [Namespace](Namespace.md) | [1..1] | [Membership](Membership.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedMemberElement | [Element](Element.md) | [1..1] | [OwningMembership](OwningMembership.md) | derived, composite |
| ownedMemberElementId | [String](String.md) | [1..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberName | [String](String.md) | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberShortName | [String](String.md) | [0..1] | [OwningMembership](OwningMembership.md) | derived |
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
| visibility | [VisibilityKind](VisibilityKind.md) | [1..1] | [Membership](Membership.md) |  |

## Constraints

### validateVariantMembershipOwningNamespace

The membershipOwningNamespace of a VariantMembership must be a variation-point Definition or Usage.

```ocl
membershipOwningNamespace.oclIsKindOf(Definition) and
    membershipOwningNamespace.oclAsType(Definition).isVariation or
membershipOwningNamespace.oclIsKindOf(Usage) and
    membershipOwningNamespace.oclAsType(Usage).isVariation
```

