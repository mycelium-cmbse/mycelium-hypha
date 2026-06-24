# EventOccurrenceUsage

`Occurrences` package · concrete metaclass

An EventOccurrenceUsage is an OccurrenceUsage that represents another OccurrenceUsage occurring as a suboccurrence of the containing occurrence of the EventOccurrenceUsage. Unless it is the EventOccurrenceUsage itself, the referenced OccurrenceUsage is related to the EventOccurrenceUsage by a ReferenceSubsetting Relationship.If the EventOccurrenceUsage is owned by an OccurrenceDefinition or OccurrenceUsage, then it also subsets the timeEnclosedOccurrences property of the Class Occurrence from the Kernel Semantic Library model Occurrences.

## Generalizations

- [OccurrenceUsage](OccurrenceUsage.md)

## Owned features

### eventOccurrence : OccurrenceUsage [1..1]

The OccurrenceUsage referenced as an event by this EventOccurrenceUsage. It is the referenceFeature of the ownedReferenceSubsetting for the EventOccurrenceUsage, if there is one, and, otherwise, the EventOccurrenceUsage itself.

### isReference : Boolean [1..1]

Always true for an EventOccurrenceUsage.

Redefines: `isReference`


## Constraints

- **checkEventOccurrenceUsageSpecialization**
- **deriveEventOccurrenceUsageEventOccurrence**
- **validateEventOccurrenceUsageIsReference**
- **validateEventOccurrenceUsageReference**
