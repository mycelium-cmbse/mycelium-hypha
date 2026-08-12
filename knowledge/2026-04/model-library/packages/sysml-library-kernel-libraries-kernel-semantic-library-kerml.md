---
name: KerML
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Semantic Library/KerML.kerml
declares: [KerML, KerML::Root, KerML::Root::AnnotatingElement, KerML::Root::AnnotatingElement::annotatedElement, KerML::Root::AnnotatingElement::ownedAnnotatingRelationship, KerML::Root::AnnotatingElement::owningAnnotatingRelationship, KerML::Root::AnnotatingElement::annotation, KerML::Root::Annotation, KerML::Root::Annotation::annotatedElement, KerML::Root::Annotation::annotatingElement, KerML::Root::Annotation::owningAnnotatedElement, KerML::Root::Annotation::owningAnnotatingElement, KerML::Root::Annotation::ownedAnnotatingElement, KerML::Root::Comment, KerML::Root::Comment::locale, KerML::Root::Comment::body, KerML::Root::Dependency, KerML::Root::Dependency::client, KerML::Root::Dependency::supplier, KerML::Root::Documentation, KerML::Root::Documentation::documentedElement, KerML::Root::Element, KerML::Root::Element::elementId, KerML::Root::Element::aliasIds, KerML::Root::Element::declaredShortName, KerML::Root::Element::declaredName, KerML::Root::Element::isImpliedIncluded, KerML::Root::Element::shortName, KerML::Root::Element::name, KerML::Root::Element::qualifiedName, KerML::Root::Element::isLibraryElement, KerML::Root::Element::owningRelationship, KerML::Root::Element::ownedRelationship, KerML::Root::Element::owningMembership, KerML::Root::Element::owningNamespace, KerML::Root::Element::owner, KerML::Root::Element::ownedElement, KerML::Root::Element::documentation, KerML::Root::Element::ownedAnnotation, KerML::Root::Element::textualRepresentation, KerML::Root::Import, KerML::Root::Import::visibility, KerML::Root::Import::isRecursive, KerML::Root::Import::isImportAll, KerML::Root::Import::importOwningNamespace, KerML::Root::Import::importedElement, KerML::Root::Membership, KerML::Root::Membership::memberShortName, KerML::Root::Membership::memberName, KerML::Root::Membership::visibility, KerML::Root::Membership::memberElementId, KerML::Root::Membership::memberElement, KerML::Root::Membership::membershipOwningNamespace, KerML::Root::MembershipImport, KerML::Root::MembershipImport::importedMembership, KerML::Root::Namespace, KerML::Root::Namespace::membership, KerML::Root::Namespace::ownedImport, KerML::Root::Namespace::member, KerML::Root::Namespace::ownedMember, KerML::Root::Namespace::ownedMembership, KerML::Root::Namespace::importedMembership, KerML::Root::NamespaceImport, KerML::Root::NamespaceImport::importedNamespace, KerML::Root::OwningMembership, KerML::Root::OwningMembership::ownedMemberElementId, KerML::Root::OwningMembership::ownedMemberShortName, KerML::Root::OwningMembership::ownedMemberName, KerML::Root::OwningMembership::ownedMemberElement, KerML::Root::Relationship, KerML::Root::Relationship::isImplied, KerML::Root::Relationship::target, KerML::Root::Relationship::source, KerML::Root::Relationship::owningRelatedElement, KerML::Root::Relationship::ownedRelatedElement, KerML::Root::Relationship::relatedElement, KerML::Root::TextualRepresentation, KerML::Root::TextualRepresentation::language, KerML::Root::TextualRepresentation::body, KerML::Root::TextualRepresentation::representedElement, KerML::Root::VisibilityKind, KerML::Root::VisibilityKind::private, KerML::Root::VisibilityKind::protected, KerML::Root::VisibilityKind::public, KerML::Core, KerML::Core::Classifier, KerML::Core::Classifier::ownedSubclassification, KerML::Core::Conjugation, KerML::Core::Conjugation::originalType, KerML::Core::Conjugation::conjugatedType, KerML::Core::Conjugation::owningType, KerML::Core::CrossSubsetting, KerML::Core::CrossSubsetting::crossedFeature, KerML::Core::CrossSubsetting::crossingFeature, KerML::Core::Differencing, KerML::Core::Differencing::differencingType, KerML::Core::Differencing::typeDifferenced, KerML::Core::Disjoining, KerML::Core::Disjoining::typeDisjoined, KerML::Core::Disjoining::disjoiningType, KerML::Core::Disjoining::owningType, KerML::Core::EndFeatureMembership, KerML::Core::EndFeatureMembership::ownedMemberFeature, KerML::Core::Feature, KerML::Core::Feature::isUnique, KerML::Core::Feature::isOrdered, KerML::Core::Feature::isComposite, KerML::Core::Feature::isEnd, KerML::Core::Feature::isDerived, KerML::Core::Feature::isPortion, KerML::Core::Feature::isVariable, KerML::Core::Feature::isConstant, KerML::Core::Feature::direction, KerML::Core::Feature::owningType, KerML::Core::Feature::type, KerML::Core::Feature::ownedRedefinition, KerML::Core::Feature::ownedSubsetting, KerML::Core::Feature::owningFeatureMembership, KerML::Core::Feature::endOwningType, KerML::Core::Feature::ownedTyping, KerML::Core::Feature::featuringType, KerML::Core::Feature::ownedTypeFeaturing, KerML::Core::Feature::chainingFeature, KerML::Core::Feature::ownedFeatureInverting, KerML::Core::Feature::ownedFeatureChaining, KerML::Core::Feature::ownedReferenceSubsetting, KerML::Core::Feature::featureTarget, KerML::Core::Feature::crossFeature, KerML::Core::Feature::ownedCrossSubsetting, KerML::Core::FeatureChaining, KerML::Core::FeatureChaining::chainingFeature, KerML::Core::FeatureChaining::featureChained, KerML::Core::FeatureDirectionKind, KerML::Core::FeatureDirectionKind::in, KerML::Core::FeatureDirectionKind::inout, KerML::Core::FeatureDirectionKind::out, KerML::Core::FeatureInverting, KerML::Core::FeatureInverting::featureInverted, KerML::Core::FeatureInverting::invertingFeature, KerML::Core::FeatureInverting::owningFeature, KerML::Core::FeatureMembership, KerML::Core::FeatureMembership::owningType, KerML::Core::FeatureMembership::ownedMemberFeature, KerML::Core::FeatureTyping, KerML::Core::FeatureTyping::typedFeature, KerML::Core::FeatureTyping::type, KerML::Core::FeatureTyping::owningFeature, KerML::Core::Intersecting, KerML::Core::Intersecting::intersectingType, KerML::Core::Intersecting::typeIntersected, KerML::Core::Multiplicity, KerML::Core::Redefinition, KerML::Core::Redefinition::redefiningFeature, KerML::Core::Redefinition::redefinedFeature, KerML::Core::ReferenceSubsetting, KerML::Core::ReferenceSubsetting::referencedFeature, KerML::Core::ReferenceSubsetting::referencingFeature, KerML::Core::Specialization, KerML::Core::Specialization::general, KerML::Core::Specialization::specific, KerML::Core::Specialization::owningType, KerML::Core::Subclassification, KerML::Core::Subclassification::superclassifier, KerML::Core::Subclassification::subclassifier, KerML::Core::Subclassification::owningClassifier, KerML::Core::Subsetting, KerML::Core::Subsetting::subsettedFeature, KerML::Core::Subsetting::subsettingFeature, KerML::Core::Subsetting::owningFeature, KerML::Core::Type, KerML::Core::Type::isAbstract, KerML::Core::Type::isSufficient, KerML::Core::Type::isConjugated, KerML::Core::Type::ownedSpecialization, KerML::Core::Type::ownedFeatureMembership, KerML::Core::Type::feature, KerML::Core::Type::ownedFeature, KerML::Core::Type::input, KerML::Core::Type::output, KerML::Core::Type::inheritedMembership, KerML::Core::Type::endFeature, KerML::Core::Type::ownedEndFeature, KerML::Core::Type::ownedConjugator, KerML::Core::Type::inheritedFeature, KerML::Core::Type::multiplicity, KerML::Core::Type::unioningType, KerML::Core::Type::ownedIntersecting, KerML::Core::Type::intersectingType, KerML::Core::Type::ownedUnioning, KerML::Core::Type::ownedDisjoining, KerML::Core::Type::featureMembership, KerML::Core::Type::differencingType, KerML::Core::Type::ownedDifferencing, KerML::Core::Type::directedFeature, KerML::Core::TypeFeaturing, KerML::Core::TypeFeaturing::featureOfType, KerML::Core::TypeFeaturing::featuringType, KerML::Core::TypeFeaturing::owningFeatureOfType, KerML::Core::Unioning, KerML::Core::Unioning::unioningType, KerML::Core::Unioning::typeUnioned, KerML::Kernel, KerML::Kernel::Association, KerML::Kernel::Association::relatedType, KerML::Kernel::Association::sourceType, KerML::Kernel::Association::targetType, KerML::Kernel::Association::associationEnd, KerML::Kernel::AssociationStructure, KerML::Kernel::Behavior, KerML::Kernel::Behavior::step, KerML::Kernel::Behavior::parameter, KerML::Kernel::BindingConnector, KerML::Kernel::BooleanExpression, KerML::Kernel::BooleanExpression::predicate, KerML::Kernel::Class, KerML::Kernel::CollectExpression, KerML::Kernel::CollectExpression::operator, KerML::Kernel::Connector, KerML::Kernel::Connector::relatedFeature, KerML::Kernel::Connector::association, KerML::Kernel::Connector::connectorEnd, KerML::Kernel::Connector::sourceFeature, KerML::Kernel::Connector::targetFeature, KerML::Kernel::Connector::defaultFeaturingType, KerML::Kernel::ConstructorExpression, KerML::Kernel::DataType, KerML::Kernel::ElementFilterMembership, KerML::Kernel::ElementFilterMembership::condition, KerML::Kernel::Expression, KerML::Kernel::Expression::isModelLevelEvaluable, KerML::Kernel::Expression::function, KerML::Kernel::Expression::result, KerML::Kernel::FeatureChainExpression, KerML::Kernel::FeatureChainExpression::operator, KerML::Kernel::FeatureChainExpression::targetFeature, KerML::Kernel::FeatureReferenceExpression, KerML::Kernel::FeatureReferenceExpression::referent, KerML::Kernel::FeatureValue, KerML::Kernel::FeatureValue::isInitial, KerML::Kernel::FeatureValue::isDefault, KerML::Kernel::FeatureValue::featureWithValue, KerML::Kernel::FeatureValue::value, KerML::Kernel::Flow, KerML::Kernel::Flow::payloadType, KerML::Kernel::Flow::targetInputFeature, KerML::Kernel::Flow::sourceOutputFeature, KerML::Kernel::Flow::flowEnd, KerML::Kernel::Flow::payloadFeature, KerML::Kernel::Flow::interaction, KerML::Kernel::FlowEnd, KerML::Kernel::Function, KerML::Kernel::Function::isModelLevelEvaluable, KerML::Kernel::Function::expression, KerML::Kernel::Function::result, KerML::Kernel::IndexExpression, KerML::Kernel::IndexExpression::operator, KerML::Kernel::InstantiationExpression, KerML::Kernel::InstantiationExpression::argument, KerML::Kernel::InstantiationExpression::instantiatedType, KerML::Kernel::Interaction, KerML::Kernel::Invariant, KerML::Kernel::Invariant::isNegated, KerML::Kernel::InvocationExpression, KerML::Kernel::LibraryPackage, KerML::Kernel::LibraryPackage::isStandard, KerML::Kernel::LiteralBoolean, KerML::Kernel::LiteralBoolean::value, KerML::Kernel::LiteralExpression, KerML::Kernel::LiteralInfinity, KerML::Kernel::LiteralInteger, KerML::Kernel::LiteralInteger::value, KerML::Kernel::LiteralRational, KerML::Kernel::LiteralRational::value, KerML::Kernel::LiteralString, KerML::Kernel::LiteralString::value, KerML::Kernel::Metaclass, KerML::Kernel::MetadataAccessExpression, KerML::Kernel::MetadataAccessExpression::referencedElement, KerML::Kernel::MetadataFeature, KerML::Kernel::MetadataFeature::metaclass, KerML::Kernel::MultiplicityRange, KerML::Kernel::MultiplicityRange::lowerBound, KerML::Kernel::MultiplicityRange::upperBound, KerML::Kernel::MultiplicityRange::bound, KerML::Kernel::NullExpression, KerML::Kernel::OperatorExpression, KerML::Kernel::OperatorExpression::operator, KerML::Kernel::Package, KerML::Kernel::Package::filterCondition, KerML::Kernel::ParameterMembership, KerML::Kernel::ParameterMembership::ownedMemberParameter, KerML::Kernel::PayloadFeature, KerML::Kernel::Predicate, KerML::Kernel::ResultExpressionMembership, KerML::Kernel::ResultExpressionMembership::ownedResultExpression, KerML::Kernel::ReturnParameterMembership, KerML::Kernel::SelectExpression, KerML::Kernel::SelectExpression::operator, KerML::Kernel::Step, KerML::Kernel::Step::behavior, KerML::Kernel::Step::parameter, KerML::Kernel::Structure, KerML::Kernel::Succession, KerML::Kernel::SuccessionFlow]
license: EPL-2.0
---

