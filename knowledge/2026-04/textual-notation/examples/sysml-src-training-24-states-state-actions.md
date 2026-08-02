---
name: State Actions
kind: example
language: SysML
source: sysml/src/training/24. States/State Actions.sysml
elements: [ActionUsage, AttributeDefinition, AttributeUsage, PartDefinition, PartUsage, StateDefinition, StateUsage]
license: EPL-2.0
---

# State Actions

Verbatim SysML model from `sysml/src/training/24. States/State Actions.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'State Actions' {
	
	attribute def VehicleStartSignal;
	attribute def VehicleOnSignal;
	attribute def VehicleOffSignal;
	
	part def Vehicle;
	
	action performSelfTest { in vehicle : Vehicle; }
	
	state def VehicleStates { in operatingVehicle : Vehicle; }
		
	state vehicleStates : VehicleStates {
		in operatingVehicle : Vehicle;
			
		first start then off;
		
		state off;
		accept VehicleStartSignal 
			then starting;
			
		state starting;
		accept VehicleOnSignal
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
