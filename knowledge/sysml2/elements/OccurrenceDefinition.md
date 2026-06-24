# OccurrenceDefinition

`Occurrences` package · concrete metaclass

An OccurrenceDefinition is a Definition of a Class of individuals that have an independent life over time and potentially an extent over space. This includes both structural things and behaviors that act on such structures. If isIndividual is true, then the OccurrenceDefinition is constrained to have (at most) a single instance that is the entire life of a single individual.

## Generalizations

- [Definition](Definition.md)

## Owned features

### isIndividual : Boolean [1..1]

Whether this OccurrenceDefinition is constrained to represent at most one thing.


## Constraints

- **checkOccurrenceDefinitionIndividualSpecialization**
- **checkOccurrenceDefinitionMultiplicitySpecialization**
