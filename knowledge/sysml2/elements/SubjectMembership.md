# SubjectMembership

`Requirements` package · concrete metaclass

A SubjectMembership is a ParameterMembership that indicates that its ownedSubjectParameter is the subject of its owningType. The owningType of a SubjectMembership must be a RequirementDefinition, RequirementUsage, CaseDefinition, or CaseUsage.

## Owned features

### ownedSubjectParameter : Usage [1..1]

The UsageownedMemberParameter of this SubjectMembership.


## Constraints

- **validateSubjectMembershipOwningType**
