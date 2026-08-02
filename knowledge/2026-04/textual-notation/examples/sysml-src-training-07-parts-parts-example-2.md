---
name: Parts Example-2
kind: example
language: SysML
source: sysml/src/training/07. Parts/Parts Example-2.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Parts Example-2

Verbatim SysML model from `sysml/src/training/07. Parts/Parts Example-2.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Parts Example-2' {
	
	// Definitions
	
	part def Vehicle;	
	part def Engine;	
	part def Cylinder;
	
	// Usages
	
	part vehicle : Vehicle {
		part eng : Engine {
			part cyl : Cylinder[4..6];
		}
	}
	
	part smallVehicle :> vehicle {
		part redefines eng {
			part redefines cyl[4];
		}
	}
	
	part bigVehicle :> vehicle {
		part redefines eng {
			part redefines cyl[6];
		}
	}
	
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
