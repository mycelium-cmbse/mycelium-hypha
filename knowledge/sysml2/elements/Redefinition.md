---
name: Redefinition
package: Features
isAbstract: false
generalizes: [Subsetting]
specializedBy: []
---

# Redefinition

`Features` package · concrete metaclass

Redefinition is a kind of Subsetting that requires the redefinedFeature and the redefiningFeature to have the same values (on each instance of the domain of the redefiningFeature). This means any restrictions on the redefiningFeature, such as type or multiplicity, also apply to the redefinedFeature (on each instance of the domain of the redefiningFeature), and vice versa. The redefinedFeature might have values for instances of the domain of the redefiningFeature, but only as instances of the domain of the redefinedFeature that happen to also be instances of the domain of the redefiningFeature. This is supported by the constraints inherited from Subsetting on the domains of the redefiningFeature and redefinedFeature. However, these constraints are narrowed for Redefinition to require the owningTypes of the redefiningFeature and redefinedFeature to be different and the redefinedFeature to not be inherited into the owningNamespace of the redefiningFeature.This enables the redefiningFeature to have the same name as the redefinedFeature, if desired.

## Generalizations

- [Subsetting](Subsetting.md)

## Owned features

### redefinedFeature : Feature [1..1]

The Feature that is redefined by the redefiningFeature of this Redefinition.

Redefines: `subsettedFeature`

### redefiningFeature : Feature [1..1]

The Feature that is redefining the redefinedFeature of this Redefinition.

Redefines: `subsettingFeature`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| general | Type | [1..1] | [Specialization](Specialization.md) |  |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelatedElement | Element | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningFeature | Feature | [0..1] | [Subsetting](Subsetting.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | Element | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| owningType | Type | [0..1] | [Specialization](Specialization.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| specific | Type | [1..1] | [Specialization](Specialization.md) |  |
| subsettedFeature | Feature | [1..1] | [Subsetting](Subsetting.md) |  |
| subsettingFeature | Feature | [1..1] | [Subsetting](Subsetting.md) |  |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |

## Constraints

### validateRedefinitionDirectionConformance

If the redefinedFeature of a Redefinition has a direction of in or out (relative to any featuringType of the redefiningFeature or the owningType, if the redefiningFeature has isVariable = true), then the redefiningFeature must have the same direction. If the redefinedFeature has a direction of inout, then the redefiningFeature must have a non-null direction. (Note: the direction of the redefinedFeature relative to a featuringType of the redefiningFeature is the direction it would have if it had been inherited and not redefined.)

```ocl
let featuringTypes : Sequence(Type) =
    if redefiningFeature.isVariable then Sequence{redefiningFeature.owningType}
    else redefiningFeature.featuringType
    endif in
featuringTypes->forAll(t |
    let direction : FeatureDirectionKind = t.directionOf(redefinedFeature) in
    ((direction = FeatureDirectionKind::_'in' or 
      direction = FeatureDirectionKind::out) implies
         redefiningFeature.direction = direction)
    and 
    (direction = FeatureDirectionKind::inout implies
        redefiningFeature.direction <> null))
```

### validateRedefinitionEndConformance

If the redefinedFeature of a Redefinition has isEnd = true, then the redefiningFeature must have isEnd = true.

```ocl
redefinedFeature.isEnd implies redefiningFeature.isEnd
```

### validateRedefinitionFeaturingTypes

The redefiningFeature of a Redefinition must have at least one featuringType that is not also a featuringType of the redefinedFeature.

```ocl
let anythingType: Type =
    redefiningFeature.resolveGlobal('Base::Anything').modelElement.oclAsType(Type) in 
-- Including "Anything" accounts for implicit featuringType of Features
-- with no explicit featuringType.
let redefiningFeaturingTypes: Set(Type) =
    if redefiningFeature.isVariable then Set{redefiningFeature.owningType}
    else redefiningFeature.featuringTypes->asSet()->including(anythingType) 
    endif in
let redefinedFeaturingTypes: Set(Type) =
    if redefinedFeature.isVariable then Set{redefinedFeature.owningType}
    else redefinedFeature.featuringTypes->asSet()->including(anythingType)
    endif in
redefiningFeaturingTypes <> redefinedFeaturingType
```

