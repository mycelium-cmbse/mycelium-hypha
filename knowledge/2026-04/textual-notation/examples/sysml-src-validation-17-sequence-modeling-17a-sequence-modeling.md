---
name: 17a-Sequence-Modeling
kind: example
language: SysML
source: sysml/src/validation/17-Sequence Modeling/17a-Sequence-Modeling.sysml
elements: [AttributeUsage, EventOccurrenceUsage, ItemDefinition, ItemUsage, OccurrenceDefinition, OccurrenceUsage, PartUsage]
license: EPL-2.0
---

# 17a-Sequence-Modeling

Verbatim SysML model from `sysml/src/validation/17-Sequence Modeling/17a-Sequence-Modeling.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '17a-Sequence-Modeling' {
	private import ScalarValues::*;
	private import PayloadDefinitions::*;

	package PayloadDefinitions {
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

	occurrence def PubSubSequence {
		part producer[1] {
			event occurrence publish_source_event;
		}
		
		message publish_message of Publish[1] from producer.publish_source_event to server.publish_target_event;
		
		part server[1] {
			event occurrence subscribe_target_event;
			then event occurrence publish_target_event;
			then event occurrence deliver_source_event;
		}
		
		message subscribe_message of Subscribe[1] from consumer.subscribe_source_event to server.subscribe_target_event;
		message deliver_message of Deliver[1] from server.deliver_source_event to consumer.deliver_target_event;
		
		part consumer[1] {
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
- [OccurrenceDefinition](../metamodel/elements/OccurrenceDefinition.md)
- [OccurrenceUsage](../metamodel/elements/OccurrenceUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
