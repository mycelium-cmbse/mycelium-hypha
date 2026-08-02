---
name: QualifiedNameImportTest
kind: example
language: SysML
source: sysml/src/examples/Import Tests/QualifiedNameImportTest.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# QualifiedNameImportTest

Verbatim SysML model from `sysml/src/examples/Import Tests/QualifiedNameImportTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package QualifiedNameImportTest {
	package P1 {
		part def A;
	}
	package P2 {
		package P2a {
			public import P1::*;
		}
		// The following should not fail.
		// A is a member of P2a because of the import.
		part x: P2a::A;
	}
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
