# RequirementVerificationMembership

`VerificationCases` package · concrete metaclass

A RequirementVerificationMembership is a RequirementConstraintMembership used in the objective of a VerificationCase to identify a RequirementUsage that is verified by the VerificationCase.

## Generalizations

- [RequirementConstraintMembership](RequirementConstraintMembership.md)

## Owned features

### kind : RequirementConstraintKind [1..1]

The kind of a RequirementVerificationMembership must be requirement.

Redefines: `kind`

### ownedRequirement : RequirementUsage [1..1]

The owned RequirementUsage that acts as the ownedConstraint for this RequirementVerificationMembership. This will either be the verifiedRequirement, or it will subset the verifiedRequirement.

Redefines: `ownedConstraint`

### verifiedRequirement : RequirementUsage [1..1]

The RequirementUsage that is identified as being verified. It is the referencedConstraint of the RequirementVerificationMembership considered as a RequirementConstraintMembership, which must be a RequirementUsage.

Redefines: `referencedConstraint`


## Constraints

- **validateRequirementVerificationMembershipKind**
- **validateRequirementVerificationMembershipOwningType**
