---
name: Items Example
kind: example
language: SysML
source: sysml/src/training/08. Items/Items Example.sysml
elements: [AttributeUsage, ItemDefinition, ItemUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Items Example

Verbatim SysML model from `sysml/src/training/08. Items/Items Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Items Example' {
	private import ScalarValues::*;
	
	item def Fuel;
	item def Person;
	
	part def Vehicle {
		attribute mass : Real;
		
		ref item driver : Person;

		part fuelTank {
			item fuel: Fuel;
		}		
	}
	
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
