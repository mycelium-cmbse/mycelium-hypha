# PortDefinition

`Ports` package · concrete metaclass

A PortDefinition defines a point at which external entities can connect to and interact with a system or part of a system. Any ownedUsages of a PortDefinition, other than PortUsages, must not be composite.

## Generalizations

- [OccurrenceDefinition](OccurrenceDefinition.md)

## Owned features

### conjugatedPortDefinition : ConjugatedPortDefinition [0..1]

The ConjugatedPortDefinition that is conjugate to this PortDefinition.


## Constraints

- **checkPortDefinitionSpecialization**
- **derivePortDefinitionConjugatedPortDefinition**
- **validatePortDefinitionConjugatedPortDefinition**
- **validatePortDefinitionOwnedUsagesNotComposite**
