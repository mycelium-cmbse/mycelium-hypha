---
name: ConjugationTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/ConjugationTest.sysml
elements: [ConnectionDefinition, ConnectionUsage, InterfaceDefinition, InterfaceUsage, PartDefinition, PartUsage, PortDefinition, PortUsage]
license: EPL-2.0
---

# ConjugationTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/ConjugationTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ConjugationTest {
	port def P;
	
	part def B {
		port p1: P;
		port p2: ~P;
	}
	
	connection def A {
		end port p1: P;
		end port p2: ~P;
	}
	
	interface def I {
		end p1: P;
		end p2: ~P;
	}
	
	part def B1 {
		part p {
			port p1: P;
			port p2: ~P;		
		}
	
		connection a: A {
			end port p3: P ::> p.p1;
			end port p4: ~P ::> p.p2;
		}
		interface i: I {
			end port p3: P ::> p.p1;
			end port p4: ~P ::> p.p2;
		}
	}
	
}
```

## Elements

- [ConnectionDefinition](../metamodel/elements/ConnectionDefinition.md)
- [ConnectionUsage](../metamodel/elements/ConnectionUsage.md)
- [InterfaceDefinition](../metamodel/elements/InterfaceDefinition.md)
- [InterfaceUsage](../metamodel/elements/InterfaceUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortDefinition](../metamodel/elements/PortDefinition.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
