# AllocationDefinition

`Allocations` package · concrete metaclass

An AllocationDefinition is a ConnectionDefinition that specifies that some or all of the responsibility to realize the intent of the source is allocated to the target instances. Such allocations define mappings across the various structures and hierarchies of a system model, perhaps as a precursor to more rigorous specifications and implementations. An AllocationDefinition can itself be refined using nested allocations that give a finer-grained decomposition of the containing allocation mapping.

## Generalizations

- [ConnectionDefinition](ConnectionDefinition.md)

## Owned features

### allocation : AllocationUsage [0..*]

The AllocationUsages that refine the allocation mapping defined by this AllocationDefinition.

Subsets: `usage`


## Constraints

- **checkAllocationDefinitionSpecialization**
- **deriveAllocationDefinitionAllocation**
