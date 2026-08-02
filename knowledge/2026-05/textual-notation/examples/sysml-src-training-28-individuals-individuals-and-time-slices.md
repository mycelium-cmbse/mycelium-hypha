---
name: Individuals and Time Slices
kind: example
language: SysML
source: sysml/src/training/28. Individuals/Individuals and Time Slices.sysml
elements: [ItemDefinition, ItemUsage]
license: EPL-2.0
---

# Individuals and Time Slices

Verbatim SysML model from `sysml/src/training/28. Individuals/Individuals and Time Slices.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Individuals and Time Slices' {
	private import 'Individuals and Snapshots Example'::*;
	
	individual item def Alice :> Person;
	individual item def Bob :> Person;
	
	individual : Vehicle_1 {
		
		timeslice aliceDriving {
			ref individual item :>> driver : Alice;

			snapshot :>> start {
				:>> mass = 2000.0;
			}
			
			snapshot :>> done {
				:>> mass = 1500.0;
			}			
		}
		
		then timeslice bobDriving {
			ref individual item :>> driver : Bob;
		}
		
	}
}
```

## Elements

- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
