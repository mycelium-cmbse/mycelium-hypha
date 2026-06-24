# SendActionUsage

`Actions` package · concrete metaclass

A SendActionUsage is an ActionUsage that specifies the sending of a payload given by the result of its payloadArgument Expression via a MessageTransfer whose source is given by the result of the senderArgument Expression and whose target is given by the result of the receiverArgument Expression. If no senderArgument is provided, the default is the this context for the action. If no receiverArgument is given, then the receiver is to be determined by, e.g., outgoing Connections from the sender.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Owned features

### payloadArgument : &#171;untyped&#187; [0..1]

An Expression whose result is bound to the payload input parameter of this SendActionUsage.

### receiverArgument : &#171;untyped&#187; [0..1]

An Expression whose result is bound to the receiver input parameter of this SendActionUsage.

### senderArgument : &#171;untyped&#187; [0..1]

An Expression whose result is bound to the sender input parameter of this SendActionUsage.


## Constraints

- **checkSendActionUsageSpecialization**
- **checkSendActionUsageSubactionSpecialization**
- **deriveSendActionUsagePayloadArgument**
- **deriveSendActionUsageReceiverArgument**
- **deriveSendActionUsageSenderArgument**
- **validateSendActionUsagePayloadArgument**
