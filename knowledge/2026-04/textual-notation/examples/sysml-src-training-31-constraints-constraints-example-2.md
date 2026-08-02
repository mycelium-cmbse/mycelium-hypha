---
name: Constraints Example-2
kind: example
language: SysML
source: sysml/src/training/31. Constraints/Constraints Example-2.sysml
elements: [AttributeUsage, ConstraintDefinition, ConstraintUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Constraints Example-2

Verbatim SysML model from `sysml/src/training/31. Constraints/Constraints Example-2.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Constraints Example-2' {
	private import ISQ::*;
	private import SI::*;
	private import NumericalFunctions::*;
	
	part def Engine;
	part def Transmission;
	
	constraint def MassConstraint {
		attribute partMasses : MassValue[0..*];
		attribute massLimit : MassValue;
			
		sum(partMasses) <= massLimit
	}
	
	part def Vehicle {
		constraint massConstraint : MassConstraint {
			redefines partMasses = (chassisMass, engine.mass, transmission.mass);
			redefines massLimit = 2500[kg];
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
