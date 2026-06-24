# OccurrenceUsage

`Occurrences` package · concrete metaclass

An OccurrenceUsage is a Usage whose types are all Classes. Nominally, if a type is an OccurrenceDefinition, an OccurrenceUsage is a Usage of that OccurrenceDefinition within a system. However, other types of Kernel Classes are also allowed, to permit use of Classes from the Kernel Model Libraries.

## Generalizations

- [Usage](Usage.md)

## Owned features

### individualDefinition : OccurrenceDefinition [0..1]

The at most one occurrenceDefinition that has isIndividual = true.

Subsets: `occurrenceDefinition`

### isIndividual : Boolean [1..1]

Whether this OccurrenceUsage represents the usage of the specific individual represented by its individualDefinition.

### occurrenceDefinition : &#171;untyped&#187; [0..*]

The Classes that are the types of this OccurrenceUsage. Nominally, these are OccurrenceDefinitions, but other kinds of kernel Classes are also allowed, to permit use of Classes from the Kernel Model Libraries.

Redefines: `definition`

### portionKind : PortionKind [0..1]

The kind of temporal portion (time slice or snapshot) is represented by this OccurrenceUsage. If portionKind is not null, then the owningType of the OccurrenceUsage must be non-null, and the OccurrenceUsage represents portions of the featuring instance of the owningType.


## Constraints

- **checkOccurrenceUsageSnapshotSpecialization**
- **checkOccurrenceUsageSpecialization**
- **checkOccurrenceUsageSuboccurrenceSpecialization**
- **checkOccurrenceUsageTimeSliceSpecialization**
- **deriveOccurrenceUsageIndividualDefinition**
- **validateOccurrenceUsageIndividualDefinition**
- **validateOccurrenceUsageIndividualUsage**
- **validateOccurrenceUsageIsPortion**
- **validateOccurrenceUsagePortionKind**
