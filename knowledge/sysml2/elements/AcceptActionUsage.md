# AcceptActionUsage

`Actions` package · concrete metaclass

An AcceptActionUsage is an ActionUsage that specifies the acceptance of an incomingTransfer from the Occurrence given by the result of its receiverArgument Expression. (If no receiverArgument is provided, the default is the this context of the AcceptActionUsage.) The payload of the accepted Transfer is output on its payloadParameter. Which Transfers may be accepted is determined by conformance to the typing and (potentially) binding of the payloadParameter.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### payloadArgument : &#171;untyped&#187; [0..1]

An Expression whose result is bound to the payload parameter of this AcceptActionUsage. If provided, the AcceptActionUsage will only accept a Transfer with exactly this payload.

### payloadParameter : ReferenceUsage [1..1]

The nestedReference of this AcceptActionUsage that redefines the payload output parameter of the base AcceptActionUsage AcceptAction from the Systems Model Library.

### receiverArgument : &#171;untyped&#187; [0..1]

An Expression whose result is bound to the receiver input parameter of this AcceptActionUsage.


## Constraints

- **checkAcceptActionUsageReceiverBindingConnector**
- **checkAcceptActionUsageSpecialization**
- **checkAcceptActionUsageSubactionSpecialization**
- **checkAcceptActionUsageTriggerActionSpecialization**
- **deriveAcceptActionUsagePayloadArgument**
- **deriveAcceptActionUsagePayloadParameter**
- **deriveAcceptActionUsageReceiverArgument**
- **validateAcceptActionUsageParameters**
