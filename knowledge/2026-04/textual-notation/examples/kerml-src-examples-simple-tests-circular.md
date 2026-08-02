---
name: Circular
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Circular.kerml
elements: []
license: EPL-2.0
---

# Circular

Verbatim KerML model from `kerml/src/examples/Simple Tests/Circular.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Circular {
	class A { }
	feature a: A;
	alias Circ for Circular;
	package P {
		public import Circular::*;
	}
	
	feature x :> z;
	feature y :> x;
	feature z :> y;
}
```
