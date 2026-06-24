# ReferenceUsage

`DefinitionAndUsage` package · concrete metaclass

A ReferenceUsage is a Usage that specifies a non-compositional (isComposite = false) reference to something. The definition of a ReferenceUsage can be any kind of Classifier, with the default being the top-level Classifier Base::Anything from the Kernel Semantic Library. This allows the specification of a generic reference without distinguishing if the thing referenced is an attribute value, item, action, etc.

## Generalizations

- [Usage](Usage.md)

## Owned features

### isReference : Boolean [1..1]

Always true for a ReferenceUsage.

Redefines: `isReference`


## Constraints

- **validateReferenceUsageIsReference**
