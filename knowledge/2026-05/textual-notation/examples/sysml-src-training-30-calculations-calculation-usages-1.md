---
name: Calculation Usages-1
kind: example
language: SysML
source: sysml/src/training/30. Calculations/Calculation Usages-1.sysml
elements: [ActionUsage, AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Calculation Usages-1

Verbatim SysML model from `sysml/src/training/30. Calculations/Calculation Usages-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Calculation Usages-1' {
	private import ScalarValues::Real;
	private import ISQ::*;
	private import 'Calculation Definitions'::*;
	
	part def VehicleDynamics {
		attribute C_d : Real;
		attribute C_f : Real;
		attribute wheelPower : PowerValue;
		attribute mass : MassValue;
		
		action straightLineDynamics {
			in delta_t : TimeValue;
			in v_in : SpeedValue;
			in x_in : LengthValue;
			out v_out : SpeedValue = vel.v;
			out x_out : LengthValue = pos.x;
		
			calc acc : Acceleration {
				in tp = Power(wheelPower, C_d, C_f, mass, v_in);
				in tm = mass;
				in v = v_in;
				return a;
			}
			
			calc vel : Velocity {
				in dt = delta_t;
				in v0 = v_in;
				in a = acc.a;
				return v;
			}
			
			calc pos : Position {
				in dt = delta_t;
				in x0 = x_in;
				in v0 = vel.v;
				return x;	
			}
		}
	} 
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
