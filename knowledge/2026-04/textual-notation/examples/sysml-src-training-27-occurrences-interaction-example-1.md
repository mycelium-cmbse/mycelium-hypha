---
name: Interaction Example-1
kind: example
language: SysML
source: sysml/src/training/27. Occurrences/Interaction Example-1.sysml
elements: [ItemDefinition, ItemUsage, OccurrenceDefinition, OccurrenceUsage, PartUsage]
license: EPL-2.0
---

# Interaction Example-1

Verbatim SysML model from `sysml/src/training/27. Occurrences/Interaction Example-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Interaction Example-1' {
	public import 'Event Occurrence Example'::*;
	
	item def SetSpeed;
	item def SensedSpeed;
	item def FuelCommand;
	
	occurrence def CruiseControlInteraction {		
		ref part :>> driver;		
		ref part :>> vehicle;
		
		message setSpeedMessage of SetSpeed 
			from driver.setSpeedSent to vehicle.cruiseController.setSpeedReceived;
			
		message sensedSpeedMessage of SensedSpeed 
			from vehicle.speedometer.sensedSpeedSent to vehicle.cruiseController.sensedSpeedReceived;
			
		message fuelCommandMessage of FuelCommand 
			from vehicle.cruiseController.fuelCommandSent to vehicle.engine.fuelCommandReceived;
		
		first setSpeedMessage then sensedSpeedMessage;
	}
}
```

## Elements

- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [OccurrenceDefinition](../metamodel/elements/OccurrenceDefinition.md)
- [OccurrenceUsage](../metamodel/elements/OccurrenceUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
