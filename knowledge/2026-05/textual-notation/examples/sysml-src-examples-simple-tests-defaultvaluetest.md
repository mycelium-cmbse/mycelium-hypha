---
name: DefaultValueTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/DefaultValueTest.sysml
elements: [AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# DefaultValueTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/DefaultValueTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package DefaultValueTest {
	
	part def V {
		attribute m default = 10;
		attribute n = 20;
	}
	
	part v1 : V {
		attribute :>> m = 20;
	}
	
	part def W :> V {
		attribute :>> m default = n;
	}
	
	part v2 = new W();
	
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
