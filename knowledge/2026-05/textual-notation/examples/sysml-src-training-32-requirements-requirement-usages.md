---
name: Requirement Usages
kind: example
language: SysML
source: sysml/src/training/32. Requirements/Requirement Usages.sysml
elements: [AttributeUsage, ConstraintUsage, RequirementUsage]
license: EPL-2.0
---

# Requirement Usages

Verbatim SysML model from `sysml/src/training/32. Requirements/Requirement Usages.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Requirement Usages' {
	private import SI::*;
	private import 'Requirement Definitions'::*;
	
	requirement <'1.1'> fullVehicleMassLimit : VehicleMassLimitationRequirement {
		subject vehicle : Vehicle;
		attribute :>> massReqd = 2000[kg];
		
		assume constraint {
			doc /* Full tank is full. */
			vehicle.fuelMass == vehicle.fuelFullMass
		}
	}
	
	requirement <'1.2'> emptyVehicleMassLimit : VehicleMassLimitationRequirement {
		subject vehicle : Vehicle;
		attribute :>> massReqd = 1500[kg];
		
		assume constraint {
			doc /* Full tank is empty. */
			vehicle.fuelMass == 0[kg]
		}
	}
	
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
