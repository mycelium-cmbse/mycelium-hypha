# RequirementConstraintMembership

`Requirements` package · concrete metaclass

A RequirementConstraintMembership is a FeatureMembership for an assumed or required ConstraintUsage of a RequirementDefinition or RequirementUsage.

## Owned features

### kind : RequirementConstraintKind [1..1]

Whether the RequirementConstraintMembership is for an assumed or required ConstraintUsage.

### ownedConstraint : ConstraintUsage [1..1]

The ConstraintUsage that is the ownedMemberFeature of this RequirementConstraintMembership.

### referencedConstraint : ConstraintUsage [1..1]

The ConstraintUsage that is referenced through this RequirementConstraintMembership. It is the referencedFeature of the ownedReferenceSubsetting of the ownedConstraint, if there is one, and, otherwise, the ownedConstraint itself.


## Constraints

- **deriveRequirementConstraintMembershipReferencedConstraint**
- **validateRequirementConstraintMembershipIsComposite**
- **validateRequirementConstraintMembershipOwningType**
