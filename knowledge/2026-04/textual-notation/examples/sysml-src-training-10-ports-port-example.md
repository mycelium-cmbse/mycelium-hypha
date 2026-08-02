---
name: Port Example
kind: example
language: SysML
source: sysml/src/training/10. Ports/Port Example.sysml
elements: [AttributeDefinition, AttributeUsage, ItemUsage, PartDefinition, PartUsage, PortDefinition, PortUsage]
license: EPL-2.0
---

# Port Example

Verbatim SysML model from `sysml/src/training/10. Ports/Port Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Port Example' {
	
	attribute def Temp;
	
	part def Fuel;
	
	port def FuelOutPort {
		attribute temperature : Temp;
		out item fuelSupply : Fuel;
		in item fuelReturn : Fuel;
	}
	
	port def FuelInPort {
		attribute temperature : Temp;
		in item fuelSupply : Fuel;
		out item fuelReturn : Fuel;
	}
	
	part def FuelTankAssembly {
		port fuelTankPort : FuelOutPort;
	}
	
	part def Engine {
		port engineFuelPort : FuelInPort;
	}
}
```

## Elements

- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortDefinition](../metamodel/elements/PortDefinition.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
