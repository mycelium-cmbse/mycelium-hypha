---
name: AllocationTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/AllocationTest.sysml
elements: [AllocationDefinition, AllocationUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# AllocationTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/AllocationTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package AllocationTest {
	part def Logical {
		part component;
	}
	
	part def Physical {
		part assembly {
			part element;
		}
	}
	
	part l : Logical {
		part :>> component;
	}
	part p : Physical {
		part :>> assembly {
			part :>> element;
		}
        allocate l.component to assembly.element;
	}
	
	allocation def A;
	
	allocation def Logical_to_Physical :> A {
		end logical : Logical;
		end physical : Physical;
	}
	
	allocation allocation1 : Logical_to_Physical allocate l to p;	
	allocation allocation2 : Logical_to_Physical allocate (
		logical ::> l,
		physical ::> p
	);

	allocate l.component to p.assembly.element;
}
```

## Elements

- [AllocationDefinition](../metamodel/elements/AllocationDefinition.md)
- [AllocationUsage](../metamodel/elements/AllocationUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
