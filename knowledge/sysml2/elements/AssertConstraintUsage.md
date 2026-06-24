# AssertConstraintUsage

`Constraints` package · concrete metaclass

An AssertConstraintUsage is a ConstraintUsage that is also an Invariant and, so, is asserted to be true (by default). Unless it is the AssertConstraintUsage itself, the asserted ConstraintUsage is related to the AssertConstraintUsage by a ReferenceSubsetting Relationship.

## Generalizations

- [ConstraintUsage](ConstraintUsage.md)

## Owned features

### assertedConstraint : ConstraintUsage [1..1]

The ConstraintUsage to be performed by the AssertConstraintUsage. It is the referenceFeature of the ownedReferenceSubsetting for the AssertConstraintUsage, if there is one, and, otherwise, the AssertConstraintUsage itself.


## Constraints

- **checkAssertConstraintUsageSpecialization**
- **deriveAssertConstraintUsageAssertedConstraint**
- **validateAssertConstraintUsageReference**
