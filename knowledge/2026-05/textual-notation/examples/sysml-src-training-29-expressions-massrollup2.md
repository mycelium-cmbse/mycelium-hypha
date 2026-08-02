---
name: MassRollup2
kind: example
language: SysML
source: sysml/src/training/29. Expressions/MassRollup2.sysml
elements: [AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# MassRollup2

Verbatim SysML model from `sysml/src/training/29. Expressions/MassRollup2.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package MassRollup2 {
	private import NumericalFunctions::*;
	
	part def MassedThing {
		attribute simpleMass :> ISQ::mass; 
		attribute totalMass :> ISQ::mass default simpleMass;
	}
	
	part compositeThing : MassedThing {
		part subcomponents: MassedThing[*];		
		attribute :>> totalMass default
			simpleMass + sum(subcomponents.totalMass); 
	}
	
	part filteredMassThing :> compositeThing {
		attribute minMass :> ISQ::mass;		
		attribute :>> totalMass =
			simpleMass + sum(subcomponents.totalMass.?{in p:>ISQ::mass; p >= minMass});
	}

}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
