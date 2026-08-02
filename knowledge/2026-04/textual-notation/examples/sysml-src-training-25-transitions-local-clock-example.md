---
name: Local Clock Example
kind: example
language: SysML
source: sysml/src/training/25. Transitions/Local Clock Example.sysml
elements: [AttributeUsage, ItemDefinition, ItemUsage, PartDefinition, PartUsage, PortUsage, StateUsage]
license: EPL-2.0
---

# Local Clock Example

Verbatim SysML model from `sysml/src/training/25. Transitions/Local Clock Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Local Clock Example' {
	private import ScalarValues::String;
	
	item def Start;
	item def Request;
	
	part def Server {
		part :>> localClock = new Time::Clock();

		attribute today : String;
				
		port requestPort;
		
		state ServerBehavior {
			first start then off;
			
			state off;
			accept Start via requestPort
				then waiting;
			
			state waiting;
			accept request : Request via requestPort
				then responding;
			accept at new Time::Iso8601DateTime(today + "11:59:00")
				then off;
			
			state responding;
			accept after 5 [SI::min]
				then waiting;
		}
	}
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
- [StateUsage](../metamodel/elements/StateUsage.md)
