---
name: Subsetting Example
kind: example
language: SysML
source: sysml/src/training/04. Subsetting/Subsetting Example.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Subsetting Example

Verbatim SysML model from `sysml/src/training/04. Subsetting/Subsetting Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Subsetting Example' {
	
	part def Vehicle {
		part parts : VehiclePart[*];
		
		part eng : Engine subsets parts;
		part trans : Transmission subsets parts;
		part wheels : Wheel[4] :> parts;
	}
	
	abstract part def VehiclePart;
	part def Engine :> VehiclePart;
	part def Transmission :> VehiclePart;
	part def Wheel :> VehiclePart;
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
