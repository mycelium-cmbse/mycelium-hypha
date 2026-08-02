---
name: Package Example
kind: example
language: SysML
source: sysml/src/training/01. Packages/Package Example.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Package Example

Verbatim SysML model from `sysml/src/training/01. Packages/Package Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Package Example' {
	public import ISQ::TorqueValue;
	private import ScalarValues::*;
	 
	private part def Automobile;
	
	public alias Car for Automobile;	                         
	alias Torque for ISQ::TorqueValue;
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
