# PartUsage

`Parts` package · concrete metaclass

A PartUsage is a usage of a PartDefinition to represent a system or a part of a system. At least one of the itemDefinitions of the PartUsage must be a PartDefinition.A PartUsage must subset, directly or indirectly, the base PartUsage parts from the Systems Model Library.

## Generalizations

- [ItemUsage](ItemUsage.md)

## Owned features

### partDefinition : PartDefinition [0..*]

The itemDefinitions of this PartUsage that are PartDefinitions.

Subsets: `itemDefinition`


## Constraints

- **checkPartUsageActorSpecialization**
- **checkPartUsageSpecialization**
- **checkPartUsageStakeholderSpecialization**
- **checkPartUsageSubpartSpecialization**
- **derivePartUsagePartDefinition**
- **validatePartUsagePartDefinition**
