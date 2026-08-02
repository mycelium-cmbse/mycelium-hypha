---
name: Inverses
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Inverses.kerml
elements: []
license: EPL-2.0
---

# Inverses

Verbatim KerML model from `kerml/src/examples/Simple Tests/Inverses.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Inverses {
	class A {
		feature f : B inverse of B::g disjoint from h;
		feature h : B;
	}
	
	class B {
		feature g : A;
	}
	
	inverse B::g of A::f;
	inverting Invert inverse B::g.f of A::h;
	
	feature gg : A featured by B inverse of A::f;
}
```
