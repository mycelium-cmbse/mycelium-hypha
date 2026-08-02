---
name: PrivateImportTest
kind: example
language: SysML
source: sysml/src/examples/Import Tests/PrivateImportTest.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# PrivateImportTest

Verbatim SysML model from `sysml/src/examples/Import Tests/PrivateImportTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package PrivateImportTest {
	package P1 {
		part def A;
	}
	package P2 {
		private import P1::*;
	}

	part x: P1::A;
	
	public import P2::*;
	// This should fail.
	// A is not visible, because the import in P2 is private.
	// part y: A;
	// part y1: P2::A;
	
	package P3 {
		part def B;
	}
	
	private import P3::*;
	
	// This should not fail.
	// Private import only restricts visibility outside the package.
	part z: B;
	
	package P4 {
		public import all P2::*;
		
		// This should not fail because "import all" overrides private import.
		part z1: A;
	}	
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
