# VariantMembership

`DefinitionAndUsage` package · concrete metaclass

A VariantMembership is a Membership between a variation point Definition or Usage and a Usage that represents a variant in the context of that variation. The membershipOwningNamespace for the VariantMembership must be either a Definition or a Usage with isVariation = true.

## Owned features

### ownedVariantUsage : Usage [1..1]

The Usage that represents a variant in the context of the owningVariationDefinition or owningVariationUsage.


## Constraints

- **validateVariantMembershipOwningNamespace**
