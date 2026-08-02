---
name: ArgumentResolution
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/ArgumentResolution.kerml
elements: []
license: EPL-2.0
---

# ArgumentResolution

Verbatim KerML model from `kerml/src/examples/Simple Tests/ArgumentResolution.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package ArgumentResolutionBug {
	class A {
		feature x;
	}
	
	behavior B  {
		in feature x;
		out feature : A = new A(x);
	}
	
	class C {
		feature a : A;
		feature b : B;
		
		connector a ::> a.x to b;
	}
}
```
