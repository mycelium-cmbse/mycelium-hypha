---
name: 17b-Sequence-Modeling
kind: example
language: SysML
source: sysml/src/validation/17-Sequence Modeling/17b-Sequence-Modeling.sysml
elements: [AttributeUsage, ItemDefinition, ItemUsage, OccurrenceDefinition, OccurrenceUsage, PartUsage]
license: EPL-2.0
---

# 17b-Sequence-Modeling

Verbatim SysML model from `sysml/src/validation/17-Sequence Modeling/17b-Sequence-Modeling.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '17b-Sequence-Modeling' {
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
			event publish_message.sourceEvent;
		}
		
		message publish_message of Publish[1];
		
		part server[1] {
			event subscribe_message.targetEvent;
			then event publish_message.targetEvent;
			then event deliver_message.sourceEvent;
		}
		
		message subscribe_message of Subscribe[1];
		message deliver_message of Deliver[1];
		
		part consumer[1] {
			event subscribe_message.sourceEvent;
			then event deliver_message.targetEvent;
		}
	}
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [OccurrenceDefinition](../metamodel/elements/OccurrenceDefinition.md)
- [OccurrenceUsage](../metamodel/elements/OccurrenceUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
