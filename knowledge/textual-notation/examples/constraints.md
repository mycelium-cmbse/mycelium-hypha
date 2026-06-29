---
name: Constraints
kind: example
language: SysML
constructs: ["constraint def", constraint, parameters, "Boolean expression"]
elements: [ConstraintDefinition, ConstraintUsage, AttributeUsage, PartUsage]
source: "sources/textual/sysml/training/31. Constraints/Constraints Example-1.sysml"
license: EPL-2.0
---

# Constraints

A worked SysML v2 textual-notation example: a constraint definition with parameters and a Boolean
expression, asserted on a part with bound arguments.

## Notation

```sysml
package 'Constraints Example-1' {
	private import ISQ::*;
	private import SI::*;
	private import NumericalFunctions::*;
	
	part def Engine;
	part def Transmission;
	
	constraint def MassConstraint {
		in partMasses : MassValue[0..*];
		in massLimit : MassValue;
			
		sum(partMasses) <= massLimit
	}
	
	part def Vehicle {
		constraint massConstraint : MassConstraint {
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

## Constructs & elements

- `constraint def { in …; <expr> }` – [ConstraintDefinition](../../sysml2/elements/ConstraintDefinition.md): a Boolean-valued definition with parameters.
- `constraint massConstraint : MassConstraint { in … = … }` – [ConstraintUsage](../../sysml2/elements/ConstraintUsage.md): a constraint applied with bound arguments.
- `in partMasses : MassValue[0..*]` – directed parameter features ([AttributeUsage](../../sysml2/elements/AttributeUsage.md)) with multiplicity.
- `attribute x : T` – [AttributeUsage](../../sysml2/elements/AttributeUsage.md); `part x : T` – [PartUsage](../../sysml2/elements/PartUsage.md).
- `sum(partMasses) <= massLimit` – the constraint's Boolean expression.

## Source

`sources/textual/sysml/training/31. Constraints/Constraints Example-1.sysml` – from
Systems-Modeling/SysML-v2-Release (EPL-2.0); see [NOTICE](../../../NOTICE).
