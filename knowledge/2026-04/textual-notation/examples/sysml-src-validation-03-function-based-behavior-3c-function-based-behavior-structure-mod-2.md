---
name: 3c-Function-based Behavior-structure mod-2
kind: example
language: SysML
source: sysml/src/validation/03-Function-based Behavior/3c-Function-based Behavior-structure mod-2.sysml
elements: [ActionUsage, ConnectionDefinition, ConnectionUsage, PartDefinition, PartUsage, PerformActionUsage]
license: EPL-2.0
---

# 3c-Function-based Behavior-structure mod-2

Verbatim SysML model from `sysml/src/validation/03-Function-based Behavior/3c-Function-based Behavior-structure mod-2.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '3c-Function-based Behavior-structure mod-2' {
	
	part def Vehicle;
	part def VehicleFrame;
	
	part def HitchBall;
	part def TrailerCoupler;
	
	part def Trailer;
	part def TrailerFrame;
	
	connection def TrailerHitch {
		end hitch : HitchBall;
		end coupler : TrailerCoupler;
	}
	
	part 'vehicle-trailer system' {
		
		part vehicle : Vehicle {
			part vehicleFrame : VehicleFrame {
				part hitch : HitchBall;
			}
		}
		
		connection trailerHitch : TrailerHitch[0..1]
			connect vehicle.vehicleFrame.hitch to trailer.trailerFrame.coupler;
		
		part trailer : Trailer {
			part trailerFrame : TrailerFrame {
				part coupler : TrailerCoupler;
			}
		}
		
		perform action {
			action 'connect trailer to vehicle' {
				// Assert that exactly one connection exists during the
				// performance of this action.
				abstract ref :>> trailerHitch[1];
			}
			then action 'disconnect trailer from vehicle' {
				// Assert that exactly no connection exists during the
				// performance of this action.
				abstract ref :>> trailerHitch[0];		
			}
		}
		
	}
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [ConnectionDefinition](../metamodel/elements/ConnectionDefinition.md)
- [ConnectionUsage](../metamodel/elements/ConnectionUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PerformActionUsage](../metamodel/elements/PerformActionUsage.md)