# KerML

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Semantic Library/KerML.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package KerML {
	doc 
	/*
	 * This package contains a reflective KerML model of the KerML abstract syntax.
	 */
	 
	private import ScalarValues::*;
	public import Kernel::*;
	
	package Root {
		metaclass AnnotatingElement specializes Element {
			derived var feature annotatedElement : Element[1..*] ordered redefines annotatedElement;
			derived composite var feature ownedAnnotatingRelationship : Annotation[0..*] ordered subsets annotation, ownedRelationship;
			derived var feature owningAnnotatingRelationship : Annotation[0..1] subsets owningRelationship, annotation;
			derived var feature annotation : Annotation[0..*] ordered;
		}		
		
		metaclass Annotation specializes Relationship {
			var feature annotatedElement : Element[1..1] redefines target, annotatedElement;
			derived var feature annotatingElement : AnnotatingElement[1..1] redefines source;
			derived var feature owningAnnotatedElement : Element[0..1] subsets annotatedElement, owningRelatedElement;
			derived var feature owningAnnotatingElement : AnnotatingElement[0..1] subsets annotatingElement, owningRelatedElement;
			derived composite var feature ownedAnnotatingElement : AnnotatingElement[0..1] subsets annotatingElement, ownedRelatedElement;
		}		
		
		metaclass Comment specializes AnnotatingElement {
			var feature 'locale' : String[0..1];
			var feature body : String[1..1];
		}		
		
		metaclass Dependency specializes Relationship {
			var feature client : Element[1..*] ordered redefines source;
			var feature supplier : Element[1..*] ordered redefines target;
		}		
		
		metaclass Documentation specializes Comment {
			derived var feature documentedElement : Element[1..1] subsets owner redefines annotatedElement;
		}		
		
		abstract metaclass Element {
			var feature elementId : String[1..1];
			var feature aliasIds : String[0..*] ordered;
			var feature declaredShortName : String[0..1];
			var feature declaredName : String[0..1];
			var feature isImpliedIncluded : Boolean[1..1];
			derived var feature shortName : String[0..1];
			derived var feature name : String[0..1];
			derived var feature qualifiedName : String[0..1];
			derived var feature isLibraryElement : Boolean[1..1];
			
			var feature owningRelationship : Relationship[0..1];
			composite var feature ownedRelationship : Relationship[0..*] ordered;
			derived var feature owningMembership : OwningMembership[0..1] subsets owningRelationship;
			derived var feature owningNamespace : Namespace[0..1];
			derived var feature owner : Element[0..1];
			derived var feature ownedElement : Element[0..*] ordered;
			derived var feature documentation : Documentation[0..*] ordered subsets ownedElement;
			derived composite var feature ownedAnnotation : Annotation[0..*] ordered subsets ownedRelationship;
			derived var feature textualRepresentation : TextualRepresentation[0..*] ordered subsets ownedElement;
		}		
		
		abstract metaclass Import specializes Relationship {
			var feature visibility : VisibilityKind[1..1];
			var feature isRecursive : Boolean[1..1];
			var feature isImportAll : Boolean[1..1];
			
			derived var feature importOwningNamespace : Namespace[1..1] subsets owningRelatedElement redefines source;
			derived var feature importedElement : Element[1..1];
		}		
		
		metaclass Membership specializes Relationship {
			var feature memberShortName : String[0..1];
			var feature memberName : String[0..1];
			var feature visibility : VisibilityKind[1..1];
			derived var feature memberElementId : String[1..1];
			
			var feature memberElement : Element[1..1] redefines target;
			derived var feature membershipOwningNamespace : Namespace[1..1] subsets owningRelatedElement redefines source;
		}		
		
		metaclass MembershipImport specializes Import {
			var feature importedMembership : Membership[1..1] redefines target;
		}		
		
		metaclass Namespace specializes Element {
			derived abstract var feature membership : Membership[0..*] ordered;
			derived composite var feature ownedImport : Import[0..*] ordered subsets ownedRelationship;
			derived var feature 'member' : Element[0..*] ordered;
			derived var feature ownedMember : Element[0..*] ordered subsets 'member';
			derived composite var feature ownedMembership : Membership[0..*] ordered subsets membership, ownedRelationship;
			derived var feature importedMembership : Membership[0..*] ordered subsets membership;
		}		
		
		metaclass NamespaceImport specializes Import {
			var feature importedNamespace : Namespace[1..1] redefines target;
		}		
		
		metaclass OwningMembership specializes Membership {
			derived var feature ownedMemberElementId : String[1..1] redefines memberElementId;
			derived var feature ownedMemberShortName : String[0..1] redefines memberShortName;
			derived var feature ownedMemberName : String[0..1] redefines memberName;
			
			derived composite var feature ownedMemberElement : Element[1..1] subsets ownedRelatedElement redefines memberElement;
		}		
		
		abstract metaclass Relationship specializes Element {
			var feature isImplied : Boolean[1..1];
			
			var feature target : Element[0..*] ordered subsets relatedElement;
			var feature source : Element[0..*] ordered subsets relatedElement;
			var feature owningRelatedElement : Element[0..1] subsets relatedElement;
			composite var feature ownedRelatedElement : Element[0..*] ordered subsets relatedElement;
			derived var feature relatedElement : Element[0..*] ordered nonunique;
		}		
		
		metaclass TextualRepresentation specializes AnnotatingElement {
			var feature 'language' : String[1..1];
			var feature body : String[1..1];
			
			derived var feature representedElement : Element[1..1] subsets owner redefines annotatedElement;
		}		
		
		datatype VisibilityKind {
			member feature 'private' : VisibilityKind[1];
			member feature 'protected' : VisibilityKind[1];
			member feature 'public' : VisibilityKind[1];
		}
		
	}
	
	package Core {
		public import Root::*;
		
		metaclass Classifier specializes Type {
			derived composite var feature ownedSubclassification : Subclassification[0..*] subsets ownedSpecialization;
		}		
		
		metaclass Conjugation specializes Relationship {
			var feature originalType : Type[1..1] redefines target;
			var feature conjugatedType : Type[1..1] redefines source;
			derived var feature owningType : Type[0..1] subsets conjugatedType, owningRelatedElement;
		}		
		
		metaclass CrossSubsetting specializes Subsetting {
			var feature crossedFeature : Feature[1..1] redefines subsettedFeature;
			derived var feature crossingFeature : Feature[1..1] redefines owningFeature, subsettingFeature;
		}		
		
		metaclass Differencing specializes Relationship {
			var feature differencingType : Type[1..1] redefines target;
			derived var feature typeDifferenced : Type[1..1] subsets owningRelatedElement redefines source;
		}		
		
		metaclass Disjoining specializes Relationship {
			var feature typeDisjoined : Type[1..1] redefines source;
			var feature disjoiningType : Type[1..1] redefines target;
			derived var feature owningType : Type[0..1] subsets owningRelatedElement, typeDisjoined;
		}		
		
		metaclass EndFeatureMembership specializes FeatureMembership {
			derived composite var feature ownedMemberFeature : Feature[1..1] redefines ownedMemberFeature;
		}		
		
		metaclass Feature specializes Type {
			var feature isUnique : Boolean[1..1];
			var feature isOrdered : Boolean[1..1];
			var feature isComposite : Boolean[1..1];
			var feature isEnd : Boolean[1..1];
			var feature isDerived : Boolean[1..1];
			var feature isPortion : Boolean[1..1];
			var feature isVariable : Boolean[1..1];
			var feature isConstant : Boolean[1..1];
			var feature direction : FeatureDirectionKind[0..1];
			
			derived var feature owningType : Type[0..1] subsets owningNamespace, featuringType;
			derived var feature 'type' : Type[0..*] ordered;
			derived composite var feature ownedRedefinition : Redefinition[0..*] subsets ownedSubsetting;
			derived composite var feature ownedSubsetting : Subsetting[0..*] subsets ownedSpecialization;
			derived var feature owningFeatureMembership : FeatureMembership[0..1] subsets owningMembership;
			derived var feature endOwningType : Type[0..1] subsets owningType;
			derived composite var feature ownedTyping : FeatureTyping[0..*] ordered subsets ownedSpecialization;
			derived var feature featuringType : Type[0..*] ordered;
			derived composite var feature ownedTypeFeaturing : TypeFeaturing[0..*] ordered subsets ownedRelationship;
			derived var feature chainingFeature : Feature[0..*] ordered nonunique;
			derived composite var feature ownedFeatureInverting : FeatureInverting[0..*] subsets ownedRelationship;
			derived composite var feature ownedFeatureChaining : FeatureChaining[0..*] ordered subsets ownedRelationship;
			derived composite var feature ownedReferenceSubsetting : ReferenceSubsetting[0..1] subsets ownedSubsetting;
			derived var feature featureTarget : Feature[1..1];
			derived var feature crossFeature : Feature[0..1];
			derived composite var feature ownedCrossSubsetting : CrossSubsetting[0..1] subsets ownedSubsetting;
		}		
		
		metaclass FeatureChaining specializes Relationship {
			var feature chainingFeature : Feature[1..1] redefines target;
			derived var feature featureChained : Feature[1..1] subsets owningRelatedElement redefines source;
		}		
		
		datatype FeatureDirectionKind {
			member feature 'in' : FeatureDirectionKind[1];
			member feature 'inout' : FeatureDirectionKind[1];
			member feature 'out' : FeatureDirectionKind[1];
		}
		
		metaclass FeatureInverting specializes Relationship {
			var feature featureInverted : Feature[1..1] redefines source;
			var feature invertingFeature : Feature[1..1] redefines target;
			derived var feature owningFeature : Feature[0..1] subsets featureInverted, owningRelatedElement;
		}		
		
		metaclass FeatureMembership specializes OwningMembership {
			derived var feature owningType : Type[1..1] redefines membershipOwningNamespace;
			derived composite var feature ownedMemberFeature : Feature[1..1] redefines ownedMemberElement;
		}		
		
		metaclass FeatureTyping specializes Specialization {
			var feature typedFeature : Feature[1..1] redefines specific;
			var feature 'type' : Type[1..1] redefines general;
			derived var feature owningFeature : Feature[0..1] subsets typedFeature redefines owningType;
		}		
		
		metaclass Intersecting specializes Relationship {
			var feature intersectingType : Type[1..1] redefines target;
			derived var feature typeIntersected : Type[1..1] subsets owningRelatedElement redefines source;
		}		
		
		metaclass Multiplicity specializes Feature;		
		
		metaclass Redefinition specializes Subsetting {
			var feature redefiningFeature : Feature[1..1] redefines subsettingFeature;
			var feature redefinedFeature : Feature[1..1] redefines subsettedFeature;
		}		
		
		metaclass ReferenceSubsetting specializes Subsetting {
			var feature referencedFeature : Feature[1..1] redefines subsettedFeature;
			derived var feature referencingFeature : Feature[1..1] redefines owningFeature, subsettingFeature;
		}		
		
		metaclass Specialization specializes Relationship {
			var feature general : Type[1..1] redefines target;
			var feature specific : Type[1..1] redefines source;
			derived var feature owningType : Type[0..1] subsets owningRelatedElement, specific;
		}		
		
		metaclass Subclassification specializes Specialization {
			var feature superclassifier : Classifier[1..1] redefines general;
			var feature 'subclassifier' : Classifier[1..1] redefines specific;
			derived var feature owningClassifier : Classifier[0..1] redefines owningType;
		}		
		
		metaclass Subsetting specializes Specialization {
			var feature subsettedFeature : Feature[1..1] redefines general;
			var feature subsettingFeature : Feature[1..1] redefines specific;
			derived var feature owningFeature : Feature[0..1] subsets subsettingFeature redefines owningType;
		}		
		
		metaclass Type specializes Namespace {
			var feature isAbstract : Boolean[1..1];
			var feature isSufficient : Boolean[1..1];
			derived var feature isConjugated : Boolean[1..1];
			
			derived composite var feature ownedSpecialization : Specialization[0..*] ordered subsets ownedRelationship;
			derived composite var feature ownedFeatureMembership : FeatureMembership[0..*] ordered subsets ownedMembership, featureMembership;
			derived var feature 'feature' : Feature[0..*] ordered subsets 'member';
			derived var feature ownedFeature : Feature[0..*] ordered subsets ownedMember;
			derived var feature input : Feature[0..*] ordered subsets directedFeature;
			derived var feature output : Feature[0..*] ordered subsets directedFeature;
			derived var feature inheritedMembership : Membership[0..*] ordered subsets membership;
			derived var feature endFeature : Feature[0..*] ordered subsets 'feature';
			derived var feature ownedEndFeature : Feature[0..*] ordered subsets endFeature, ownedFeature;
			derived composite var feature ownedConjugator : Conjugation[0..1] subsets ownedRelationship;
			derived var feature inheritedFeature : Feature[0..*] ordered subsets 'feature';
			derived var feature 'multiplicity' : Multiplicity[0..1] subsets ownedMember;
			derived var feature unioningType : Type[0..*] ordered;
			derived composite var feature ownedIntersecting : Intersecting[0..*] ordered subsets ownedRelationship;
			derived var feature intersectingType : Type[0..*] ordered;
			derived composite var feature ownedUnioning : Unioning[0..*] ordered subsets ownedRelationship;
			derived composite var feature ownedDisjoining : Disjoining[0..*] subsets ownedRelationship;
			derived var feature featureMembership : FeatureMembership[0..*] ordered;
			derived var feature differencingType : Type[0..*] ordered;
			derived composite var feature ownedDifferencing : Differencing[0..*] ordered subsets ownedRelationship;
			derived var feature directedFeature : Feature[0..*] ordered subsets 'feature';
		}		
		
		metaclass TypeFeaturing specializes Relationship {
			var feature featureOfType : Feature[1..1] redefines source;
			var feature featuringType : Type[1..1] redefines target;
			derived var feature owningFeatureOfType : Feature[0..1] subsets owningRelatedElement, featureOfType;
		}		
		
		metaclass Unioning specializes Relationship {
			var feature unioningType : Type[1..1] redefines target;
			derived var feature typeUnioned : Type[1..1] subsets owningRelatedElement redefines source;
		}		
		
	}
	
	package Kernel {
		public import Core::*;
		
		metaclass Association specializes Classifier, Relationship {
			derived var feature relatedType : Type[0..*] ordered nonunique redefines relatedElement;
			derived var feature sourceType : Type[0..1] subsets relatedType redefines source;
			derived var feature targetType : Type[0..*] subsets relatedType redefines target;
			derived var feature associationEnd : Feature[0..*] redefines endFeature;
		}		
		
		metaclass AssociationStructure specializes Association, Structure;		
		
		metaclass Behavior specializes Class {
			derived var feature 'step' : Step[0..*] subsets 'feature';
			derived var feature parameter : Feature[0..*] ordered redefines directedFeature;
		}		
		
		metaclass BindingConnector specializes Connector;		
		
		metaclass BooleanExpression specializes Expression {
			derived var feature 'predicate' : Predicate[0..1] redefines 'function';
		}		
		
		metaclass Class specializes Classifier;		
		
		metaclass CollectExpression specializes OperatorExpression {
			var feature operator : String[1..1] redefines operator;
		}		
		
		metaclass Connector specializes Feature, Relationship {
			derived var feature relatedFeature : Feature[0..*] ordered nonunique redefines relatedElement;
			derived var feature association : Association[0..*] ordered redefines 'type';
			derived var feature connectorEnd : Feature[0..*] ordered redefines endFeature;
			derived var feature sourceFeature : Feature[0..1] ordered subsets relatedFeature redefines source;
			derived var feature targetFeature : Feature[0..*] ordered subsets relatedFeature redefines target;
			derived var feature defaultFeaturingType : Type[0..1];
		}		
		
		metaclass ConstructorExpression specializes InstantiationExpression;		
		
		metaclass DataType specializes Classifier;		
		
		metaclass ElementFilterMembership specializes OwningMembership {
			derived composite var feature condition : Expression[1..1] redefines ownedMemberElement;
		}		
		
		metaclass Expression specializes Step {
			derived var feature isModelLevelEvaluable : Boolean[1..1];
			
			derived var feature 'function' : Function[0..1] redefines 'behavior';
			derived var feature result : Feature[1..1] subsets output, parameter;
		}		
		
		metaclass FeatureChainExpression specializes OperatorExpression {
			var feature operator : String[1..1] redefines operator;
			
			derived var feature targetFeature : Feature[1..1] subsets 'member';
		}		
		
		metaclass FeatureReferenceExpression specializes Expression {
			derived var feature referent : Feature[1..1] subsets 'member';
		}		
		
		metaclass FeatureValue specializes OwningMembership {
			var feature isInitial : Boolean[1..1];
			var feature isDefault : Boolean[1..1];
			
			derived var feature featureWithValue : Feature[1..1] subsets membershipOwningNamespace;
			derived composite var feature value : Expression[1..1] redefines ownedMemberElement;
		}		
		
		metaclass Flow specializes Connector, Step {
			derived var feature payloadType : Classifier[0..*] ordered nonunique;
			derived var feature targetInputFeature : Feature[0..1] ordered nonunique;
			derived var feature sourceOutputFeature : Feature[0..1] ordered nonunique;
			derived var feature flowEnd : FlowEnd[0..2] ordered subsets connectorEnd;
			derived var feature payloadFeature : PayloadFeature[0..1] subsets ownedFeature;
			derived var feature 'interaction' : Interaction[0..*] ordered redefines association, 'behavior';
		}		
		
		metaclass FlowEnd specializes Feature;		
		
		metaclass Function specializes Behavior {
			derived var feature isModelLevelEvaluable : Boolean[1..1];
			
			derived var feature expression : Expression[0..*] subsets 'step';
			derived var feature result : Feature[1..1] subsets output, parameter;
		}		
		
		metaclass IndexExpression specializes OperatorExpression {
			var feature operator : String[1..1] redefines operator;
		}		
		
		abstract metaclass InstantiationExpression specializes Expression {
			derived var feature argument : Expression[0..*] ordered;
			derived var feature instantiatedType : Type[1..1] subsets 'member';
		}		
		
		metaclass Interaction specializes Association, Behavior;		
		
		metaclass Invariant specializes BooleanExpression {
			var feature isNegated : Boolean[1..1];
		}		
		
		metaclass InvocationExpression specializes InstantiationExpression;		
		
		metaclass LibraryPackage specializes Package {
			var feature isStandard : Boolean[1..1];
		}		
		
		metaclass LiteralBoolean specializes LiteralExpression {
			var feature value : Boolean[1..1];
		}		
		
		metaclass LiteralExpression specializes Expression;		
		
		metaclass LiteralInfinity specializes LiteralExpression;		
		
		metaclass LiteralInteger specializes LiteralExpression {
			var feature value : Integer[1..1];
		}		
		
		metaclass LiteralRational specializes LiteralExpression {
			var feature value : Rational[1..1];
		}		
		
		metaclass LiteralString specializes LiteralExpression {
			var feature value : String[1..1];
		}		
		
		metaclass Metaclass specializes Structure;		
		
		metaclass MetadataAccessExpression specializes Expression {
			derived var feature referencedElement : Element[1..1] subsets 'member';
		}		
		
		metaclass MetadataFeature specializes AnnotatingElement, Feature {
			derived var feature 'metaclass' : Metaclass[0..1] subsets 'type';
		}		
		
		metaclass MultiplicityRange specializes Multiplicity {
			derived var feature lowerBound : Expression[0..1] subsets bound;
			derived var feature upperBound : Expression[1..1] subsets bound;
			derived var feature bound : Expression[1..2] ordered subsets ownedMember;
		}		
		
		metaclass NullExpression specializes Expression;		
		
		metaclass OperatorExpression specializes InvocationExpression {
			var feature operator : String[1..1];
		}		
		
		metaclass Package specializes Namespace {
			derived var feature filterCondition : Expression[0..*] ordered subsets ownedMember;
		}		
		
		metaclass ParameterMembership specializes FeatureMembership {
			derived composite var feature ownedMemberParameter : Feature[1..1] redefines ownedMemberFeature;
		}		
		
		metaclass PayloadFeature specializes Feature;		
		
		metaclass Predicate specializes Function;		
		
		metaclass ResultExpressionMembership specializes FeatureMembership {
			derived composite var feature ownedResultExpression : Expression[1..1] redefines ownedMemberFeature;
		}		
		
		metaclass ReturnParameterMembership specializes ParameterMembership;		
		
		metaclass SelectExpression specializes OperatorExpression {
			var feature operator : String[1..1] redefines operator;
		}		
		
		metaclass Step specializes Feature {
			derived var feature 'behavior' : Behavior[0..*] ordered subsets 'type';
			derived var feature parameter : Feature[0..*] ordered redefines directedFeature;
		}		
		
		metaclass Structure specializes Class;		
		
		metaclass Succession specializes Connector;		
		
		metaclass SuccessionFlow specializes Succession, Flow;		
		
	}
}
```

## Declarations

- `KerML` — standard library package
- `KerML::Root` — package
- `KerML::Root::AnnotatingElement` — metaclass
- `KerML::Root::AnnotatingElement::annotatedElement` — feature
- `KerML::Root::AnnotatingElement::ownedAnnotatingRelationship` — feature
- `KerML::Root::AnnotatingElement::owningAnnotatingRelationship` — feature
- `KerML::Root::AnnotatingElement::annotation` — feature
- `KerML::Root::Annotation` — metaclass
- `KerML::Root::Annotation::annotatedElement` — feature
- `KerML::Root::Annotation::annotatingElement` — feature
- `KerML::Root::Annotation::owningAnnotatedElement` — feature
- `KerML::Root::Annotation::owningAnnotatingElement` — feature
- `KerML::Root::Annotation::ownedAnnotatingElement` — feature
- `KerML::Root::Comment` — metaclass
- `KerML::Root::Comment::locale` — feature
- `KerML::Root::Comment::body` — feature
- `KerML::Root::Dependency` — metaclass
- `KerML::Root::Dependency::client` — feature
- `KerML::Root::Dependency::supplier` — feature
- `KerML::Root::Documentation` — metaclass
- `KerML::Root::Documentation::documentedElement` — feature
- `KerML::Root::Element` — metaclass
- `KerML::Root::Element::elementId` — feature
- `KerML::Root::Element::aliasIds` — feature
- `KerML::Root::Element::declaredShortName` — feature
- `KerML::Root::Element::declaredName` — feature
- `KerML::Root::Element::isImpliedIncluded` — feature
- `KerML::Root::Element::shortName` — feature
- `KerML::Root::Element::name` — feature
- `KerML::Root::Element::qualifiedName` — feature
- `KerML::Root::Element::isLibraryElement` — feature
- `KerML::Root::Element::owningRelationship` — feature
- `KerML::Root::Element::ownedRelationship` — feature
- `KerML::Root::Element::owningMembership` — feature
- `KerML::Root::Element::owningNamespace` — feature
- `KerML::Root::Element::owner` — feature
- `KerML::Root::Element::ownedElement` — feature
- `KerML::Root::Element::documentation` — feature
- `KerML::Root::Element::ownedAnnotation` — feature
- `KerML::Root::Element::textualRepresentation` — feature
- `KerML::Root::Import` — metaclass
- `KerML::Root::Import::visibility` — feature
- `KerML::Root::Import::isRecursive` — feature
- `KerML::Root::Import::isImportAll` — feature
- `KerML::Root::Import::importOwningNamespace` — feature
- `KerML::Root::Import::importedElement` — feature
- `KerML::Root::Membership` — metaclass
- `KerML::Root::Membership::memberShortName` — feature
- `KerML::Root::Membership::memberName` — feature
- `KerML::Root::Membership::visibility` — feature
- `KerML::Root::Membership::memberElementId` — feature
- `KerML::Root::Membership::memberElement` — feature
- `KerML::Root::Membership::membershipOwningNamespace` — feature
- `KerML::Root::MembershipImport` — metaclass
- `KerML::Root::MembershipImport::importedMembership` — feature
- `KerML::Root::Namespace` — metaclass
- `KerML::Root::Namespace::membership` — feature
- `KerML::Root::Namespace::ownedImport` — feature
- `KerML::Root::Namespace::member` — feature
- `KerML::Root::Namespace::ownedMember` — feature
- `KerML::Root::Namespace::ownedMembership` — feature
- `KerML::Root::Namespace::importedMembership` — feature
- `KerML::Root::NamespaceImport` — metaclass
- `KerML::Root::NamespaceImport::importedNamespace` — feature
- `KerML::Root::OwningMembership` — metaclass
- `KerML::Root::OwningMembership::ownedMemberElementId` — feature
- `KerML::Root::OwningMembership::ownedMemberShortName` — feature
- `KerML::Root::OwningMembership::ownedMemberName` — feature
- `KerML::Root::OwningMembership::ownedMemberElement` — feature
- `KerML::Root::Relationship` — metaclass
- `KerML::Root::Relationship::isImplied` — feature
- `KerML::Root::Relationship::target` — feature
- `KerML::Root::Relationship::source` — feature
- `KerML::Root::Relationship::owningRelatedElement` — feature
- `KerML::Root::Relationship::ownedRelatedElement` — feature
- `KerML::Root::Relationship::relatedElement` — feature
- `KerML::Root::TextualRepresentation` — metaclass
- `KerML::Root::TextualRepresentation::language` — feature
- `KerML::Root::TextualRepresentation::body` — feature
- `KerML::Root::TextualRepresentation::representedElement` — feature
- `KerML::Root::VisibilityKind` — datatype
- `KerML::Root::VisibilityKind::private` — feature
- `KerML::Root::VisibilityKind::protected` — feature
- `KerML::Root::VisibilityKind::public` — feature
- `KerML::Core` — package
- `KerML::Core::Classifier` — metaclass
- `KerML::Core::Classifier::ownedSubclassification` — feature
- `KerML::Core::Conjugation` — metaclass
- `KerML::Core::Conjugation::originalType` — feature
- `KerML::Core::Conjugation::conjugatedType` — feature
- `KerML::Core::Conjugation::owningType` — feature
- `KerML::Core::CrossSubsetting` — metaclass
- `KerML::Core::CrossSubsetting::crossedFeature` — feature
- `KerML::Core::CrossSubsetting::crossingFeature` — feature
- `KerML::Core::Differencing` — metaclass
- `KerML::Core::Differencing::differencingType` — feature
- `KerML::Core::Differencing::typeDifferenced` — feature
- `KerML::Core::Disjoining` — metaclass
- `KerML::Core::Disjoining::typeDisjoined` — feature
- `KerML::Core::Disjoining::disjoiningType` — feature
- `KerML::Core::Disjoining::owningType` — feature
- `KerML::Core::EndFeatureMembership` — metaclass
- `KerML::Core::EndFeatureMembership::ownedMemberFeature` — feature
- `KerML::Core::Feature` — metaclass
- `KerML::Core::Feature::isUnique` — feature
- `KerML::Core::Feature::isOrdered` — feature
- `KerML::Core::Feature::isComposite` — feature
- `KerML::Core::Feature::isEnd` — feature
- `KerML::Core::Feature::isDerived` — feature
- `KerML::Core::Feature::isPortion` — feature
- `KerML::Core::Feature::isVariable` — feature
- `KerML::Core::Feature::isConstant` — feature
- `KerML::Core::Feature::direction` — feature
- `KerML::Core::Feature::owningType` — feature
- `KerML::Core::Feature::type` — feature
- `KerML::Core::Feature::ownedRedefinition` — feature
- `KerML::Core::Feature::ownedSubsetting` — feature
- `KerML::Core::Feature::owningFeatureMembership` — feature
- `KerML::Core::Feature::endOwningType` — feature
- `KerML::Core::Feature::ownedTyping` — feature
- `KerML::Core::Feature::featuringType` — feature
- `KerML::Core::Feature::ownedTypeFeaturing` — feature
- `KerML::Core::Feature::chainingFeature` — feature
- `KerML::Core::Feature::ownedFeatureInverting` — feature
- `KerML::Core::Feature::ownedFeatureChaining` — feature
- `KerML::Core::Feature::ownedReferenceSubsetting` — feature
- `KerML::Core::Feature::featureTarget` — feature
- `KerML::Core::Feature::crossFeature` — feature
- `KerML::Core::Feature::ownedCrossSubsetting` — feature
- `KerML::Core::FeatureChaining` — metaclass
- `KerML::Core::FeatureChaining::chainingFeature` — feature
- `KerML::Core::FeatureChaining::featureChained` — feature
- `KerML::Core::FeatureDirectionKind` — datatype
- `KerML::Core::FeatureDirectionKind::in` — feature
- `KerML::Core::FeatureDirectionKind::inout` — feature
- `KerML::Core::FeatureDirectionKind::out` — feature
- `KerML::Core::FeatureInverting` — metaclass
- `KerML::Core::FeatureInverting::featureInverted` — feature
- `KerML::Core::FeatureInverting::invertingFeature` — feature
- `KerML::Core::FeatureInverting::owningFeature` — feature
- `KerML::Core::FeatureMembership` — metaclass
- `KerML::Core::FeatureMembership::owningType` — feature
- `KerML::Core::FeatureMembership::ownedMemberFeature` — feature
- `KerML::Core::FeatureTyping` — metaclass
- `KerML::Core::FeatureTyping::typedFeature` — feature
- `KerML::Core::FeatureTyping::type` — feature
- `KerML::Core::FeatureTyping::owningFeature` — feature
- `KerML::Core::Intersecting` — metaclass
- `KerML::Core::Intersecting::intersectingType` — feature
- `KerML::Core::Intersecting::typeIntersected` — feature
- `KerML::Core::Multiplicity` — metaclass
- `KerML::Core::Redefinition` — metaclass
- `KerML::Core::Redefinition::redefiningFeature` — feature
- `KerML::Core::Redefinition::redefinedFeature` — feature
- `KerML::Core::ReferenceSubsetting` — metaclass
- `KerML::Core::ReferenceSubsetting::referencedFeature` — feature
- `KerML::Core::ReferenceSubsetting::referencingFeature` — feature
- `KerML::Core::Specialization` — metaclass
- `KerML::Core::Specialization::general` — feature
- `KerML::Core::Specialization::specific` — feature
- `KerML::Core::Specialization::owningType` — feature
- `KerML::Core::Subclassification` — metaclass
- `KerML::Core::Subclassification::superclassifier` — feature
- `KerML::Core::Subclassification::subclassifier` — feature
- `KerML::Core::Subclassification::owningClassifier` — feature
- `KerML::Core::Subsetting` — metaclass
- `KerML::Core::Subsetting::subsettedFeature` — feature
- `KerML::Core::Subsetting::subsettingFeature` — feature
- `KerML::Core::Subsetting::owningFeature` — feature
- `KerML::Core::Type` — metaclass
- `KerML::Core::Type::isAbstract` — feature
- `KerML::Core::Type::isSufficient` — feature
- `KerML::Core::Type::isConjugated` — feature
- `KerML::Core::Type::ownedSpecialization` — feature
- `KerML::Core::Type::ownedFeatureMembership` — feature
- `KerML::Core::Type::feature` — feature
- `KerML::Core::Type::ownedFeature` — feature
- `KerML::Core::Type::input` — feature
- `KerML::Core::Type::output` — feature
- `KerML::Core::Type::inheritedMembership` — feature
- `KerML::Core::Type::endFeature` — feature
- `KerML::Core::Type::ownedEndFeature` — feature
- `KerML::Core::Type::ownedConjugator` — feature
- `KerML::Core::Type::inheritedFeature` — feature
- `KerML::Core::Type::multiplicity` — feature
- `KerML::Core::Type::unioningType` — feature
- `KerML::Core::Type::ownedIntersecting` — feature
- `KerML::Core::Type::intersectingType` — feature
- `KerML::Core::Type::ownedUnioning` — feature
- `KerML::Core::Type::ownedDisjoining` — feature
- `KerML::Core::Type::featureMembership` — feature
- `KerML::Core::Type::differencingType` — feature
- `KerML::Core::Type::ownedDifferencing` — feature
- `KerML::Core::Type::directedFeature` — feature
- `KerML::Core::TypeFeaturing` — metaclass
- `KerML::Core::TypeFeaturing::featureOfType` — feature
- `KerML::Core::TypeFeaturing::featuringType` — feature
- `KerML::Core::TypeFeaturing::owningFeatureOfType` — feature
- `KerML::Core::Unioning` — metaclass
- `KerML::Core::Unioning::unioningType` — feature
- `KerML::Core::Unioning::typeUnioned` — feature
- `KerML::Kernel` — package
- `KerML::Kernel::Association` — metaclass
- `KerML::Kernel::Association::relatedType` — feature
- `KerML::Kernel::Association::sourceType` — feature
- `KerML::Kernel::Association::targetType` — feature
- `KerML::Kernel::Association::associationEnd` — feature
- `KerML::Kernel::AssociationStructure` — metaclass
- `KerML::Kernel::Behavior` — metaclass
- `KerML::Kernel::Behavior::step` — feature
- `KerML::Kernel::Behavior::parameter` — feature
- `KerML::Kernel::BindingConnector` — metaclass
- `KerML::Kernel::BooleanExpression` — metaclass
- `KerML::Kernel::BooleanExpression::predicate` — feature
- `KerML::Kernel::Class` — metaclass
- `KerML::Kernel::CollectExpression` — metaclass
- `KerML::Kernel::CollectExpression::operator` — feature
- `KerML::Kernel::Connector` — metaclass
- `KerML::Kernel::Connector::relatedFeature` — feature
- `KerML::Kernel::Connector::association` — feature
- `KerML::Kernel::Connector::connectorEnd` — feature
- `KerML::Kernel::Connector::sourceFeature` — feature
- `KerML::Kernel::Connector::targetFeature` — feature
- `KerML::Kernel::Connector::defaultFeaturingType` — feature
- `KerML::Kernel::ConstructorExpression` — metaclass
- `KerML::Kernel::DataType` — metaclass
- `KerML::Kernel::ElementFilterMembership` — metaclass
- `KerML::Kernel::ElementFilterMembership::condition` — feature
- `KerML::Kernel::Expression` — metaclass
- `KerML::Kernel::Expression::isModelLevelEvaluable` — feature
- `KerML::Kernel::Expression::function` — feature
- `KerML::Kernel::Expression::result` — feature
- `KerML::Kernel::FeatureChainExpression` — metaclass
- `KerML::Kernel::FeatureChainExpression::operator` — feature
- `KerML::Kernel::FeatureChainExpression::targetFeature` — feature
- `KerML::Kernel::FeatureReferenceExpression` — metaclass
- `KerML::Kernel::FeatureReferenceExpression::referent` — feature
- `KerML::Kernel::FeatureValue` — metaclass
- `KerML::Kernel::FeatureValue::isInitial` — feature
- `KerML::Kernel::FeatureValue::isDefault` — feature
- `KerML::Kernel::FeatureValue::featureWithValue` — feature
- `KerML::Kernel::FeatureValue::value` — feature
- `KerML::Kernel::Flow` — metaclass
- `KerML::Kernel::Flow::payloadType` — feature
- `KerML::Kernel::Flow::targetInputFeature` — feature
- `KerML::Kernel::Flow::sourceOutputFeature` — feature
- `KerML::Kernel::Flow::flowEnd` — feature
- `KerML::Kernel::Flow::payloadFeature` — feature
- `KerML::Kernel::Flow::interaction` — feature
- `KerML::Kernel::FlowEnd` — metaclass
- `KerML::Kernel::Function` — metaclass
- `KerML::Kernel::Function::isModelLevelEvaluable` — feature
- `KerML::Kernel::Function::expression` — feature
- `KerML::Kernel::Function::result` — feature
- `KerML::Kernel::IndexExpression` — metaclass
- `KerML::Kernel::IndexExpression::operator` — feature
- `KerML::Kernel::InstantiationExpression` — metaclass
- `KerML::Kernel::InstantiationExpression::argument` — feature
- `KerML::Kernel::InstantiationExpression::instantiatedType` — feature
- `KerML::Kernel::Interaction` — metaclass
- `KerML::Kernel::Invariant` — metaclass
- `KerML::Kernel::Invariant::isNegated` — feature
- `KerML::Kernel::InvocationExpression` — metaclass
- `KerML::Kernel::LibraryPackage` — metaclass
- `KerML::Kernel::LibraryPackage::isStandard` — feature
- `KerML::Kernel::LiteralBoolean` — metaclass
- `KerML::Kernel::LiteralBoolean::value` — feature
- `KerML::Kernel::LiteralExpression` — metaclass
- `KerML::Kernel::LiteralInfinity` — metaclass
- `KerML::Kernel::LiteralInteger` — metaclass
- `KerML::Kernel::LiteralInteger::value` — feature
- `KerML::Kernel::LiteralRational` — metaclass
- `KerML::Kernel::LiteralRational::value` — feature
- `KerML::Kernel::LiteralString` — metaclass
- `KerML::Kernel::LiteralString::value` — feature
- `KerML::Kernel::Metaclass` — metaclass
- `KerML::Kernel::MetadataAccessExpression` — metaclass
- `KerML::Kernel::MetadataAccessExpression::referencedElement` — feature
- `KerML::Kernel::MetadataFeature` — metaclass
- `KerML::Kernel::MetadataFeature::metaclass` — feature
- `KerML::Kernel::MultiplicityRange` — metaclass
- `KerML::Kernel::MultiplicityRange::lowerBound` — feature
- `KerML::Kernel::MultiplicityRange::upperBound` — feature
- `KerML::Kernel::MultiplicityRange::bound` — feature
- `KerML::Kernel::NullExpression` — metaclass
- `KerML::Kernel::OperatorExpression` — metaclass
- `KerML::Kernel::OperatorExpression::operator` — feature
- `KerML::Kernel::Package` — metaclass
- `KerML::Kernel::Package::filterCondition` — feature
- `KerML::Kernel::ParameterMembership` — metaclass
- `KerML::Kernel::ParameterMembership::ownedMemberParameter` — feature
- `KerML::Kernel::PayloadFeature` — metaclass
- `KerML::Kernel::Predicate` — metaclass
- `KerML::Kernel::ResultExpressionMembership` — metaclass
- `KerML::Kernel::ResultExpressionMembership::ownedResultExpression` — feature
- `KerML::Kernel::ReturnParameterMembership` — metaclass
- `KerML::Kernel::SelectExpression` — metaclass
- `KerML::Kernel::SelectExpression::operator` — feature
- `KerML::Kernel::Step` — metaclass
- `KerML::Kernel::Step::behavior` — feature
- `KerML::Kernel::Step::parameter` — feature
- `KerML::Kernel::Structure` — metaclass
- `KerML::Kernel::Succession` — metaclass
- `KerML::Kernel::SuccessionFlow` — metaclass
