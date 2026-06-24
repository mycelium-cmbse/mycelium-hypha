# ConcernUsage

`Requirements` package · concrete metaclass

A ConcernUsage is a Usage of a ConcernDefinition. The ownedStakeholder features of the ConcernUsage shall all subset the ConcernCheck::concernedStakeholders feature. If the ConcernUsage is an ownedFeature of a StakeholderDefinition or StakeholderUsage, then the ConcernUsage shall have an ownedStakeholder feature that is bound to the self feature of its owner.

## Generalizations

- [RequirementUsage](RequirementUsage.md)

## Owned features

### concernDefinition : ConcernDefinition [0..1]

The ConcernDefinition that is the single type of this ConcernUsage.

Redefines: `requirementDefinition`


## Constraints

- **checkConcernUsageFramedConcernSpecialization**
- **checkConcernUsageSpecialization**
