---
name: SubjectMembership
package: Requirements
fully qualified name: SysML::Systems::Requirements::SubjectMembership
isAbstract: false
visibility: public
generalizes: [ParameterMembership]
specializedBy: []
---

# SubjectMembership

`Requirements` package · concrete metaclass

A SubjectMembership is a ParameterMembership that indicates that its ownedSubjectParameter is the subject of its owningType. The owningType of a SubjectMembership must be a RequirementDefinition, RequirementUsage, CaseDefinition, or CaseUsage.

## Generalizations

- [ParameterMembership](ParameterMembership.md)

## Owned features

### ownedSubjectParameter

`+` [Usage](Usage.md) · `[1..1]` · *derived, composite*

The UsageownedMemberParameter of this SubjectMembership.

Redefines [ownedMemberParameter](ParameterMembership.md#ownedmemberparameter)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | [String](String.md) | [0..*] | [Element](Element.md) | ordered |
| declaredName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| declaredShortName | [String](String.md) | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | [String](String.md) | [1..1] | [Element](Element.md) |  |
| isImplied | [Boolean](Boolean.md) | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) |  |
| isLibraryElement | [Boolean](Boolean.md) | [1..1] | [Element](Element.md) | derived |
| memberElement | [Element](Element.md) | [1..1] | [Membership](Membership.md) |  |
| memberElementId | [String](String.md) | [1..1] | [Membership](Membership.md) | derived |
| memberName | [String](String.md) | [0..1] | [Membership](Membership.md) |  |
| memberShortName | [String](String.md) | [0..1] | [Membership](Membership.md) |  |
| membershipOwningNamespace | [Namespace](Namespace.md) | [1..1] | [Membership](Membership.md) | derived |
| name | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedMemberElement | [Element](Element.md) | [1..1] | [OwningMembership](OwningMembership.md) | derived, composite |
| ownedMemberElementId | [String](String.md) | [1..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberFeature | [Feature](Feature.md) | [1..1] | [FeatureMembership](FeatureMembership.md) | derived, composite |
| ownedMemberName | [String](String.md) | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberParameter | [Feature](Feature.md) | [1..1] | [ParameterMembership](ParameterMembership.md) | derived, composite |
| ownedMemberShortName | [String](String.md) | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedRelatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | [Element](Element.md) | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| owningType | [Type](Type.md) | [1..1] | [FeatureMembership](FeatureMembership.md) | derived |
| qualifiedName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| relatedElement | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | [String](String.md) | [0..1] | [Element](Element.md) | derived |
| source | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| target | [Element](Element.md) | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| visibility | [VisibilityKind](VisibilityKind.md) | [1..1] | [Membership](Membership.md) |  |

## Constraints

### validateSubjectMembershipOwningType

The owningType of a SubjectMembership must be a RequirementDefinition, RequirementUsage, CaseDefinition, or CaseUsage.

```ocl
owningType.oclIsType(RequirementDefinition) or
owningType.oclIsType(RequiremenCaseRequirementDefinition) or
owningType.oclIsType(CaseDefinition) or
owningType.oclIsType(CaseUsage)
```

