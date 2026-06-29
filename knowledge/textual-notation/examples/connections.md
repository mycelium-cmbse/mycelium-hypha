---
name: Connections
kind: example
language: SysML
constructs: ["connection def", end, "connect ... to", multiplicity]
elements: [ConnectionDefinition, ConnectionUsage, PartUsage]
source: "sources/textual/sysml/training/09. Connections/Connections Example.sysml"
license: EPL-2.0
---

# Connections

A worked SysML v2 textual-notation example: a connection definition with ends, nested parts with
multiplicities, and both named and inline connections.

## Notation

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

## Constructs & elements

- `connection def { end … }` – [ConnectionDefinition](../../metamodel/elements/ConnectionDefinition.md): a definition relating two or more ends.
- `end [1] part bead : TireBead` – a connection end, a [PartUsage](../../metamodel/elements/PartUsage.md) playing the role of an end.
- `connection : PressureSeat connect … to …` – a named [ConnectionUsage](../../metamodel/elements/ConnectionUsage.md) typed by the definition.
- `connect [m] a to [n] b` – an inline `ConnectionUsage` with end multiplicities.
- `part x : T[mult]` – [PartUsage](../../metamodel/elements/PartUsage.md) with a multiplicity.

## Source

`sources/textual/sysml/training/09. Connections/Connections Example.sysml` – from
Systems-Modeling/SysML-v2-Release (EPL-2.0); see [NOTICE](../../../NOTICE).
