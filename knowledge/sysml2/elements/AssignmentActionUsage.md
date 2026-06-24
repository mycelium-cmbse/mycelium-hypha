# AssignmentActionUsage

`Actions` package · concrete metaclass

An AssignmentActionUsage is an ActionUsage that is defined, directly or indirectly, by the ActionDefinition AssignmentAction from the Systems Model Library. It specifies that the value of the referent Feature, relative to the target given by the result of the targetArgument Expression, should be set to the result of the valueExpression.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### referent : &#171;untyped&#187; [1..1]

The Feature whose value is to be set.

### targetArgument : &#171;untyped&#187; [0..1]

The Expression whose value is an occurrence in the domain of the referent Feature, for which the value of the referent will be set to the result of the valueExpression by this AssignmentActionUsage.

### valueExpression : &#171;untyped&#187; [0..1]

The Expression whose result is to be assigned to the referent Feature.


## Constraints

- **checkAssignmentActionUsageAccessedFeatureRedefinition**
- **checkAssignmentActionUsageReferentRedefinition**
- **checkAssignmentActionUsageSpecialization**
- **checkAssignmentActionUsageStartingAtRedefinition**
- **checkAssignmentActionUsageSubactionSpecialization**
- **deriveAssignmentActionUsageReferent**
- **deriveAssignmentActionUsageValueExpression**
- **deriveAssignmentUsageTargetArgument**
- **validateAssignmentActionUsageReferent**
- **validateAssignmentActionUsageReferentIsTimeVarying**
