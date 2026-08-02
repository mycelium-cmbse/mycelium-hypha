---
name: Classifications
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Classifications.kerml
elements: []
license: EPL-2.0
---

# Classifications

Verbatim KerML model from `kerml/src/examples/Simple Tests/Classifications.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Classifications {
	class T;
	x;
	y = x istype T or x hastype z;
	z = (all T)#(3);
	a = x as T;
	b = x meta KerML::Feature;
}
```
