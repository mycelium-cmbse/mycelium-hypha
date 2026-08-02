---
name: 12b-Allocation
kind: example
language: SysML
source: sysml/src/validation/12-Dependency Relationships/12b-Allocation.sysml
elements: [ActionUsage, PartUsage]
license: EPL-2.0
---

# 12b-Allocation

Verbatim SysML model from `sysml/src/validation/12-Dependency Relationships/12b-Allocation.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '12b-Allocation' {
	private import LogicalModel::*;
	private import PhysicalModel::*;
	
	package LogicalModel {
		action providePower {
			action generateTorque;
		}
		
		part torqueGenerator {
			perform providePower.generateTorque;
		}
	}
	
	package PhysicalModel {
		part powerTrain {
			part engine {
				perform providePower.generateTorque;
			}
		}
	}
	
	allocate torqueGenerator to powerTrain {
		allocate torqueGenerator.generateTorque to powerTrain.engine.generateTorque;
	}
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
