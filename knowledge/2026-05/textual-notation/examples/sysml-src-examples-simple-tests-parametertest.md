---
name: ParameterTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/ParameterTest.sysml
elements: [AttributeDefinition, AttributeUsage]
license: EPL-2.0
---

# ParameterTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/ParameterTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ParameterTest {
	attribute def A {
		attribute x : ScalarValues::String;
		attribute y : A;
	}
	
	attribute a : A;
	
	calc def F { in p : A; in q : ScalarValues::Integer; return :  ScalarValues::Integer; }
	
	attribute f = F(a, 2);
	attribute g = F(q = 1, p = a);
	
	attribute b = new A(y=a, x=""); 
	attribute c = new A("test2");
}
```

## Elements

- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
