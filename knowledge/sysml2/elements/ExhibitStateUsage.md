# ExhibitStateUsage

`States` package · concrete metaclass

An ExhibitStateUsage is a StateUsage that represents the exhibiting of a StateUsage. Unless it is the StateUsage itself, the StateUsage to be exhibited is related to the ExhibitStateUsage by a ReferenceSubsetting Relationship. An ExhibitStateUsage is also a PerformActionUsage, with its exhibitedState as the performedAction.

## Generalizations

- [PerformActionUsage](PerformActionUsage.md)
- [StateUsage](StateUsage.md)

## Owned features

### exhibitedState : StateUsage [1..1]

The StateUsage to be exhibited by the ExhibitStateUsage. It is the performedAction of the ExhibitStateUsage considered as a PerformActionUsage, which must be a StateUsage.

Redefines: `performedAction`


## Constraints

- **checkExhibitStateUsageSpecialization**
- **validateExhibitStateUsageReference**
