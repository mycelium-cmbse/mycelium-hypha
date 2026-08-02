---
name: 15_04-Logical Expressions
kind: example
language: SysML
source: sysml/src/validation/15-Properties-Values-Expressions/15_04-Logical Expressions.sysml
elements: [AssertConstraintUsage, AttributeUsage, ConstraintUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# 15_04-Logical Expressions

Verbatim SysML model from `sysml/src/validation/15-Properties-Values-Expressions/15_04-Logical Expressions.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '15_04-Logical Expressions' {
	private import ScalarValues::*;
	
	part def Engine;
	part def '4CylEngine' :> Engine;
	part def '6CylEngine' :> Engine;
	
	part def Transmission;
	part def ManualTransmission :> Transmission;
	part def AutomaticTransmission :> Transmission;
	
	part def Vehicle {
		attribute isHighPerformance: Boolean;
		
		part engine: Engine[1];
		part transmission: Transmission[1];
		
		assert constraint {
			if isHighPerformance? engine istype '6CylEngine'
			else engine istype '4CylEngine'
		}
		
		assert constraint {
			(engine istype '4CylEngine' and 
			 transmission istype ManualTransmission) xor
			(engine istype '6CylEngine' and
			 transmission istype AutomaticTransmission)
		}
	}
}
```

## Elements

- [AssertConstraintUsage](../metamodel/elements/AssertConstraintUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
