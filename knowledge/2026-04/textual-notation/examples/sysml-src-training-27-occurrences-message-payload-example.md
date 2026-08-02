---
name: Message Payload Example
kind: example
language: SysML
source: sysml/src/training/27. Occurrences/Message Payload Example.sysml
elements: [AttributeUsage, EventOccurrenceUsage, ItemDefinition, ItemUsage, OccurrenceDefinition, OccurrenceUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Message Payload Example

Verbatim SysML model from `sysml/src/training/27. Occurrences/Message Payload Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Message Payload Example' {
	private import 'Event Occurrence Example'::*;
	
	item def SetSpeed;
	item def SensedSpeed;
	item def FuelCommand {
		attribute fuelFlow : ScalarValues::Real;
	}
	
	part def EngineController;
	
	part vehicle1 :> vehicle {
		part engineController : EngineController {
			event occurrence fuelCommandReceived;
			then event occurrence fuelCommandForwarded;
		}
	}
	
	occurrence def CruiseControlInteraction {		
		ref part :>> driver;		
		ref part vehicle :>> vehicle1;
		
		message setSpeedMessage of SetSpeed 
			from driver.setSpeedSent to vehicle.cruiseController.setSpeedReceived;
			
		then message sensedSpeedMessage of SensedSpeed 
			from vehicle.speedometer.sensedSpeedSent to vehicle.cruiseController.sensedSpeedReceived;
			
		then message fuelCommandMessage of fuelCommand : FuelCommand 
			from vehicle.cruiseController.fuelCommandSent to vehicle.engineController.fuelCommandReceived;
		
		then message fuelCommandForwardingMessage of fuelCommand : FuelCommand = fuelCommandMessage.fuelCommand
			from vehicle.engineController.fuelCommandForwarded to vehicle.engine.fuelCommandReceived;
		
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
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
