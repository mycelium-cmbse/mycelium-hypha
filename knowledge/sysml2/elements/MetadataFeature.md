---
name: MetadataFeature
package: Metadata
fully qualified name: KerML::Kernel::Metadata::MetadataFeature
isAbstract: false
visibility: public
generalizes: [AnnotatingElement, Feature]
specializedBy: [MetadataUsage]
---

# MetadataFeature

`Metadata` package · concrete metaclass

A MetadataFeature is a Feature that is an AnnotatingElement used to annotate another Element with metadata. It is typed by a Metaclass. All its ownedFeatures must redefine features of its metaclass and any feature bindings must be model-level evaluable.

## Generalizations

- [AnnotatingElement](AnnotatingElement.md)
- [Feature](Feature.md)

## Specializations

- [MetadataUsage](MetadataUsage.md)

## Owned features

### metaclass

`+` [Metaclass](Metaclass.md) · `[0..1]` · *derived*

The type of this MetadataFeature, which must be a Metaclass.

Subsets [type](Feature.md#type)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| annotatedElement | [Element](Element.md) | [1..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| annotation | [Annotation](Annotation.md) | [0..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
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
| importedMembership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| inheritedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| inheritedMembership | [Membership](Membership.md) | [0..*] | [Type](Type.md) | derived, ordered |
| input | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| intersectingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| isAbstract | Boolean | [1..1] | [Type](Type.md) |  |
| isComposite | Boolean | [1..1] | [Feature](Feature.md) |  |
| isConjugated | Boolean | [1..1] | [Type](Type.md) | derived |
| isConstant | Boolean | [1..1] | [Feature](Feature.md) |  |
| isDerived | Boolean | [1..1] | [Feature](Feature.md) |  |
| isEnd | Boolean | [1..1] | [Feature](Feature.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
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
| ownedAnnotatingRelationship | [Annotation](Annotation.md) | [0..*] | [AnnotatingElement](AnnotatingElement.md) | derived, composite, ordered |
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
| owningAnnotatingRelationship | [Annotation](Annotation.md) | [0..1] | [AnnotatingElement](AnnotatingElement.md) | derived |
| owningFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..1] | [Feature](Feature.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| type | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |

## Constraints

### checkMetadataFeatureSemanticSpecialization

If this MetadataFeature is an application of SemanticMetadata, then its annotatingElement must be a Type. The annotated Type must then directly or indirectly specialize the specified value of the baseType, unless the Type is a Classifier and the baseType represents a kind of Feature, in which case the Classifier must directly or indirectly specialize each of the types of the Feature.

```ocl
isSemantic() implies
    let annotatedTypes : Sequence(Type) = 
        annotatedElement->selectAsKind(Type) in
    let baseTypes : Sequence(MetadataFeature) = 
        evaluateFeature(resolveGlobal(
            'Metaobjects::SemanticMetadata::baseType').
            memberElement.
            oclAsType(Feature))->
        selectAsKind(MetadataFeature) in
    annotatedTypes->notEmpty() and 
    baseTypes()->notEmpty() and 
    baseTypes()->first().isSyntactic() implies
        let annotatedType : Type = annotatedTypes->first() in
        let baseType : Element = baseTypes->first().syntaxElement() in
        if annotatedType.oclIsKindOf(Classifier) and 
            baseType.oclIsKindOf(Feature) then
            baseType.oclAsType(Feature).type->
                forAll(t | annotatedType.specializes(t))
        else if baseType.oclIsKindOf(Type) then
            annotatedType.specializes(baseType.oclAsType(Type))
        else
            true
        endif
```

### checkMetadataFeatureSpecialization

A MetadataFeature must directly or indirectly specialize the base MetadataFeature Metaobjects::metaobjects from the Kernel Semantic Library.

```ocl
specializesFromLibrary('Metaobjects::metaobjects')
```

### deriveMetadataFeatureMetaclass

The metaclass of a MetadataFeature is one of its types that is a Metaclass.

```ocl
metaclass = 
    let metaclassTypes : Sequence(Type) = type->selectByKind(Metaclass) in
    if metaclassTypes->isEmpty() then null
    else metaClassTypes->first()
    endif
```

### validateMetadataFeatureAnnotatedElement

The annotatedElements of a MetadataFeature must have an abstract syntax metaclass consistent with the annotatedElement declarations for the MetadataFeature.

```ocl
let baseAnnotatedElementFeature : Feature =
    resolveGlobal('Metaobjects::Metaobject::annotatedElement').memberElement.
    oclAsType(Feature) in
let annotatedElementFeatures : OrderedSet(Feature) = feature->
    select(specializes(baseAnnotatedElementFeature))->
    excluding(baseAnnotatedElementFeature) in
annotatedElementFeatures->notEmpty() implies
    let annotatedElementTypes : Set(Feature) =
        annotatedElementFeatures.typing.type->asSet() in
    let metaclasses : Set(Metaclass) =
        annotatedElement.oclType().qualifiedName->collect(qn | 
            resolveGlobal(qn).memberElement.oclAsType(Metaclass)) in
   metaclasses->forAll(m | annotatedElementTypes->exists(t | m.specializes(t)))
```

### validateMetadataFeatureBody

Each ownedFeature of a MetadataFeature must have no declared name, redefine a single Feature, either have no featureValue or a featureValue with a value Expression that is model-level evaluable, and only have ownedFeatures that also meet these restrictions.

```ocl
ownedFeature->closure(ownedFeature)->forAll(f |
    f.declaredName = null and f.declaredShortName = null and
    f.valuation <> null implies f.valuation.value.isModelLevelEvaluable and
    f.redefinition.redefinedFeature->size() = 1)
```

### validateMetadataFeatureMetaclass

A MetadataFeature must have exactly one type that is a Metaclass.

```ocl
type->selectByKind(Metaclass).size() = 1
```

### validateMetadataFeatureMetaclassNotAbstract

The metaclass of a MetadataFeature must not be abstract.

```ocl
not metaclass.isAbstract
```

