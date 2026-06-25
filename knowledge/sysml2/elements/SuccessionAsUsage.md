---
name: SuccessionAsUsage
package: Connections
fully qualified name: SysML::Systems::Connections::SuccessionAsUsage
isAbstract: false
visibility: public
generalizes: [ConnectorAsUsage, Succession]
specializedBy: []
---

# SuccessionAsUsage

`Connections` package · concrete metaclass

A SuccessionAsUsage is both a ConnectorAsUsage and a Succession.

## Generalizations

- [ConnectorAsUsage](ConnectorAsUsage.md)
- [Succession](Succession.md)

## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| association | [Association](Association.md) | [0..*] | [Connector](Connector.md) | derived, ordered |
| chainingFeature | [Feature](Feature.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| connectorEnd | [Feature](Feature.md) | [0..*] | [Connector](Connector.md) | derived, ordered |
| crossFeature | [Feature](Feature.md) | [0..1] | [Feature](Feature.md) | derived |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| defaultFeaturingType | [Type](Type.md) | [0..1] | [Connector](Connector.md) | derived |
| definition | [Classifier](Classifier.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| differencingType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedFeature | [Feature](Feature.md) | [0..*] | [Type](Type.md) | derived, ordered |
| directedUsage | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
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
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| isOrdered | Boolean | [1..1] | [Feature](Feature.md) |  |
| isPortion | Boolean | [1..1] | [Feature](Feature.md) |  |
| isReference | Boolean | [1..1] | [Usage](Usage.md) | derived |
| isSufficient | Boolean | [1..1] | [Type](Type.md) |  |
| isUnique | Boolean | [1..1] | [Feature](Feature.md) |  |
| isVariable | Boolean | [1..1] | [Feature](Feature.md) |  |
| isVariation | Boolean | [1..1] | [Usage](Usage.md) |  |
| mayTimeVary | Boolean | [1..1] | [Usage](Usage.md) | derived |
| member | [Element](Element.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| membership | [Membership](Membership.md) | [0..*] | [Namespace](Namespace.md) | derived, ordered |
| multiplicity | [Multiplicity](Multiplicity.md) | [0..1] | [Type](Type.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| nestedAction | [ActionUsage](ActionUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAllocation | [AllocationUsage](AllocationUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAnalysisCase | [AnalysisCaseUsage](AnalysisCaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedAttribute | [AttributeUsage](AttributeUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedCalculation | [CalculationUsage](CalculationUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedCase | [CaseUsage](CaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedConcern | [ConcernUsage](ConcernUsage.md) | [0..*] | [Usage](Usage.md) | derived |
| nestedConnection | [ConnectorAsUsage](ConnectorAsUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedConstraint | [ConstraintUsage](ConstraintUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedEnumeration | [EnumerationUsage](EnumerationUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedFlow | [FlowUsage](FlowUsage.md) | [0..*] | [Usage](Usage.md) | derived |
| nestedInterface | [InterfaceUsage](InterfaceUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedItem | [ItemUsage](ItemUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedMetadata | [MetadataUsage](MetadataUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedOccurrence | [OccurrenceUsage](OccurrenceUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedPart | [PartUsage](PartUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedPort | [PortUsage](PortUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedReference | [ReferenceUsage](ReferenceUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedRendering | [RenderingUsage](RenderingUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedRequirement | [RequirementUsage](RequirementUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedState | [StateUsage](StateUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedTransition | [TransitionUsage](TransitionUsage.md) | [0..*] | [Usage](Usage.md) | derived |
| nestedUsage | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedUseCase | [UseCaseUsage](UseCaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedVerificationCase | [VerificationCaseUsage](VerificationCaseUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedView | [ViewUsage](ViewUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| nestedViewpoint | [ViewpointUsage](ViewpointUsage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
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
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| ownedSpecialization | [Specialization](Specialization.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| ownedSubsetting | [Subsetting](Subsetting.md) | [0..*] | [Feature](Feature.md) | derived, composite |
| ownedTypeFeaturing | [TypeFeaturing](TypeFeaturing.md) | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedTyping | [FeatureTyping](FeatureTyping.md) | [0..*] | [Feature](Feature.md) | derived, composite, ordered |
| ownedUnioning | [Unioning](Unioning.md) | [0..*] | [Type](Type.md) | derived, composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningDefinition | [Definition](Definition.md) | [0..1] | [Usage](Usage.md) | derived |
| owningFeatureMembership | [FeatureMembership](FeatureMembership.md) | [0..1] | [Feature](Feature.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [0..1] | [Feature](Feature.md) | derived |
| owningUsage | [Usage](Usage.md) | [0..1] | [Usage](Usage.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| relatedFeature | [Feature](Feature.md) | [0..*] | [Connector](Connector.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| sourceFeature | [Feature](Feature.md) | [0..1] | [Connector](Connector.md) | derived, ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| targetFeature | [Feature](Feature.md) | [0..*] | [Connector](Connector.md) | derived, ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| type | [Type](Type.md) | [0..*] | [Feature](Feature.md) | derived, ordered |
| unioningType | [Type](Type.md) | [0..*] | [Type](Type.md) | derived, ordered |
| usage | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived, ordered |
| variant | [Usage](Usage.md) | [0..*] | [Usage](Usage.md) | derived |
| variantMembership | [VariantMembership](VariantMembership.md) | [0..*] | [Usage](Usage.md) | derived, composite |
