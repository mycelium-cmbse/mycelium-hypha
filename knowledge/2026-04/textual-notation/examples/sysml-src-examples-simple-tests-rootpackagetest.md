---
name: RootPackageTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/RootPackageTest.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# RootPackageTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/RootPackageTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package P1 {
	part def A;
}

package P2 {
	private import P1::*;
	part a : A;
}

private import P2::*;

package P3 {
	part b subsets a;
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
