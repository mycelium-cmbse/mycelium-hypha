---
name: Individuals and Roles-1
kind: example
language: SysML
source: sysml/src/training/28. Individuals/Individuals and Roles-1.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Individuals and Roles-1

Verbatim SysML model from `sysml/src/training/28. Individuals/Individuals and Roles-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Individuals and Roles' {
	private import 'Part Definition Example'::*;
	
	part def Wheel;
	
	individual part def Vehicle_1 :> Vehicle {
		part leftFrontWheel : Wheel;
		part rightFrontWheel : Wheel;
	}
	
	individual part def Wheel_1 :> Wheel;
	
	individual part vehicle_1 : Vehicle_1 {
		snapshot part vehicle_1_t0 {
			snapshot leftFrontWheel_t0 : Wheel_1 :>> leftFrontWheel;
		}
		
		then snapshot part vehicle_1_t1 {
			snapshot rightFrontWheel_t1 : Wheel_1 :>> rightFrontWheel;
		}
	}
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
