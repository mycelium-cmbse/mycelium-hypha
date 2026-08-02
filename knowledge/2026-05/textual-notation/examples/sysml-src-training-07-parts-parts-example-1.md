---
name: Parts Example-1
kind: example
language: SysML
source: sysml/src/training/07. Parts/Parts Example-1.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Parts Example-1

Verbatim SysML model from `sysml/src/training/07. Parts/Parts Example-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Parts Example-1' {
	
	// Definitions
	
	part def Vehicle {
		part eng : Engine;
	}
	
	part def Engine {
		part cyl : Cylinder[4..6];
	}
	
	part def Cylinder;
	
	// Usages
	
	part smallVehicle : Vehicle {
		part redefines eng {
			part redefines cyl[4];
		}
	}
	
	part bigVehicle : Vehicle {
		part redefines eng {
			part redefines cyl[6];
		}
	}
	
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
