# AllocationUsage

`Allocations` package · concrete metaclass

An AllocationUsage is a usage of an AllocationDefinition asserting the allocation of the source feature to the target feature.

## Generalizations

- [ConnectionUsage](ConnectionUsage.md)

## Owned features

### allocationDefinition : AllocationDefinition [0..*]

The AllocationDefinitions that are the types of this AllocationUsage.

Redefines: `connectionDefinition`


## Constraints

- **checkAllocationUsageSpecialization**
