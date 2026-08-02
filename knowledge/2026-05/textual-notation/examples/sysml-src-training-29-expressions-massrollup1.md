---
name: MassRollup1
kind: example
language: SysML
source: sysml/src/training/29. Expressions/MassRollup1.sysml
elements: [AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# MassRollup1

Verbatim SysML model from `sysml/src/training/29. Expressions/MassRollup1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package MassRollup1 {
	private import NumericalFunctions::*;
	
	part def MassedThing {
		attribute simpleMass :> ISQ::mass; 
		attribute totalMass :> ISQ::mass;
	}
	
	part simpleThing : MassedThing {
		attribute :>> totalMass = simpleMass;
	}
	
	part compositeThing : MassedThing {
		part subcomponents: MassedThing[*];		
		attribute :>> totalMass =
			simpleMass + sum(subcomponents.totalMass); 
	}
	
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
