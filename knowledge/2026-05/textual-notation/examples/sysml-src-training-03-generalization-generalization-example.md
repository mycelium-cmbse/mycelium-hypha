---
name: Generalization Example
kind: example
language: SysML
source: sysml/src/training/03. Generalization/Generalization Example.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Generalization Example

Verbatim SysML model from `sysml/src/training/03. Generalization/Generalization Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Generalization Example' {

	abstract part def Vehicle;
	
	part def HumanDrivenVehicle specializes Vehicle {
		ref part driver : Person;
	}
	
	part def PoweredVehicle :> Vehicle {
		part eng : Engine;
	}
	
	part def HumanDrivenPoweredVehicle :> 
		HumanDrivenVehicle, PoweredVehicle;
	
	part def Engine;	
	part def Person;
	
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
