---
name: MassRollup_1
kind: example
language: KerML
source: kerml/src/examples/Mass Roll-up Example/MassRollup_1.kerml
elements: []
license: EPL-2.0
---

# MassRollup_1

Verbatim KerML model from `kerml/src/examples/Mass Roll-up Example/MassRollup_1.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package MassRollup_1 {
	private import NumericalFunctions::*;

	class MassedThing {
		feature mass : ScalarValues::Real;	
		composite subcomponents: MassedThing[0..*];

		feature totalMass : ScalarValues::Real = 
			mass + sum(subcomponents.totalMass);
	}
}
```
