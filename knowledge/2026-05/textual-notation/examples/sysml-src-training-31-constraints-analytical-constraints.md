---
name: Analytical Constraints
kind: example
language: SysML
source: sysml/src/training/31. Constraints/Analytical Constraints.sysml
elements: [ActionDefinition, ActionUsage, AssertConstraintUsage, AttributeUsage, ConstraintDefinition, ConstraintUsage]
license: EPL-2.0
---

# Analytical Constraints

Verbatim SysML model from `sysml/src/training/31. Constraints/Analytical Constraints.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Analytical Constraints' {
	private import ISQ::*;
	private import 'Calculation Definitions'::*;
	
	constraint def StraightLineDynamicsEquations {
		in p : PowerValue;
		in m : MassValue;
		in dt : TimeValue;
		in x_i : LengthValue;
		in v_i : SpeedValue;
		in x_f : LengthValue;
		in v_f : SpeedValue;
		in a : AccelerationValue;
	
		attribute v_avg : SpeedValue = (v_i + v_f)/2;
		
		a == Acceleration(p, m, v_avg) and
		v_f == Velocity(dt, v_i, a) and
		x_f == Position(dt, x_i, v_avg)
	}
	
	action def StraightLineDynamics {
		in power : PowerValue;
		in mass : MassValue;
		in delta_t : TimeValue;
		in x_in : LengthValue;
		in v_in : SpeedValue;
		out x_out : LengthValue;
		out v_out : SpeedValue;
		out a_out : AccelerationValue;
	
	    assert constraint dynamics : StraightLineDynamicsEquations {
			in p = power;
			in m = mass;
			in dt = delta_t;
			in x_i = x_in;
			in v_i = v_in;
			in x_f = x_out;
			in v_f = v_out;
			in a = a_out;
	    }
	}
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AssertConstraintUsage](../metamodel/elements/AssertConstraintUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintDefinition](../metamodel/elements/ConstraintDefinition.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
