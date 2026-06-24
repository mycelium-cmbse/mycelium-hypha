# RequirementDefinition

`Requirements` package · concrete metaclass

A RequirementDefinition is a ConstraintDefinition that defines a requirement used in the context of a specification as a constraint that a valid solution must satisfy. The specification is relative to a specified subject, possibly in collaboration with one or more external actors.

## Generalizations

- [ConstraintDefinition](ConstraintDefinition.md)

## Owned features

### actorParameter : PartUsage [0..*]

The parameters of this RequirementDefinition that represent actors involved in the requirement.

### assumedConstraint : ConstraintUsage [0..*]

The owned ConstraintUsages that represent assumptions of this RequirementDefinition, which are the ownedConstraints of the RequirementConstraintMemberships of the RequirementDefinition with kind = assumption.

### framedConcern : ConcernUsage [0..*]

The ConcernUsages framed by this RequirementDefinition, which are the ownedConcerns of all FramedConcernMemberships of the RequirementDefinition.

Subsets: `requiredConstraint`

### reqId : String [0..1]

An optional modeler-specified identifier for this RequirementDefinition (used, e.g., to link it to an original requirement text in some source document), which is the declaredShortName for the RequirementDefinition.

### requiredConstraint : ConstraintUsage [0..*]

The owned ConstraintUsages that represent requirements of this RequirementDefinition, derived as the ownedConstraints of the RequirementConstraintMemberships of the RequirementDefinition with kind = requirement.

### stakeholderParameter : PartUsage [0..*]

The parameters of this RequirementDefinition that represent stakeholders for the requirement.

### subjectParameter : Usage [1..1]

The parameter of this RequirementDefinition that represents its subject.

### text : String [0..*]

An optional textual statement of the requirement represented by this RequirementDefinition, derived from the bodies of the documentation of the RequirementDefinition.


## Constraints

- **checkRequirementDefinitionSpecialization**
- **deriveRequirementDefinitionActorParameter**
- **deriveRequirementDefinitionAssumedConstraint**
- **deriveRequirementDefinitionFramedConcern**
- **deriveRequirementDefinitionRequiredConstraint**
- **deriveRequirementDefinitionStakeholderParameter**
- **deriveRequirementDefinitionSubjectParameter**
- **deriveRequirementDefinitionText**
- **validateRequirementDefinitionOnlyOneSubject**
- **validateRequirementDefinitionSubjectParameterPosition**
