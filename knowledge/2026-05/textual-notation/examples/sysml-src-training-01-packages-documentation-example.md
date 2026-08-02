---
name: Documentation Example
kind: example
language: SysML
source: sysml/src/training/01. Packages/Documentation Example.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Documentation Example

Verbatim SysML model from `sysml/src/training/01. Packages/Documentation Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Documentation Example' {
	doc /* This is documentation of the owning 
	     * package.
	     */
	
	part def Automobile {
		doc Document1 /* This documentation of Automobile. */
	}
	
	alias Car for Automobile {
		doc /* This is documentation of the alias. */
	}
	alias Torque for ISQ::TorqueValue;
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
