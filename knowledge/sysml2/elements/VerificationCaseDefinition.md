# VerificationCaseDefinition

`VerificationCases` package · concrete metaclass

A VerificationCaseDefinition is a CaseDefinition for the purpose of verification of the subject of the case against its requirements.

## Generalizations

- [CaseDefinition](CaseDefinition.md)

## Owned features

### verifiedRequirement : RequirementUsage [0..*]

The RequirementUsages verified by this VerificationCaseDefinition, which are the verifiedRequirements of all RequirementVerificationMemberships of the objectiveRequirement.


## Constraints

- **checkVerificationCaseSpecialization**
- **deriveVerificationCaseDefinitionVerifiedRequirement**
