---
name: Ports
kind: example
language: SysML
constructs: ["port def", port, "in item", "out item"]
elements: [PortDefinition, PortUsage, ItemUsage, AttributeUsage, AttributeDefinition, PartDefinition]
source: "sources/textual/sysml/training/10. Ports/Port Example.sysml"
license: EPL-2.0
---

# Ports

A worked SysML v2 textual-notation example: port definitions carrying directed items and attributes,
used on parts.

## Notation

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

## Constructs & elements

- `port def { … }` – [PortDefinition](../../metamodel/elements/PortDefinition.md): a definition of a connection point.
- `port fuelTankPort : FuelOutPort` – [PortUsage](../../metamodel/elements/PortUsage.md): a port feature on a part.
- `in item` / `out item` – directed [ItemUsage](../../metamodel/elements/ItemUsage.md) features; the `in`/`out` direction flips on a conjugated port.
- `attribute x : T` – [AttributeUsage](../../metamodel/elements/AttributeUsage.md).
- `attribute def`, `part def` – [AttributeDefinition](../../metamodel/elements/AttributeDefinition.md), [PartDefinition](../../metamodel/elements/PartDefinition.md).

## Source

`sources/textual/sysml/training/10. Ports/Port Example.sysml` – from
Systems-Modeling/SysML-v2-Release (EPL-2.0); see [NOTICE](../../../NOTICE).
