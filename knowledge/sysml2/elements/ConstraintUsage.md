# ConstraintUsage

`Constraints` package · concrete metaclass

A ConstraintUsage is an OccurrenceUsage that is also a BooleanExpression, and, so, is typed by a Predicate. Nominally, if the type is a ConstraintDefinition, a ConstraintUsage is a Usage of that ConstraintDefinition. However, other kinds of kernel Predicates are also allowed, to permit use of Predicates from the Kernel Model Libraries.

## Generalizations

- [OccurrenceUsage](OccurrenceUsage.md)

## Owned features

### constraintDefinition : &#171;untyped&#187; [0..1]

The (single) Predicate that is the type of this ConstraintUsage. Nominally, this will be a ConstraintDefinition, but other kinds of Predicates are also allowed, to permit use of Predicates from the Kernel Model Libraries.


## Constraints

- **checkConstraintUsageCheckedConstraintSpecialization**
- **checkConstraintUsageRequirementConstraintSpecialization**
- **checkConstraintUsageSpecialization**
