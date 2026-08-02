---
name: 3c-Function-based Behavior-structure mod-3
kind: example
language: SysML
source: sysml/src/validation/03-Function-based Behavior/3c-Function-based Behavior-structure mod-3.sysml
elements: [ActionUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# 3c-Function-based Behavior-structure mod-3

Verbatim SysML model from `sysml/src/validation/03-Function-based Behavior/3c-Function-based Behavior-structure mod-3.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '3c-Function-based Behavior-structure mod-3' {
	
	part def Vehicle;
	part def VehicleFrame;
	part def HitchBall;
	part def Trailer;
	part def TrailerFrame;
	part def TrailerCoupler;
	
	part vehicle : Vehicle {
		part vehicleFrame : VehicleFrame {
			part hitch : HitchBall;
		}
	}
	
	part trailer : Trailer {
		part trailerFrame : TrailerFrame {
			part coupler : TrailerCoupler {
				ref part hitch : HitchBall;
			}
		}		
	}
			
	action {
		// Insert the vehicle HitchBall into the TrailerCoupler.
		action 'connect trailer to vehicle'
			assign trailer.trailerFrame.coupler.hitch := vehicle.vehicleFrame.hitch;
		
		// Remove the HitchBall from the TrailerCoupler.
		then action 'disconnect trailer from vehicle'
			assign trailer.trailerFrame.coupler.hitch := null;
	}
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
