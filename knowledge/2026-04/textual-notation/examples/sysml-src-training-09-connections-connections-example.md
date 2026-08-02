---
name: Connections Example
kind: example
language: SysML
source: sysml/src/training/09. Connections/Connections Example.sysml
elements: [ConnectionDefinition, ConnectionUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Connections Example

Verbatim SysML model from `sysml/src/training/09. Connections/Connections Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Connections Example' {
	
	part def WheelHubAssembly;
	part def WheelAssembly;
	part def Tire;
	part def TireBead;
	part def Wheel;
	part def TireMountingRim;
	part def LugBoltMountingHole;
	part def Hub;
	part def LugBoltThreadableHole;
	part def LugBoltJoint;
	
	connection def PressureSeat {
		end [1] part bead : TireBead;
		end [1] part mountingRim : TireMountingRim;
	}
	
	part wheelHubAssembly : WheelHubAssembly {
		
		part wheel : WheelAssembly[1] {
			part t : Tire[1] {
				part bead : TireBead[2];			
			}
			part w: Wheel[1] {
				part rim : TireMountingRim[2];
				part mountingHoles : LugBoltMountingHole[5];
			}						
			connection : PressureSeat 
				connect bead references t.bead 
				to mountingRim references w.rim;		
		}
		
		part lugBoltJoints : LugBoltJoint[0..5];
		part hub : Hub[1] {
			part h : LugBoltThreadableHole[5];
		}
		connect [0..1] lugBoltJoints to [1] wheel.w.mountingHoles;
		connect [0..1] lugBoltJoints to [1] hub.h;
	}
	
}
```

## Elements

- [ConnectionDefinition](../metamodel/elements/ConnectionDefinition.md)
- [ConnectionUsage](../metamodel/elements/ConnectionUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
