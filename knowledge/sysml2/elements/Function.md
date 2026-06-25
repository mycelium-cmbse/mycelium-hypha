---
name: Function
package: Functions
isAbstract: false
generalizes: [Behavior]
specializedBy: [CalculationDefinition, Predicate]
---

# Function

`Functions` package · concrete metaclass

A Function is a Behavior that has an out parameter that is identified as its result. A Function represents the performance of a calculation that produces the values of its result parameter. This calculation may be decomposed into Expressions that are steps of the Function.

## Generalizations

- [Behavior](Behavior.md)

## Specializations

- [CalculationDefinition](CalculationDefinition.md)
- [Predicate](Predicate.md)

## Owned features

### expression : Expression [0..*] {derived}

The Expressions that are steps in the calculation of the result of this Function.

Subsets: `step`

### isModelLevelEvaluable : Boolean [1..1] {derived}

Whether this Function can be used as the function of a model-level evaluable InvocationExpression. Certain Functions from the Kernel Functions Library are considered to have isModelLevelEvaluable = true. For all other Functions it is false.<strong>Note:</strong> See the specification of the KerML concrete syntax notation for Expressions for an identification of which library Functions are model-level evaluable.

### result : Feature [1..1] {derived}

The object or value that is the result of evaluating the Function.

Subsets: `output`, `parameter`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| differencingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| endFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| feature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| featureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, ordered |
| importedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | Membership | [0..*] | [Type](Type.md) | derived, ordered |
| input | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | Type | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | Boolean | [1..1] | [Type](Type.md) |  |
| isConjugated | Boolean | [1..1] | [Type](Type.md) | derived |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isSufficient | Boolean | [1..1] | [Type](Type.md) |  |
| member | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | Membership | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | Multiplicity | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| output | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedConjugator | Conjugation | [0..1] | [Type](Type.md) | derived, composite |
| ownedDifferencing | Differencing | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedDisjoining | Disjoining | [0..*] | [Type](Type.md) | derived, composite |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedEndFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeature | Feature | [0..*] | [Type](Type.md) | derived, ordered |
| ownedFeatureMembership | FeatureMembership | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedImport | Import | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedIntersecting | Intersecting | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedMember | Element | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| ownedMembership | Membership | [0..*] | [Namespace](Namespace.md) | derived, composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | Specialization | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubclassification | Subclassification | [0..*] | [Classifier](Classifier.md) | derived, composite |
| ownedUnioning | Unioning | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| parameter | Feature | [0..*] | [Behavior](Behavior.md) | derived, ordered |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| step | Step | [0..*] | [Behavior](Behavior.md) | derived |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| unioningType | Type | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkFunctionResultBindingConnector

If a Function has an Expression owned via a ResultExpressionMembership, then the owning Function must also own a BindingConnector between its result parameter and the result parameter of the result Expression.

```ocl
ownedMembership.selectByKind(ResultExpressionMembership)->
    forAll(mem | ownedFeature.selectByKind(BindingConnector)->
        exists(binding |
            binding.relatedFeature->includes(result) and
            binding.relatedFeature->includes(mem.ownedResultExpression.result)))
```

### checkFunctionSpecialization

A Function must directly or indirectly specialize the base Function Performances::Evaluation from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Performances::Evaluation')
```

### deriveFunctionResult

The result parameter of a Function is its parameter owned (possibly in a supertype) via a ReturnParameterMembership (if any).

```ocl
result =
    let resultParams : Sequence(Feature) =
        featureMemberships->
            selectByKind(ReturnParameterMembership).
            ownedMemberParameter in
    if resultParams->notEmpty() then resultParams->first()
    else null
    endif
```

### validateFunctionResultExpressionMembership

A Function must have at most one ResultExpressionMembership.

```ocl
membership->selectByKind(ResultExpressionMembership)->size() <= 1
```

### validateFunctionResultParameterMembership

A Function must have exactly one featureMembership (owned or inherited) that is a ResultParameterMembership.

```ocl
featureMembership->
    selectByKind(ReturnParameterMembership)->
    size() = 1
```

