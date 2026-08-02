---
name: Allocation Definition Example
kind: example
language: SysML
source: sysml/src/training/38. Allocation/Allocation Definition Example.sysml
elements: [ActionDefinition, ActionUsage, AllocationDefinition, AllocationUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Allocation Definition Example

Verbatim SysML model from `sysml/src/training/38. Allocation/Allocation Definition Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Allocation Definition Example' {
	package LogicalModel {
		action def ProvidePower;
		action def GenerateTorque;
		
		part def LogicalElement;
		part def TorqueGenerator :> LogicalElement;
		
		action providePower : ProvidePower {
			action generateTorque : GenerateTorque;
		}
		
		part torqueGenerator : TorqueGenerator {
			perform providePower.generateTorque;
		}
		
	}
	
	package PhysicalModel {
		private import LogicalModel::*;
		
		part def PhysicalElement;
		part def PowerTrain :> PhysicalElement;
		
		part powerTrain : PowerTrain {
			part engine {
				perform providePower.generateTorque;
			}
		}
	
		allocation def LogicalToPhysical {
			end logical : LogicalElement;
			end physical : PhysicalElement;
		}
		
		allocation torqueGenAlloc : LogicalToPhysical allocate torqueGenerator to powerTrain;
	}	
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AllocationDefinition](../metamodel/elements/AllocationDefinition.md)
- [AllocationUsage](../metamodel/elements/AllocationUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
