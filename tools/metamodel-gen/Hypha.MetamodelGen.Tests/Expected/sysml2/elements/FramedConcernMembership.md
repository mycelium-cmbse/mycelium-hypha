# FramedConcernMembership

`Requirements` package · concrete metaclass

A FramedConcernMembership is a RequirementConstraintMembership for a framed ConcernUsage of a RequirementDefinition or RequirementUsage.

## Generalizations

- [RequirementConstraintMembership](RequirementConstraintMembership.md)

## Owned features

### kind : RequirementConstraintKind [1..1]

The kind of an FramedConcernMembership must be requirement.

Redefines: `kind`

### ownedConcern : ConcernUsage [1..1]

The ConcernUsage that is the ownedConstraint of this FramedConcernMembership.

Redefines: `ownedConstraint`

### referencedConcern : ConcernUsage [1..1]

The ConcernUsage that is referenced through this FramedConcernMembership. It is the referencedConstraint of the FramedConcernMembership considered as a RequirementConstraintMembership, which must be a ConcernUsage.

Redefines: `referencedConstraint`


## Constraints

- **validateFramedConcernMembershipConstraintKind**
