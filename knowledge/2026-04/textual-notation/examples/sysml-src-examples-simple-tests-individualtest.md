---
name: IndividualTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/IndividualTest.sysml
elements: [ActionDefinition, ActionUsage, ItemDefinition, ItemUsage, OccurrenceDefinition, OccurrenceUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# IndividualTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/IndividualTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package IndividualTest {
	individual def IO1;
	individual occurrence def IO2 {
		individual io : IO1;
	}
	
	individual item def II1 {
		individual item ii : II1;
	}
	
	item def I {
		part i : I;
	}
	individual item def II2 :> I {
		individual item :>> i : II2;
	}
	
	individual part def IP1 {
		individual part p : IP1;
	}
	
	part def P {
		part p : P;
	}
	individual part def IP2 :> P {
		individual part :>> p : IP2;
	}
	
	individual action def AP1 {
		individual action a : AP1;
	}
	
	action def A {
		action a : A;
	}
	individual action def IA2 :> A {
		individual action :>> a : IA2;
	}
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [OccurrenceDefinition](../metamodel/elements/OccurrenceDefinition.md)
- [OccurrenceUsage](../metamodel/elements/OccurrenceUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
