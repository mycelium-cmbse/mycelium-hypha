---
name: FramedConcernMembership
package: Requirements
isAbstract: false
generalizes: [RequirementConstraintMembership]
specializedBy: []
---

# FramedConcernMembership

`Requirements` package · concrete metaclass

A FramedConcernMembership is a RequirementConstraintMembership for a framed ConcernUsage of a RequirementDefinition or RequirementUsage.

## Generalizations

- [RequirementConstraintMembership](RequirementConstraintMembership.md)

## Owned features

### kind : RequirementConstraintKind [1..1]

The kind of an FramedConcernMembership must be requirement.

Redefines: `kind`

### ownedConcern : ConcernUsage [1..1] {derived, composite}

The ConcernUsage that is the ownedConstraint of this FramedConcernMembership.

Redefines: `ownedConstraint`

### referencedConcern : ConcernUsage [1..1] {derived}

The ConcernUsage that is referenced through this FramedConcernMembership. It is the referencedConstraint of the FramedConcernMembership considered as a RequirementConstraintMembership, which must be a ConcernUsage.

Redefines: `referencedConstraint`


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| ownedConstraint | ConstraintUsage | [1..1] | [RequirementConstraintMembership](RequirementConstraintMembership.md) | derived, composite |
| referencedConstraint | ConstraintUsage | [1..1] | [RequirementConstraintMembership](RequirementConstraintMembership.md) | derived |

## Constraints

### validateFramedConcernMembershipConstraintKind

A FramedConcernMembership must have kind = requirement.

```ocl
kind = RequirementConstraintKind::requirement
```

