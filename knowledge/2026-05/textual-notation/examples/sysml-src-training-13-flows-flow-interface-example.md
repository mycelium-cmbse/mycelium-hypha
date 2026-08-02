---
name: Flow Interface Example
kind: example
language: SysML
source: sysml/src/training/13. Flows/Flow Interface Example.sysml
elements: [FlowUsage, InterfaceDefinition, InterfaceUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Flow Interface Example

Verbatim SysML model from `sysml/src/training/13. Flows/Flow Interface Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Flow Interface Example' {
	private import 'Port Example'::*;
	
	part def Vehicle;
	
	interface def FuelInterface {
		end supplierPort : FuelOutPort;
		end consumerPort : FuelInPort;
		
		flow supplierPort.fuelSupply to consumerPort.fuelSupply;			
		flow consumerPort.fuelReturn to supplierPort.fuelReturn;
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

- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [InterfaceDefinition](../metamodel/elements/InterfaceDefinition.md)
- [InterfaceUsage](../metamodel/elements/InterfaceUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
