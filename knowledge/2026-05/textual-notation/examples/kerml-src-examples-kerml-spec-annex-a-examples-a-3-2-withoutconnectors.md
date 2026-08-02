---
name: A-3-2-WithoutConnectors
kind: example
language: KerML
source: kerml/src/examples/KerML Spec Annex A Examples/A-3-2-WithoutConnectors.kerml
elements: []
license: EPL-2.0
---

# A-3-2-WithoutConnectors

Verbatim KerML model from `kerml/src/examples/KerML Spec Annex A Examples/A-3-2-WithoutConnectors.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml

package WithoutConnectorsModelToBeExecuted {
	doc
	/* 
	 */

	classifier Bicycle {
		feature rollsOn : Wheel [2];
		feature holdsWheel : BikeFork [*];
	}
	classifier Wheel;
	classifier BikeFork;
}

package WithoutConnectorsExecution {
	doc
	/* 
	 */

	private import Atoms::*;
	private import WithoutConnectorsModelToBeExecuted::*;

	#atom
	classifier MyWheel1 specializes Wheel;
	#atom
	classifier MyWheel2 specializes Wheel;

	classifier MyWheel unions MyWheel1, MyWheel2;

	#atom
	classifier MyBike specializes Bicycle {
		feature redefines rollsOn : MyWheel;
	}
}
```
