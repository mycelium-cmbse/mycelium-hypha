# ControlNode

`Actions` package · abstract metaclass

A ControlNode is an ActionUsage that does not have any inherent behavior but provides constraints on incoming and outgoing Successions that are used to control other Actions. A ControlNode must be a composite owned usage of an ActionDefinition or ActionUsage.

## Generalizations

- [ActionUsage](ActionUsage.md)

## Constraints

- **checkControlNodeSpecialization**
- **validateControlNodeIncomingSuccessions**
- **validateControlNodeIsComposite**
- **validateControlNodeOutgoingSuccessions**
- **validateControlNodeOwningType**
