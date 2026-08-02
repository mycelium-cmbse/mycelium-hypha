---
name: Vehicles_2
kind: example
language: KerML
source: kerml/src/examples/Mass Roll-up Example/Vehicles_2.kerml
elements: []
license: EPL-2.0
---

# Vehicles_2

Verbatim KerML model from `kerml/src/examples/Mass Roll-up Example/Vehicles_2.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Vehicles_2 {
	private import ScalarValues::String;
	private import MassRollup_1::*;
	
	class CarPart specializes MassedThing {		
		feature serialNumber: String;
		feature m redefines mass;
		
		composite subparts: CarPart[0..*] redefines subcomponents;
	}
	
	feature vehicle: CarPart {	
		feature vin redefines serialNumber;
		
		composite engine: CarPart subsets subparts {
			//...
		}
		
		composite transmission: CarPart subsets subparts {
			//...
		}
	}
	
	// Example usage
	
	private import SI::*;
	feature v: vehicle {
		feature m redefines CarPart::m = 1000;
		composite engine redefines vehicle::engine {
			feature m redefines CarPart::m = 100;
		}
		composite transmission redefines vehicle::transmission {
			feature m redefines CarPart::m = 50;
		}
	}
	
	// v.totalMass evaluates to 1150.0
}
```
