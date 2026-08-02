---
name: A-2-ModelingInstances
kind: example
language: KerML
source: kerml/src/examples/KerML Spec Annex A Examples/A-2-ModelingInstances.kerml
elements: []
license: EPL-2.0
---

# A-2-ModelingInstances

Verbatim KerML model from `kerml/src/examples/KerML Spec Annex A Examples/A-2-ModelingInstances.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package ModelingInstances {
	doc
	/* 
	 */

	classifier Vehicle;
	classifier Bicycle specializes Vehicle;
	classifier MyBike [1] specializes Bicycle;
	classifier YourBike [1] specializes Bicycle disjoint from MyBike;
}

package ModelingInstancesWithAtoms {
	doc
	/* 
	 */

	private import Atoms::atom;

	classifier Vehicle;
	classifier Bicycle specializes Vehicle;

	#atom
	classifier MyBike specializes Bicycle;
	#atom
	classifier YourBike specializes Bicycle;

	/* Assigning feature values. */

	classifier Garage {
		feature stores : Bicycle [*];
	}
	classifier OurBicycle unions MyBike, YourBike;

	#atom
	classifier OurGarage specializes Garage {
		feature redefines stores : OurBicycle [2];
	}
}
```
