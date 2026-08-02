---
name: Constraint Assertions-2
kind: example
language: SysML
source: sysml/src/training/31. Constraints/Constraint Assertions-2.sysml
elements: [AttributeUsage, ConstraintDefinition, ConstraintUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Constraint Assertions-2

Verbatim SysML model from `sysml/src/training/31. Constraints/Constraint Assertions-2.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Constraint Assertions-2' {
	private import ISQ::*;
	private import SI::*;
	private import NumericalFunctions::*;
	
	part def Engine;
	part def Transmission;
	
	constraint def MassConstraint {
		in partMasses : MassValue[0..*];
		in massLimit : MassValue;
	}
	
	constraint massConstraint : MassConstraint {
		in partMasses : MassValue[0..*];
		in massLimit : MassValue;
			
		sum(partMasses) <= massLimit
	}
	
	part def Vehicle {
		assert massConstraint {
			in partMasses = (chassisMass, engine.mass, transmission.mass);
			in massLimit = 2500[kg];
		}
		
		attribute chassisMass : MassValue;
		
		part engine : Engine {
			attribute mass : MassValue;
		}
		
		part transmission : Engine {
			attribute mass : MassValue;
		}
	}	
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintDefinition](../metamodel/elements/ConstraintDefinition.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
