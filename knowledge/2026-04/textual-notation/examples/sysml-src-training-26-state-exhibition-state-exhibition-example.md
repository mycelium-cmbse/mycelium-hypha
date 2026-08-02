---
name: State Exhibition Example
kind: example
language: SysML
source: sysml/src/training/26. State Exhibition/State Exhibition Example.sysml
elements: [PartUsage]
license: EPL-2.0
---

# State Exhibition Example

Verbatim SysML model from `sysml/src/training/26. State Exhibition/State Exhibition Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'State Exhibition Example' {
	private import 'Transition Actions'::*;
	
	part vehicle : Vehicle {
		
		part vehicleController : VehicleController;
		
		exhibit vehicleStates {
			in operatingVehicle = vehicle;
			in controller = vehicleController;
		}

	}
	
}
```

## Elements

- [PartUsage](../metamodel/elements/PartUsage.md)
