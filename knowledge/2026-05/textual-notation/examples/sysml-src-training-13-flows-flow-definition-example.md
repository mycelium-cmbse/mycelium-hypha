---
name: Flow Definition Example
kind: example
language: SysML
source: sysml/src/training/13. Flows/Flow Definition Example.sysml
elements: [FlowDefinition, FlowUsage, PartDefinition, PartUsage, PortUsage]
license: EPL-2.0
---

# Flow Definition Example

Verbatim SysML model from `sysml/src/training/13. Flows/Flow Definition Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Flow Definition Example' {
	private import 'Port Example'::*;
	
	part def Vehicle;
	
	flow def FuelFlow {
		ref :>> payload : Fuel;
		end port supplierPort : FuelOutPort;
		end port consumerPort : FuelInPort;
	}
	
	part vehicle : Vehicle {
		part tankAssy : FuelTankAssembly;
		part eng : Engine;
		
		flow : FuelFlow of Fuel
		  from tankAssy.fuelTankPort.fuelSupply
			to eng.engineFuelPort.fuelSupply;
			
	} 
}
```

## Elements

- [FlowDefinition](../metamodel/elements/FlowDefinition.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
