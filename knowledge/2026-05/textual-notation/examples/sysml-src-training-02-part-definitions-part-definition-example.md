---
name: Part Definition Example
kind: example
language: SysML
source: sysml/src/training/02. Part Definitions/Part Definition Example.sysml
elements: [AttributeDefinition, AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Part Definition Example

Verbatim SysML model from `sysml/src/training/02. Part Definitions/Part Definition Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Part Definition Example' {
	private import ScalarValues::*;
	
	part def Vehicle {
		attribute mass : Real;
		attribute status : VehicleStatus;
		
		part eng : Engine;
		
		ref part driver : Person;
	}
	
	attribute def VehicleStatus {
		attribute gearSetting : Integer;
		attribute acceleratorPosition : Real;
	}
	
	part def Engine;	
	part def Person;
}
```

## Elements

- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
