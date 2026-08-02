---
name: MultiplicityTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/MultiplicityTest.sysml
elements: [AttributeDefinition, AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# MultiplicityTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/MultiplicityTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package MultiplicityTest {
	
	part def P;
	attribute n : ScalarValues::Integer = 5;
	
	part a[1];
	part b[0..2] : P;
	part c : P[2..*];
	part d[*];
	
	part e[n];
	part f[n..*];
	part g[1..n];

	attribute def A {
		attribute i :ScalarValues::Integer;
		attribute x : A[i];
	}
}
```

## Elements

- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
