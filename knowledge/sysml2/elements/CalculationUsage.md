# CalculationUsage

`Calculations` package · concrete metaclass

A CalculationUsage is an ActionUsage that is also an Expression, and, so, is typed by a Function. Nominally, if the type is a CalculationDefinition, a CalculationUsage is a Usage of that CalculationDefinition within a system. However, other kinds of kernel Functions are also allowed, to permit use of Functions from the Kernel Model Libraries.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### calculationDefinition : &#171;untyped&#187; [0..1]

The <ode>Function that is the type of this CalculationUsage. Nominally, this would be a CalculationDefinition, but a kernel Function is also allowed, to permit use of Functions from the Kernel Model Libraries.</ode>


## Constraints

- **checkCalculationUsageSpecialization**
- **checkCalculationUsageSubcalculationSpecialization**
