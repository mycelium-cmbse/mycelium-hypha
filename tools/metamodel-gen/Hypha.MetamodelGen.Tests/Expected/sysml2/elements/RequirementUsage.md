# RequirementUsage

`Requirements` package · concrete metaclass

A RequirementUsage is a Usage of a RequirementDefinition.

## Generalizations

- [ConstraintUsage](ConstraintUsage.md)

## Owned features

### actorParameter : PartUsage [0..*]

The parameters of this RequirementUsage that represent actors involved in the requirement.

### assumedConstraint : ConstraintUsage [0..*]

The owned ConstraintUsages that represent assumptions of this RequirementUsage, derived as the ownedConstraints of the RequirementConstraintMemberships of the RequirementUsage with kind = assumption.

### framedConcern : ConcernUsage [0..*]

The ConcernUsages framed by this RequirementUsage, which are the ownedConcerns of all FramedConcernMemberships of the RequirementUsage.

Subsets: `requiredConstraint`

### reqId : String [0..1]

An optional modeler-specified identifier for this RequirementUsage (used, e.g., to link it to an original requirement text in some source document), which is the declaredShortName for the RequirementUsage.

### requiredConstraint : ConstraintUsage [0..*]

The owned ConstraintUsages that represent requirements of this RequirementUsage, which are the ownedConstraints of the RequirementConstraintMemberships of the RequirementUsage with kind = requirement.

### requirementDefinition : RequirementDefinition [0..1]

The RequirementDefinition that is the single definition of this RequirementUsage.

Redefines: `constraintDefinition`

### stakeholderParameter : PartUsage [0..*]

The parameters of this RequirementUsage that represent stakeholders for the requirement.

### subjectParameter : Usage [1..1]

The parameter of this RequirementUsage that represents its subject.

### text : String [0..*]

An optional textual statement of the requirement represented by this RequirementUsage, derived from the bodies of the documentation of the RequirementUsage.


## Constraints

- **checkRequirementUsageObjectiveRedefinition**
- **checkRequirementUsageRequirementVerificationSpecialization**
- **checkRequirementUsageSpecialization**
- **checkRequirementUsageSubrequirementSpecialization**
- **deriveRequirementUsageActorParameter**
- **deriveRequirementUsageAssumedConstraint**
- **deriveRequirementUsageFramedConcern**
- **deriveRequirementUsageRequiredConstraint**
- **deriveRequirementUsageStakeholderParameter**
- **deriveRequirementUsageSubjectParameter**
- **deriveRequirementUsageText**
- **validateRequirementUsageOnlyOneSubject**
- **validateRequirementUsageSubjectParameterPosition**
