---
name: Redefinition Example
kind: example
language: SysML
source: sysml/src/training/05. Redefinition/Redefinition Example.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Redefinition Example

Verbatim SysML model from `sysml/src/training/05. Redefinition/Redefinition Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Redefinition Example' {

	part def Vehicle {
		part eng : Engine;
	}
	part def SmallVehicle :> Vehicle {
		part smallEng : SmallEngine redefines eng;
	}
	part def BigVehicle :> Vehicle {
		part bigEng : BigEngine :>> eng;
	}

	part def Engine {
		part cyl : Cylinder[4..6];
	}
	part def SmallEngine :> Engine {
		part redefines cyl[4];
	}
	part def BigEngine :> Engine {
		part redefines cyl[6];
	}

	part def Cylinder;
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
