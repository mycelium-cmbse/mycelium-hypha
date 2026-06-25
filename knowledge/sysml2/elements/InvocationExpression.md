---
name: InvocationExpression
package: Expressions
fully qualified name: KerML::Kernel::Expressions::InvocationExpression
isAbstract: false
visibility: public
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
| feature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
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

