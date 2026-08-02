---
name: CircularImport
kind: example
language: SysML
source: sysml/src/examples/Import Tests/CircularImport.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# CircularImport

Verbatim SysML model from `sysml/src/examples/Import Tests/CircularImport.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package CircularImport {

	package P1 {
		public import P2::*;
		part def A;
	}
	package P2 {
		public import P1::*;
		part def B;
	}
	package Test1 {
		public import P1::*;
		part x: A;
		part y: B;
	}
	package Test2 {
		public import P2::*;
		part x: A;
		part y: B;
	}
	
	part x: P1::A;
	
	// The following should not fail.
	part y: P1::B;
	
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
