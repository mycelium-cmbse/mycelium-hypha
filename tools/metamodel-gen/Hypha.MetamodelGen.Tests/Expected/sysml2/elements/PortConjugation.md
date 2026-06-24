# PortConjugation

`Ports` package · concrete metaclass

A PortConjugation is a Conjugation Relationship between a PortDefinition and its corresponding ConjugatedPortDefinition. As a result of this Relationship, the ConjugatedPortDefinition inherits all the features of the original PortDefinition, but input flows of the original PortDefinition become outputs on the ConjugatedPortDefinition and output flows of the original PortDefinition become inputs on the ConjugatedPortDefinition.

## Owned features

### conjugatedPortDefinition : ConjugatedPortDefinition [1..1]

The ConjugatedPortDefinition that is conjugate to the originalPortDefinition.

### originalPortDefinition : PortDefinition [1..1]

The PortDefinition being conjugated.

