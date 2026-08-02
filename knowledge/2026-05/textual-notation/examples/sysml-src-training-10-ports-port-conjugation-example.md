---
name: Port Conjugation Example
kind: example
language: SysML
source: sysml/src/training/10. Ports/Port Conjugation Example.sysml
elements: [AttributeDefinition, AttributeUsage, ItemUsage, PartDefinition, PartUsage, PortDefinition, PortUsage]
license: EPL-2.0
---

# Port Conjugation Example

Verbatim SysML model from `sysml/src/training/10. Ports/Port Conjugation Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Port Conjugation Example' {
	
	attribute def Temp;
	
	part def Fuel;
	
	port def FuelPort {
		attribute temperature : Temp;
		out item fuelSupply : Fuel;
		in item fuelReturn : Fuel;
	}
	
	part def FuelTank {
		port fuelTankPort : FuelPort;
	}
	
	part def Engine {
		port engineFuelPort : ~FuelPort;
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
