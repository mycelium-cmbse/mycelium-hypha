---
name: Redefinition
package: Features
fully qualified name: KerML::Core::Features::Redefinition
isAbstract: false
visibility: public
generalizes: [Subsetting]
specializedBy: []
---

# Redefinition

`Features` package · concrete metaclass

Redefinition is a kind of Subsetting that requires the redefinedFeature and the redefiningFeature to have the same values (on each instance of the domain of the redefiningFeature). This means any restrictions on the redefiningFeature, such as type or multiplicity, also apply to the redefinedFeature (on each instance of the domain of the redefiningFeature), and vice versa. The redefinedFeature might have values for instances of the domain of the redefiningFeature, but only as instances of the domain of the redefinedFeature that happen to also be instances of the domain of the redefiningFeature. This is supported by the constraints inherited from Subsetting on the domains of the redefiningFeature and redefinedFeature. However, these constraints are narrowed for Redefinition to require the owningTypes of the redefiningFeature and redefinedFeature to be different and the redefinedFeature to not be inherited into the owningNamespace of the redefiningFeature.This enables the redefiningFeature to have the same name as the redefinedFeature, if desired.

## Generalizations

- [Subsetting](Subsetting.md)

## Owned features

### redefinedFeature

`+` [Feature](Feature.md) · `[1..1]`

The Feature that is redefined by the redefiningFeature of this Redefinition.

Redefines [subsettedFeature](Subsetting.md#subsettedfeature)

### redefiningFeature

`+` [Feature](Feature.md) · `[1..1]`

The Feature that is redefining the redefinedFeature of this Redefinition.

Redefines [subsettingFeature](Subsetting.md#subsettingfeature)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| general | [Type](Type.md) | [1..1] | [Specialization](Specialization.md) |  |
| isImplied | [Boolean](Boolean.md) | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningFeature | [Feature](Feature.md) | [0..1] | [Subsetting](Subsetting.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Specialization](Specialization.md) | derived |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| specific | [Type](Type.md) | [1..1] | [Specialization](Specialization.md) |  |
| subsettedFeature | [Feature](Feature.md) | [1..1] | [Subsetting](Subsetting.md) |  |
| subsettingFeature | [Feature](Feature.md) | [1..1] | [Subsetting](Subsetting.md) |  |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |

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

