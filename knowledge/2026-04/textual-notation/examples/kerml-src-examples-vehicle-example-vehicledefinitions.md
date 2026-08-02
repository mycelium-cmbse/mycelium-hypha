---
name: VehicleDefinitions
kind: example
language: KerML
source: kerml/src/examples/Vehicle Example/VehicleDefinitions.kerml
elements: []
license: EPL-2.0
---

# VehicleDefinitions

Verbatim KerML model from `kerml/src/examples/Vehicle Example/VehicleDefinitions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package VehicleDefinitions {
	doc
	/*
	 * Example vehicle definitions model.
	 */

	
	/* BLOCKS */
	
	class Vehicle;	
	class Transmission;	
	class AxleAssembly;
	class Axle;	
	class Wheel;
	class Lugbolt {
		tighteningTorque[1] : ScalarValues::Real;
	}
	
	/* INTERFACE BLOCKS */
	
	class DriveIF { 
		in driveTorque: ScalarValues::Real;
	}
	
	class AxleMountIF { 
		out transferredTorque : ScalarValues::Real;
	}
	
	class WheelHubIF { 
		in appliedTorque : ScalarValues::Real;
	}
	
	/* ASSOCIATION BLOCKS */
	
	assoc Mounting {
		doc
		/*
		 *  mounting a Wheel to an Axle.
		 */
	
		end axleMount: AxleMountIF;
		end hub: WheelHubIF;
	}
}
```
