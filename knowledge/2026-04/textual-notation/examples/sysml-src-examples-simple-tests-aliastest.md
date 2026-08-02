---
name: AliasTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/AliasTest.sysml
elements: [AttributeUsage, PartDefinition, PartUsage, PortUsage]
license: EPL-2.0
---

# AliasTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/AliasTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package AliasTest {
	private import ISQSpaceTime::breadth; // import of an alias
	attribute b :> breadth;
	
    part def P1 {
        port porig1;
        alias po1 for porig1;
    }

    part p1 : P1 {
        port po1 :>> po1;
    }

    part p2 : P1 {
        port pdest;
        alias pd1 for pdest;
    }


    connect p1.po1 to p2.pdest;
	connect p1.po1 to p2.pd1;
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
