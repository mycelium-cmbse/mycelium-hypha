# PerformActionUsage

`Actions` package · concrete metaclass

A PerformActionUsage is an ActionUsage that represents the performance of an ActionUsage. Unless it is the PerformActionUsage itself, the ActionUsage to be performed is related to the PerformActionUsage by a ReferenceSubsetting relationship. A PerformActionUsage is also an EventOccurrenceUsage, with its performedAction as the eventOccurrence.

## Generalizations

- [ActionUsage](ActionUsage.md)
- [EventOccurrenceUsage](EventOccurrenceUsage.md)

## Owned features

### performedAction : ActionUsage [1..1]

The ActionUsage to be performed by this PerformedActionUsage. It is the eventOccurrence of the PerformActionUsage considered as an EventOccurrenceUsage, which must be an ActionUsage.

Redefines: `eventOccurrence`


## Constraints

- **checkPerformActionUsageSpecialization**
- **validatePerformActionUsageReference**
