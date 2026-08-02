---
name: ItemTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/ItemTest.sysml
elements: [ItemDefinition, ItemUsage, PartDefinition, PartUsage, PortDefinition, PortUsage]
license: EPL-2.0
---

# ItemTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/ItemTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ItemTest {
	
	item f: A;

	public item def A {
		item b: B;
		protected ref part c: C;
	}
	
	abstract item def B {
		public abstract part a: A;
	}
	
	private part def C {
		private in ref y: A, B;
	}
	
	port def P {
		in item a1: A;
		out item a2: A;
	}
	
}
```

## Elements

- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortDefinition](../metamodel/elements/PortDefinition.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
