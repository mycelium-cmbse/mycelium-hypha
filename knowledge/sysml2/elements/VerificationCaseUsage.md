# VerificationCaseUsage

`VerificationCases` package · concrete metaclass

A VerificationCaseUsage is a Usage of a VerificationCaseDefinition.

## Generalizations

- [CaseUsage](CaseUsage.md)

## Owned features

### verificationCaseDefinition : VerificationCaseDefinition [0..1]

The VerificationCase that is the definition of this VerificationCaseUsage.

Subsets: `caseDefinition`

### verifiedRequirement : RequirementUsage [0..*]

The RequirementUsages verified by this VerificationCaseUsage, which are the verifiedRequirements of all RequirementVerificationMemberships of the objectiveRequirement.


## Constraints

- **checkVerificationCaseUsageSpecialization**
- **checkVerificationCaseUsageSubVerificationCaseSpecialization**
- **deriveVerificationCaseUsageVerifiedRequirement**
