---
name: Assignment Example
kind: example
language: SysML
source: sysml/src/training/20. Assignment Actions/Assignment Example.sysml
elements: [ActionDefinition, ActionUsage, AttributeUsage, PerformActionUsage]
license: EPL-2.0
---

# Assignment Example

Verbatim SysML model from `sysml/src/training/20. Assignment Actions/Assignment Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'For Loop Example' {
	private import SequenceFunctions::*;
	
    action def StraightLineDynamics {
        in power : ISQ::PowerValue;
        in mass : ISQ::MassValue;
        in delta_t : ISQ::TimeValue;
        in x_in : ISQ::LengthValue;
        in v_in : ISQ::SpeedValue;
        out x_out : ISQ::LengthValue;
        out v_out : ISQ::SpeedValue;
    }
	    
	action def ComputeMotion {
		in attribute powerProfile :> ISQ::power[*];
		in attribute vehicleMass :> ISQ::mass;
		in attribute initialPosition :> ISQ::length;
		in attribute initialSpeed :> ISQ::speed;
		in attribute deltaT :> ISQ::time;
		out attribute positions :> ISQ::length[*] := ( );
		
		private attribute position := initialPosition;
		private attribute speed := initialSpeed;
		
		for vehiclePower in powerProfile {
			perform action dynamics : StraightLineDynamics {
				in power = vehiclePower;
				in mass = vehicleMass;
				in delta_t = deltaT;
				in x_in = position;
				in v_in = speed;
				out x_out;
				out v_out;
			}
			then assign position := dynamics.x_out;
			then assign speed := dynamics.v_out;
			then assign positions := positions->including(position);
		}
	}
	
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PerformActionUsage](../metamodel/elements/PerformActionUsage.md)
