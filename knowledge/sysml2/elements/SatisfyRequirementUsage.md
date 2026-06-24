# SatisfyRequirementUsage

`Requirements` package · concrete metaclass

A SatisfyRequirementUsage is an AssertConstraintUsage that asserts, by default, that a satisfied RequirementUsage is true for a specific satisfyingFeature, or, if isNegated = true, that the RequirementUsage is false. The satisfied RequirementUsage is related to the SatisfyRequirementUsage by a ReferenceSubsetting Relationship.

## Generalizations

- [AssertConstraintUsage](AssertConstraintUsage.md)
- [RequirementUsage](RequirementUsage.md)

## Owned features

### satisfiedRequirement : RequirementUsage [1..1]

The RequirementUsage that is satisfied by the satisfyingSubject of this SatisfyRequirementUsage. It is the assertedConstraint of the SatisfyRequirementUsage considered as an AssertConstraintUsage, which must be a RequirementUsage.

Redefines: `assertedConstraint`

### satisfyingFeature : &#171;untyped&#187; [1..1]

The Feature that represents the actual subject that is asserted to satisfy the satisfiedRequirement. The satisfyingFeature is bound to the subjectParameter of the SatisfyRequirementUsage.


## Constraints

- **checkSatisfyRequirementUsageBindingConnector**
- **checkSatisfyRequirementUsageSpecialization**
- **deriveSatisfyRequirementUsageSatisfyingFeature**
- **validateSatisfyRequirementUsageReference**
