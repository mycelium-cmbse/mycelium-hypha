---
name: Individuals and Snapshots Example
kind: example
language: SysML
source: sysml/src/training/28. Individuals/Individuals and Snapshots Example.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Individuals and Snapshots Example

Verbatim SysML model from `sysml/src/training/28. Individuals/Individuals and Snapshots Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Individuals and Snapshots Example' {
	public import 'Part Definition Example'::*;
	
	individual part def Vehicle_1 :> Vehicle {
		
		snapshot part vehicle_1_t0 {
			:>> mass = 2000.0;
			:>> status {
				:>> gearSetting = 0;
				:>> acceleratorPosition = 0.0;
			}
		}
		
		snapshot part vehicle_1_t1 {
			:>> mass = 1500.0;
			:>> status {
				:>> gearSetting = 2;
				:>> acceleratorPosition = 0.5;
			}
		}
		
		first vehicle_1_t0 then vehicle_1_t1;
	}
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
