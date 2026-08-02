---
name: Conjugation
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Conjugation.kerml
elements: []
license: EPL-2.0
---

# Conjugation

Verbatim KerML model from `kerml/src/examples/Simple Tests/Conjugation.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Conjugation {
	class A {
		in feature f;
	}
	
	class B conjugates A;
	
	feature g ~ B::f;
}
```
