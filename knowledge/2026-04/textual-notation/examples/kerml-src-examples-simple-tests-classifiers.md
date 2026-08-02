---
name: Classifiers
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Classifiers.kerml
elements: []
license: EPL-2.0
---

# Classifiers

Verbatim KerML model from `kerml/src/examples/Simple Tests/Classifiers.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Classifiers {
	classifier A;
	classifier B;
	
	specialization Super subclassifier A specializes B;
	specialization subclassifier B :> A;
	
	subclassifier C specializes A;
	subclassifier C specializes B;
	
	classifier C specializes A, B;
	
	classifier D disjoint from C differences A, B;
	classifier E specializes C intersects A, B;
	classifier F unions A unions B;
}
```
