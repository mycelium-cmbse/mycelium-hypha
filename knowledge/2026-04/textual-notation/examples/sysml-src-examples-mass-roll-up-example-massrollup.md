---
name: MassRollup
kind: example
language: SysML
source: sysml/src/examples/Mass Roll-up Example/MassRollup.sysml
elements: [AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# MassRollup

Verbatim SysML model from `sysml/src/examples/Mass Roll-up Example/MassRollup.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package MassRollup {
	private import NumericalFunctions::*;
	
	part def MassedThing {
		attribute mass :> ISQ::mass; 
		attribute totalMass :> ISQ::mass;
	}
	
	part simpleThing : MassedThing {
		attribute redefines totalMass = mass;
	}
	
	part compositeThing : MassedThing {
		part subcomponents: MassedThing[*];
		
		attribute redefines totalMass default
			mass + sum(subcomponents.totalMass); 
	}
	
	part filteredMassThing :> compositeThing {
		abstract attribute minMass :> ISQ::mass;
		
		attribute redefines totalMass =
			mass + sum(subcomponents.totalMass.?{in p :> ISQ::mass; p > minMass});
	}

}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
