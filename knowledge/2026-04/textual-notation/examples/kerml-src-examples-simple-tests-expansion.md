---
name: Expansion
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Expansion.kerml
elements: []
license: EPL-2.0
---

# Expansion

Verbatim KerML model from `kerml/src/examples/Simple Tests/Expansion.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Expansion {
	private import ControlFunctions::select;
	feature x = x->select {in y; in w; in z; w+1}; 
}
```
