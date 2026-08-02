---
name: 12b-Allocation-1
kind: example
language: SysML
source: sysml/src/validation/12-Dependency Relationships/12b-Allocation-1.sysml
elements: [ActionDefinition, ActionUsage, AllocationDefinition, AllocationUsage, ConstraintUsage, PartDefinition, PartUsage, PerformActionUsage, RequirementUsage]
license: EPL-2.0
---

# 12b-Allocation-1

Verbatim SysML model from `sysml/src/validation/12-Dependency Relationships/12b-Allocation-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '12b-Allocation-1' {
	private import SI::*;
	private import RequirementModel::*;
	private import LogicalModel::*;
	private import PhysicalModel::*;
	
	package RequirementModel {
		requirement torqueGeneration {
			subject generator: TorqueGenerator;
			require constraint { 
				 generator.generateTorque.torque > 0.0 [N*m]
			}
		}
	}
	
	package LogicalModel {
		action def GenerateTorque { out torque :> ISQ::torque; }
		
		part def LogicalElement;
		part def TorqueGenerator :> LogicalElement {
			perform action generateTorque : GenerateTorque;
		}	
		
		action providePower {
			action generateTorque : GenerateTorque;
		}
		
		part torqueGenerator : TorqueGenerator {
			perform providePower.generateTorque :>> generateTorque;
		}
		
		satisfy torqueGeneration by torqueGenerator;			
	}
	
	package PhysicalModel {
		part def PhysicalElement;
		part def PowerTrain :> PhysicalElement;
		
		part powerTrain : PowerTrain {
			part engine {
				perform providePower.generateTorque;
			}
		}
	}
	
	allocation def LogicalToPhysical {
		end logical : LogicalElement;
		end physical : PhysicalElement;
	}
	
	allocation torqueGenAlloc : LogicalToPhysical 
		allocate logical ::> torqueGenerator to physical ::> powerTrain {
			
		allocate torqueGenerator.generateTorque to powerTrain.engine.generateTorque;		
	}
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AllocationDefinition](../metamodel/elements/AllocationDefinition.md)
- [AllocationUsage](../metamodel/elements/AllocationUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PerformActionUsage](../metamodel/elements/PerformActionUsage.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
