---
name: Inheritance
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Inheritance.kerml
elements: []
license: EPL-2.0
---

# Inheritance

Verbatim KerML model from `kerml/src/examples/Simple Tests/Inheritance.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Inheritance {
	class A {
		feature f;
	}
	
	class B specializes A {
		
	}
		
	feature y: A {
		alias x for B::f;
		feature g redefines f;
	}
	
	alias z for y::g;
	
	feature w subsets y;
	
	alias us for w::g;
	
	feature yy: y;
}
```
