---
name: Flow Usage Example
kind: example
language: SysML
source: sysml/src/training/13. Flows/Flow Usage Example.sysml
elements: [FlowUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Flow Usage Example

Verbatim SysML model from `sysml/src/training/13. Flows/Flow Usage Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Flow Usage Example' {
	private import 'Port Example'::*;
	
	part def Vehicle;
	
	part vehicle : Vehicle {
		part tankAssy : FuelTankAssembly;
		part eng : Engine;
		
		flow of Fuel
		  from tankAssy.fuelTankPort.fuelSupply
			to eng.engineFuelPort.fuelSupply;
			
		flow of Fuel
		  from eng.engineFuelPort.fuelReturn
			to tankAssy.fuelTankPort.fuelReturn;
	} 
}
```

## Elements

- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
