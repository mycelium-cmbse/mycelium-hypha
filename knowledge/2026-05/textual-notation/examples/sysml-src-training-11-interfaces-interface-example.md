---
name: Interface Example
kind: example
language: SysML
source: sysml/src/training/11. Interfaces/Interface Example.sysml
elements: [InterfaceDefinition, InterfaceUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Interface Example

Verbatim SysML model from `sysml/src/training/11. Interfaces/Interface Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Interface Example' {
	private import 'Port Example'::*;
	
	part def Vehicle;
	
	interface def FuelInterface {
		end supplierPort : FuelOutPort;
		end consumerPort : FuelInPort;
	}
	
	part vehicle : Vehicle {	
		part tankAssy : FuelTankAssembly;		
		part eng : Engine;
		
		interface : FuelInterface connect 
			supplierPort ::> tankAssy.fuelTankPort to 
			consumerPort ::> eng.engineFuelPort;
	} 
}
```

## Elements

- [InterfaceDefinition](../metamodel/elements/InterfaceDefinition.md)
- [InterfaceUsage](../metamodel/elements/InterfaceUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
