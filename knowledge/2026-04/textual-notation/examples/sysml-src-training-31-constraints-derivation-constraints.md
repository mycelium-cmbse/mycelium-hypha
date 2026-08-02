---
name: Derivation Constraints
kind: example
language: SysML
source: sysml/src/training/31. Constraints/Derivation Constraints.sysml
elements: [AssertConstraintUsage, AttributeUsage, ConstraintDefinition, ConstraintUsage, PartUsage]
license: EPL-2.0
---

# Derivation Constraints

Verbatim SysML model from `sysml/src/training/31. Constraints/Derivation Constraints.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Derivation Constraints' {
	private import SI::*;
	private import 'Constraints Example-1'::*;
	
	part vehicle1 : Vehicle {
		attribute totalMass : MassValue;			
		assert constraint {totalMass == chassisMass + engine.mass + transmission.mass}	
	}
	
	part vehicle2 : Vehicle {
		attribute totalMass : MassValue = chassisMass + engine.mass + transmission.mass;
	}
	
	constraint def Dynamics {
		in mass: MassValue;
		in initialSpeed : SpeedValue;
		in finalSpeed : SpeedValue;
		in deltaT : TimeValue;
		in force : ForceValue;

		force * deltaT == mass * (finalSpeed - initialSpeed) and
		mass > 0[kg]
	}
	
}
```

## Elements

- [AssertConstraintUsage](../metamodel/elements/AssertConstraintUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintDefinition](../metamodel/elements/ConstraintDefinition.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
