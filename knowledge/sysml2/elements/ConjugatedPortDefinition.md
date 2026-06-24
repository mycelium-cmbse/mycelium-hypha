# ConjugatedPortDefinition

`Ports` package · concrete metaclass

A ConjugatedPortDefinition is a PortDefinition that is a PortDefinition of its original PortDefinition. That is, a ConjugatedPortDefinition inherits all the features of the original PortDefinition, but input flows of the original PortDefinition become outputs on the ConjugatedPortDefinition and output flows of the original PortDefinition become inputs on the ConjugatedPortDefinition. Every PortDefinition (that is not itself a ConjugatedPortDefinition) has exactly one corresponding ConjugatedPortDefinition, whose effective name is the name of the originalPortDefinition, with the character ~ prepended.

## Generalizations

- [PortDefinition](PortDefinition.md)

## Owned features

### originalPortDefinition : PortDefinition [1..1]

The original PortDefinition for this ConjugatedPortDefinition, which is the owningNamespace of the ConjugatedPortDefinition.

### ownedPortConjugator : PortConjugation [1..1]

The PortConjugation that is the ownedConjugator of this ConjugatedPortDefinition, linking it to its originalPortDefinition.


## Constraints

- **validateConjugatedPortDefinitionConjugatedPortDefinitionIsEmpty**
- **validateConjugatedPortDefinitionOriginalPortDefinition**
