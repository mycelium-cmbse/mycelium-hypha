---
name: InvocationExpression
package: Expressions
isAbstract: false
generalizes: [InstantiationExpression]
specializedBy: [OperatorExpression, TriggerInvocationExpression]
---

# InvocationExpression

`Expressions` package · concrete metaclass

An InvocationExpression is an InstantiationExpression whose instantiatedType must be a Behavior or a Feature typed by a single Behavior (such as a Step). Each of the input parameters of the instantiatedType are bound to the result of an argument Expression. If the instantiatedType is a Function or a Feature typed by a Function, then the result of the InvocationExpression is the result of the invoked Function. Otherwise, the result is an instance of the instantiatedType (essentially like a behavioral ConstructorExpression).

## Generalizations

- [InstantiationExpression](InstantiationExpression.md)

## Specializations

- [OperatorExpression](OperatorExpression.md)
- [TriggerInvocationExpression](TriggerInvocationExpression.md)

## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| argument | Expression | [0..*] | [InstantiationExpression](InstantiationExpression.md) | derived, ordered |
| behavior | Behavior | [0..*] | [Step](Step.md) | derived, ordered |
| chainingFeature | Feature | [0..*] | [Feature](Feature.md) | derived, ordered |
| crossFeature | Feature | [0..1] | [Feature](Feature.md) | derived |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| differencingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| direction | FeatureDirectionKind | [0..1] | [Feature](Feature.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| endFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| endOwningType | Type | [0..1] | [Feature](Feature.md) | derived |
| feature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, ordered |
| featureTarget | Feature | [1..1] | [Feature](Feature.md) | derived |
| featuringType | Type | [0..*] | [Feature](Feature.md) | derived, ordered |
| function | Function | [0..1] | [Expression](Expression.md) | derived |
| importedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | Membership | [0..*] | [Type](Type.md) | derived, ordered |
| input | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| instantiatedType | Type | [1..1] | [InstantiationExpression](InstantiationExpression.md) | derived |
| intersectingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
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
| member | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | Multiplicity | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| output | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedConjugator | Conjugation | [0..1] | [Type](Type.md) | derived, composite |
| ownedCrossSubsetting | CrossSubsetting | [0..1] | [Feature](Feature.md) | derived, composite |
| ownedDifferencing | Differencing | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedDisjoining | Disjoining | [0..*] | [Type](Type.md) | derived, composite |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedEndFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeatureChaining | FeatureChaining | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedFeatureInverting | FeatureInverting | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedFeatureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedImport | Import | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedIntersecting | Intersecting | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedMember | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedRedefinition | Redefinition | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedReferenceSubsetting | ReferenceSubsetting | [0..1] | [Feature](Feature.md) | derived, composite |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | Specialization | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubsetting | Subsetting | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedTypeFeaturing | TypeFeaturing | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedTyping | FeatureTyping | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedUnioning | Unioning | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningFeatureMembership | FeatureMembership | [0..1] | [Feature](Feature.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| owningType | Type | [0..1] | [Feature](Feature.md) | derived |
| parameter | Feature | [0..*] | [Step](Step.md) | derived, ordered |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| result | Feature | [1..1] | [Expression](Expression.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| type | Type | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkInvocationExpressionBehaviorBindingConnector

If the instantiatedType of an InvocationExpression is neither a Function nor a Feature whose type is a Function, then the InvocationExpression must own a BindingConnector between itself and its result parameter.

```ocl
not instantiatedType.oclIsKindOf(Function) and
not (instantiatedType.oclIsKindOf(Feature) and 
     instantiatedType.oclAsType(Feature).type->exists(oclIsKindOf(Function))) implies
    ownedFeature.selectByKind(BindingConnector)->exists(
        relatedFeature->includes(self) and
        relatedFeature->includes(result))
```

### checkInvocationExpressionBehaviorResultSpecialization

If the instantiatedType of an InvocationExpression is neither a Function nor a Feature whose type is a Function, then the result of the InvocationExpression must specialize the instantiatedType.

```ocl
not instantiatedType.oclIsKindOf(Function) and
not (instantiatedType.oclIsKindOf(Feature) and 
     instantiatedType.oclAsType(Feature).type->exists(oclIsKindOf(Function))) implies
    result.specializes(instantiatedType)
```

### checkInvocationExpressionDefaultValueBindingConnector

An InvocationExpression must own a BindingConnector between the featureWithValue and value Expression of any FeatureValue that is the effective default value for a feature of the instantiatedType of the InvocationExpression.

```ocl
TBD
```

### checkInvocationExpressionSpecialization

An InvocationExpression must specialize its instantiatedType.

```ocl
specializes(instantiatedType)
```

### deriveInvocationExpressionArgument

The arguments of an InvocationExpression are the value Expressions of the FeatureValues of its ownedFeatures, in an order corresponding to the order of the input parameters of the instantiatedType that the ownedFeatures redefine.

```ocl
instantiatedType.input->collect(inp | 
    ownedFeatures->select(redefines(inp)).valuation->
    select(v | v <> null).value
)
```

### validateInvocationExpressionInstantiatedType

The instantiatedType of an InvocationExpression must be either a Behavior or a Feature with a single type, which is a Behavior.

```ocl
instantiatedType.oclIsKindOf(Behavior) or
instantiatedType.oclIsKindOf(Feature) and
    instantiatedType.type->exists(oclIsKindOf(Behavior)) and
    instantiatedType.type->size(1)
```

### validateInvocationExpressionNoDuplicateParameterRedefinition

Two different ownedFeatures of an InvocationExpression must not redefine the same feature of the instantiatedType of the InvocationExpression.

```ocl
let features : OrderedSet(Feature) = instantiatedType.feature in
input->forAll(inp1 | input->forAll(inp2 |
    inp1 <> inp2 implies
        inp1.ownedRedefinition.redefinedFeature->
            intersection(inp2.ownedRedefinition.redefinedFeature)->
            intersection(features)->isEmpty()))
```

### validateInvocationExpressionOwnedFeatures

Other than its result, all the ownedFeatures of an InvocationExpression must have direction = in.

```ocl
ownedFeature->forAll(f |
    f <> result implies 
        f.direction = FeatureDirectionKind::_'in')
```

### validateInvocationExpressionParameterRedefinition

Each input parameter of an InvocationExpression must redefine exactly one input parameter of the instantiatedType of the InvocationExpression.

```ocl
let parameters : OrderedSet(Feature) = instantiatedType.input in
input->forAll(inp | 
    inp.ownedRedefinition.redefinedFeature->
        intersection(parameters)->size() = 1)
```

