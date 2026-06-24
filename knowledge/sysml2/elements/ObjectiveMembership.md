# ObjectiveMembership

`Cases` package · concrete metaclass

An ObjectiveMembership is a FeatureMembership that indicates that its ownedObjectiveRequirement is the objective RequirementUsage for its owningType, which must be a CaseDefinition or CaseUsage.

## Owned features

### ownedObjectiveRequirement : RequirementUsage [1..1]

The RequirementUsage that is the ownedMemberFeature of this RequirementUsage.


## Constraints

- **validateObjectiveMembershipIsComposite**
- **validateObjectiveMembershipOwningType**
