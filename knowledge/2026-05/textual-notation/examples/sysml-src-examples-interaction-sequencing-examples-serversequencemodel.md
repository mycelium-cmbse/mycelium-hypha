---
name: ServerSequenceModel
kind: example
language: SysML
source: sysml/src/examples/Interaction Sequencing Examples/ServerSequenceModel.sysml
elements: [AttributeUsage, EventOccurrenceUsage, ItemDefinition, ItemUsage, OccurrenceUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# ServerSequenceModel

Verbatim SysML model from `sysml/src/examples/Interaction Sequencing Examples/ServerSequenceModel.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ServerSequenceModel {
	private import ScalarValues::String;
	public import SignalDefinitions::*;

	package SignalDefinitions {
	    item def Subscribe {
	    	attribute topic : String;
	    	ref part subscriber;
	    }
	    
		item def Publish {
			attribute topic : String;
			ref publication;
		}
		
		item def Deliver {
			ref publication;
		}
	}

	part def PubSubSequence {
		part producer[1] {
			event occurrence publish_source_event;
		}
		
		message publish_message from producer.publish_source_event to server.publish_target_event;
		
		part server[1] {
			event occurrence subscribe_target_event;
			then event occurrence publish_target_event;
			then event occurrence deliver_source_event;
		}
		
		message subscribe_message from consumer.subscribe_source_event to server.subscribe_target_event;
		message deliver_message from server.deliver_source_event to consumer.deliver_target_event;
		
		part consumer {
			event occurrence subscribe_source_event;
			then event occurrence deliver_target_event;
		}
	}
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [EventOccurrenceUsage](../metamodel/elements/EventOccurrenceUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [OccurrenceUsage](../metamodel/elements/OccurrenceUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
