---
name: MassRollup_2
kind: example
language: KerML
source: kerml/src/examples/Mass Roll-up Example/MassRollup_2.kerml
elements: []
license: EPL-2.0
---

# MassRollup_2

Verbatim KerML model from `kerml/src/examples/Mass Roll-up Example/MassRollup_2.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package MassRollup_2 {
	private import NumericalFunctions::*;
	private import ISQ::*;
	
	class MassedThing {
		feature mass : ScalarValues::Real; 
		feature totalMass : ScalarValues::Real =
			mass + sum(subcomponents.totalMass);
			
		feature subcomponents redefines massedThings;	
	}
	
	feature massedThings: MassedThing[0..*];

}
```
