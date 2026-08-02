---
name: Binding Connectors Example-1
kind: example
language: SysML
source: sysml/src/training/12. Binding Connectors/Binding Connectors Example-1.sysml
elements: [ItemUsage, PartDefinition, PartUsage, PortUsage]
license: EPL-2.0
---

# Binding Connectors Example-1

Verbatim SysML model from `sysml/src/training/12. Binding Connectors/Binding Connectors Example-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Binding Connectors Example-1' {
	private import 'Port Example'::*;
	
	part def Vehicle;
	part def FuelPump;
	part def FuelTank;
	
	part vehicle : Vehicle {	
		part tank : FuelTankAssembly {
			port redefines fuelTankPort {
				out item redefines fuelSupply;
				in item redefines fuelReturn;
			}
			
			bind fuelTankPort.fuelSupply = pump.pumpOut;
			bind fuelTankPort.fuelReturn = tank.fuelIn;
			
			part pump : FuelPump {
				out item pumpOut : Fuel;
				in item pumpIn : Fuel;
			}
			
			part tank : FuelTank {
				out item fuelOut : Fuel;
				in item fuelIn : Fuel;
			}
		}
	} 
}
```

## Elements

- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
