---
name: VehicleDefinitions
kind: example
language: SysML
source: sysml/src/examples/Vehicle Example/VehicleDefinitions.sysml
elements: [AttributeUsage, FlowUsage, InterfaceDefinition, InterfaceUsage, PartDefinition, PartUsage, PortDefinition, PortUsage]
license: EPL-2.0
---

# VehicleDefinitions

Verbatim SysML model from `sysml/src/examples/Vehicle Example/VehicleDefinitions.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package VehicleDefinitions {
	doc
	/*
	 * Example vehicle definitions model.
	 */

	private import ScalarValues::*;
	private import Quantities::*;
	private import MeasurementReferences::*;
	private import ISQ::*;
	private import SI::*;
	
	/* PART DEFINITIONS */
	
	part def Vehicle {
		attribute mass :> ISQ::mass;
	}
	part def Transmission;	
	part def AxleAssembly;
	part def Axle {
		port leftMountingPoint: AxleMountIF;
		port rightMountingPoint: AxleMountIF;
	}
	part def Wheel {
		port hub: WheelHubIF;
	}
	part def Lugbolt {
		attribute tighteningTorque :> ISQ::torque;
	}
	
	/* PORT DEFINITIONS */
	
	port def DriveIF { 
		in driveTorque :> ISQ::torque;
	}
	
	port def AxleMountIF { 
		out transferredTorque :> ISQ::torque;
	}
	
	port def WheelHubIF { 
		in appliedTorque :> ISQ::torque;
	}
	
	/* INTERFACE DEFINITIONS */
	
	interface def Mounting {
		doc /* The definition of the interface for mounting a Wheel to an Axle. */
		end axleMount: AxleMountIF;
		end hub: WheelHubIF;
		
		flow axleMount.transferredTorque to hub.appliedTorque;
	}
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [InterfaceDefinition](../metamodel/elements/InterfaceDefinition.md)
- [InterfaceUsage](../metamodel/elements/InterfaceUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortDefinition](../metamodel/elements/PortDefinition.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
