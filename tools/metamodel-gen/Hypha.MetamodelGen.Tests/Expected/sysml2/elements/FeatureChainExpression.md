---
name: FeatureChainExpression
package: Expressions
fully qualified name: KerML::Kernel::Expressions::FeatureChainExpression
isAbstract: false
visibility: public
generalizes: [OperatorExpression]
specializedBy: []
---

# FeatureChainExpression

`Expressions` package · concrete metaclass

A FeatureChainExpression is an OperatorExpression whose operator is ".", which resolves to the Function ControlFunctions::'.' from the Kernel Functions Library. It evaluates to the result of chaining the result Feature of its single argument Expression with its targetFeature.

## Generalizations

- [OperatorExpression](OperatorExpression.md)

## Owned features

### operator

`+` String · `[1..1]`

Redefines [operator](#operator)

### targetFeature

`+` [Feature](Feature.md) · `[1..1]` · *derived*

The Feature that is accessed by this FeatureChainExpression, which is its first non-parameter member.

Subsets [member](Namespace.md#member)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| argument | [Expression](Expression.md) | [0..*] | [InstantiationExpression](InstantiationExpression.md) | derived, ordered |
| behavior | [Behavior](Behavior.md) | [0..*] | [Step](Step.md) | derived, ordered |
| chainingFeature | [Feature](Feature.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| crossFeature | [Feature](Feature.md) | [0..1] | [Feature](Feature.md) | derived |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| differencingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| direction | FeatureDirectionKind | [0..1] | [Feature](Feature.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| endFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| endOwningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| feature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | [FeatureMembership](FeatureMembership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| featureTarget | [Feature](Feature.md) | [1..1] | [Feature](Feature.md) | derived |
| featuringType | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| function | [Function](Function.md) | [0..1] | [Expression](Expression.md) | derived |
| importedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | [Membership](Membership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| input | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| instantiatedType | [Type](Type.md) | [1..1] | [InstantiationExpression](InstantiationExpression.md) | derived |
| intersectingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | Boolean | [1..1] | [Type](Type.md) |  |
| isComposite | Boolean | [1..1] | [Feature](Feature.md) |  |
| isConjugated | Boolean | [1..1] | [Type](Type.md) | derived |
| isConstant | Boolean | [1..1] | [Feature](Feature.md) |  |
| isDerived | Boolean | [1..1] | [Feature](Feature.md) |  |
| isEnd | Boolean | [1..1] | [Feature](Feature.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isModelLevelEvaluable | Boolean | [1..1] | [Expression](Expression.md) | derived |
| isOrdered | Boolean | [1..1] | [Feature](Feature.md) |  |
| isPortion | Boolean | [1..1] | [Feature](Feature.md) |  |
| isSufficient | Boolean | [1..1] | [Type](Type.md) |  |
| isUnique | Boolean | [1..1] | [Feature](Feature.md) |  |
| isVariable | Boolean | [1..1] | [Feature](Feature.md) |  |
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | [Multiplicity](Multiplicity.md) | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| output | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedConjugator | [Conjugation](Conjugation.md) | [0..1] | [Type](Type.md) | derived, composite |
| ownedCrossSubsetting | [CrossSubsetting](CrossSubsetting.md) | [0..1] | [Feature](Feature.md) | derived, composite |
| ownedDifferencing | [Differencing](Differencing.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedDisjoining | [Disjoining](Disjoining.md) | [0..*] | [Type](Type.md) | derived, composite |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedEndFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeatureChaining | [FeatureChaining](FeatureChaining.md) | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedFeatureInverting | [FeatureInverting](FeatureInverting.md) | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedImport | [Import](Import.md) | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedIntersecting | [Intersecting](Intersecting.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedMember | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedRedefinition | [Redefinition](Redefinition.md) | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedReferenceSubsetting | [ReferenceSubsetting](ReferenceSubsetting.md) | [0..1] | [Feature](Feature.md) | derived, composite |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | [Specialization](Specialization.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubsetting | [Subsetting](Subsetting.md) | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedTypeFeaturing | [TypeFeaturing](TypeFeaturing.md) | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedTyping | [FeatureTyping](FeatureTyping.md) | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedUnioning | [Unioning](Unioning.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..1] | [Feature](Feature.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| parameter | [Feature](Feature.md) | [0..*] | [Step](Step.md) | derived, ordered |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| result | [Feature](Feature.md) | [1..1] | [Expression](Expression.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| type | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkFeatureChainExpressionResultSpecialization

The result parameter of a FeatureChainExpression must specialize the feature chain of the FeatureChainExpression.

```ocl
let inputParameters : Sequence(Feature) = 
    ownedFeatures->select(direction = _'in') in
let sourceTargetFeature : Feature = 
    owningExpression.sourceTargetFeature() in
sourceTargetFeature <> null and
result.subsetsChain(inputParameters->first(), sourceTargetFeature) and
result.owningType = self
```

### checkFeatureChainExpressionSourceTargetRedefinition

The first ownedFeature of the first owned input parameter of a FeatureChainExpression must redefine its targetFeature.

```ocl
let sourceTargetFeature : Feature = sourceTargetFeature() in
sourceTargetFeature <> null and
sourceTargetFeature.redefines(targetFeature)
```

### checkFeatureChainExpressionTargetRedefinition

The first ownedFeature of the first owned input parameter of a FeatureChainExpression must redefine the Feature ControlFunctions::'.'::source::target from the Kernel Functions Library.

```ocl
let sourceTargetFeature : Feature = sourceTargetFeature() in
sourceTargetFeature <> null and
sourceTargetFeature.redefinesFromLibrary('ControlFunctions::\'.\'::source::target')
```

### deriveFeatureChainExpressionTargetFeature

The targetFeature of a FeatureChainExpression is the memberElement of its first ownedMembership that is not a ParameterMembership.

```ocl
targetFeature =
    let nonParameterMemberships : Sequence(Membership) = ownedMembership->
        reject(oclIsKindOf(ParameterMembership)) in
    if nonParameterMemberships->isEmpty() or
       not nonParameterMemberships->first().memberElement.oclIsKindOf(Feature)
    then null
    else nonParameterMemberships->first().memberElement.oclAsType(Feature)
    endif
```

### validateFeatureChainExpressionConformance

The targetFeature of a FeatureChainExpression must be featured within the result parameter of the argument Expression of the FeatureChainExpression.

```ocl
argument->notEmpty() implies
    targetFeature.isFeaturedWithin(argument->first().result)
```

### validateFeatureChainExpressionOperator

The operator of a FeatureChainExpression must be ".".

```ocl
operator = '.'
```

