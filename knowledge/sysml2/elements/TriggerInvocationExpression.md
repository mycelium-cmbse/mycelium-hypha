# TriggerInvocationExpression

`Actions` package · concrete metaclass

A TriggerInvocationExpression is an InvocationExpression that invokes one of the trigger Functions from the Kernel Semantic Library Triggers package, as indicated by its kind.

## Owned features

### kind : TriggerKind [1..1]

Indicates which of the Functions from the Triggers model in the Kernel Semantic Library is to be invoked by this TriggerInvocationExpression.


## Constraints

- **validateTriggerInvocationExpressionAfterArgument**
- **validateTriggerInvocationExpressionAtArgument**
- **validateTriggerInvocationExpressionWhenArgument**
