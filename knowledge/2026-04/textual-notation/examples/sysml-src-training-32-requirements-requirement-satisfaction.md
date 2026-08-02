---
name: Requirement Satisfaction
kind: example
language: SysML
source: sysml/src/training/32. Requirements/Requirement Satisfaction.sysml
elements: [ActionUsage, PartUsage, PortUsage]
license: EPL-2.0
---

# Requirement Satisfaction

Verbatim SysML model from `sysml/src/training/32. Requirements/Requirement Satisfaction.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Requirement Satisfaction' {
	private import 'Requirement Definitions'::*;
	private import 'Requirement Groups'::*;
	
	action 'provide power' {
		action 'generate torque' { }
	}
	
	part vehicle_c1 : Vehicle {
		perform 'provide power';
			
		part engine_v1: Engine {
			port :>> clutchPort;
			perform 'provide power'.'generate torque' :>> generateTorque;
		}	
	}
	
	part 'Vehicle c1 Design Context' {
		
		ref vehicle_design :> vehicle_c1;
	
		satisfy vehicleSpecification by vehicle_design;
		satisfy engineSpecification by vehicle_design.engine_v1;
	
	}
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
