---
name: Event Occurrence Example
kind: example
language: SysML
source: sysml/src/training/27. Occurrences/Event Occurrence Example.sysml
elements: [EventOccurrenceUsage, OccurrenceUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Event Occurrence Example

Verbatim SysML model from `sysml/src/training/27. Occurrences/Event Occurrence Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Event Occurrence Example' {	
	part def Driver;
	part def CruiseController;
	part def Speedometer;
	part def Engine;
	part def Vehicle;
	
	part driver : Driver {
		event occurrence setSpeedSent;
	}
	
	part vehicle : Vehicle {
	
		part cruiseController : CruiseController {
			event occurrence setSpeedReceived;		
			then event occurrence sensedSpeedReceived;		
			then event occurrence fuelCommandSent;
		}
		
		part speedometer : Speedometer {
			event occurrence sensedSpeedSent;
		}
		
		part engine : Engine {
			event occurrence fuelCommandReceived;
		}
	
	}
}
```

## Elements

- [EventOccurrenceUsage](../metamodel/elements/EventOccurrenceUsage.md)
- [OccurrenceUsage](../metamodel/elements/OccurrenceUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
