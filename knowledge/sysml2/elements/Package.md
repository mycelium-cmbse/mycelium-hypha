---
name: Package
package: Packages
isAbstract: false
generalizes: [Namespace]
specializedBy: [LibraryPackage]
---

# Package

`Packages` package · concrete metaclass

A Package is a Namespace used to group Elements, without any instance-level semantics. It may have one or more model-level evaluable filterCondition Expressions used to filter its importedMemberships. Any imported member must meet all of the filterConditions.

## Generalizations

- [Namespace](Namespace.md)

## Specializations

- [LibraryPackage](LibraryPackage.md)

## Owned features

### filterCondition : Expression [0..*] {derived, ordered}

The model-level evaluable Boolean-valued Expression used to filter the members of this Package, which are owned by the Package are via ElementFilterMemberships.

Subsets: `ownedMember`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| importedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| member | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedImport | Import | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedMember | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |

## Constraints

### derivePackageFilterCondition

The filterConditions of a Package are the conditions of its owned ElementFilterMemberships.

```ocl
filterCondition = ownedMembership->
    selectByKind(ElementFilterMembership).condition
```

