---
name: ServerSequenceModelOutside
kind: example
language: SysML
source: sysml/src/examples/Interaction Sequencing Examples/ServerSequenceModelOutside.sysml
elements: [EventOccurrenceUsage, OccurrenceUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# ServerSequenceModelOutside

Verbatim SysML model from `sysml/src/examples/Interaction Sequencing Examples/ServerSequenceModelOutside.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ServerSequenceModelOutside {
	public import ServerSequenceModel::*;

	part def PubSubSequenceOutside :> PubSubSequence {
		part :>> producer {
			event publish_source_event = publish_message.start;
		}
		
		part :>> server {
			event occurrence :>> subscribe_target_event = subscribe_message.done;
			then event occurrence :>> publish_target_event = publish_message.done;
			then event occurrence :>> deliver_source_event = deliver_message.start;
		}
		
		part :>> consumer {  /* Redundant with timing constraints on server and generic transfers. */
			event occurrence :>> subscribe_source_event = subscribe_message.start;
			then event occurrence :>> deliver_target_event = deliver_message.done;
		}
	}
}
```

## Elements

- [EventOccurrenceUsage](../metamodel/elements/EventOccurrenceUsage.md)
- [OccurrenceUsage](../metamodel/elements/OccurrenceUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
