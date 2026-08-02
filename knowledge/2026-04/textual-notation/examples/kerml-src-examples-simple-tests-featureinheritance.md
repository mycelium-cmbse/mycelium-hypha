---
name: FeatureInheritance
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/FeatureInheritance.kerml
elements: []
license: EPL-2.0
---

# FeatureInheritance

Verbatim KerML model from `kerml/src/examples/Simple Tests/FeatureInheritance.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package FeatureInheritance {
	feature s {
		feature t : ISQ::TorqueValue;
	}
	
	feature u subsets s;
}
```
