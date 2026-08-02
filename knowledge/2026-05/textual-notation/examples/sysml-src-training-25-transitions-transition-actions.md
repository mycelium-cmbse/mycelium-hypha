---
name: Transition Actions
kind: example
language: SysML
source: sysml/src/training/25. Transitions/Transition Actions.sysml
elements: [ActionUsage, AttributeDefinition, AttributeUsage, PartDefinition, PartUsage, StateDefinition, StateUsage]
license: EPL-2.0
---

# Transition Actions

Verbatim SysML model from `sysml/src/training/25. Transitions/Transition Actions.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Transition Actions' {
	
	attribute def VehicleStartSignal;
	attribute def VehicleOnSignal;
	attribute def VehicleOffSignal;
	
	attribute def ControllerStartSignal;
	
	part def Vehicle {
		brakePedalDepressed : ScalarValues::Boolean;
	}
	part def VehicleController;
	
	action performSelfTest { in vehicle : Vehicle; }
	
	state def VehicleStates;
		
	state vehicleStates : VehicleStates {
		in operatingVehicle : Vehicle;
		in controller : VehicleController;

		first start then off;
		
		state off;
		accept VehicleStartSignal 
			then starting;
			
		state starting;
		accept VehicleOnSignal
			if operatingVehicle.brakePedalDepressed
			do send new ControllerStartSignal() to controller
			then on;
			
		state on {
			entry performSelfTest{ in vehicle = operatingVehicle; }
			do action providePower { /* ... */ }
			exit action applyParkingBrake { /* ... */ }
		}
		accept VehicleOffSignal
			then off;

	}
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [StateDefinition](../metamodel/elements/StateDefinition.md)
- [StateUsage](../metamodel/elements/StateUsage.md)
