# ConjugatedPortTyping

`Ports` package · concrete metaclass

A ConjugatedPortTyping is a FeatureTyping whose type is a ConjugatedPortDefinition. (This relationship is intended to be an abstract-syntax marker for a special surface notation for conjugated typing of ports.)

## Owned features

### conjugatedPortDefinition : ConjugatedPortDefinition [1..1]

The type of this ConjugatedPortTyping considered as a FeatureTyping, which must be a ConjugatedPortDefinition.

### portDefinition : PortDefinition [1..1]

The originalPortDefinition of the conjugatedPortDefinition of this ConjugatedPortTyping.


## Constraints

- **deriveConjugatedPortTypingPortDefinition**
