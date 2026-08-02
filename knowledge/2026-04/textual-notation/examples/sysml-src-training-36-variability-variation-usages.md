---
name: Variation Usages
kind: example
language: SysML
source: sysml/src/training/36. Variability/Variation Usages.sysml
elements: [AssertConstraintUsage, ConstraintUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Variation Usages

Verbatim SysML model from `sysml/src/training/36. Variability/Variation Usages.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Variation Usages' {
	private import 'Variation Definitions'::*;
	
	part def Vehicle;
	part def Transmission;
	part manualTransmission;
	part automaticTransmission;
	
	abstract part vehicleFamily : Vehicle {
		part engine : EngineChoices[1];
		
		variation part transmission : Transmission[1] {
			variant manualTransmission;
			variant automaticTransmission;
		}
		
		assert constraint {
			(engine == engine::'4cylEngine' and
			 transmission == transmission::manualTransmission) xor
			(engine == engine::'6cylEngine' and 
			 transmission == transmission::automaticTransmission)
		}	
	}
	
}
```

## Elements

- [AssertConstraintUsage](../metamodel/elements/AssertConstraintUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
