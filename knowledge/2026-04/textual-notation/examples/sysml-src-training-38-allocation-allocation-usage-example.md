---
name: Allocation Usage Example
kind: example
language: SysML
source: sysml/src/training/38. Allocation/Allocation Usage Example.sysml
elements: [ActionDefinition, ActionUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Allocation Usage Example

Verbatim SysML model from `sysml/src/training/38. Allocation/Allocation Usage Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Allocation Usage Example' {
	package LogicalModel {
		action def ProvidePower;
		action def GenerateTorque;
		
		part def TorqueGenerator;
		
		action providePower : ProvidePower {
			action generateTorque : GenerateTorque;
		}
		
		part torqueGenerator : TorqueGenerator {
			perform providePower.generateTorque;
		}
	}
	
	package PhysicalModel {
		private import LogicalModel::*;
	
		part def PowerTrain;
		part def Engine;
		
		part powerTrain : PowerTrain {
			part engine : Engine {
				perform providePower.generateTorque;
			}
		}
		
		allocate torqueGenerator to powerTrain {
			allocate torqueGenerator.generateTorque to powerTrain.engine.generateTorque;
		}
	}
	
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
