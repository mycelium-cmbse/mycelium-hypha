---
name: VehicleRequirementDerivation
kind: example
language: SysML
source: sysml/src/examples/Requirements Examples/VehicleRequirementDerivation.sysml
elements: [AttributeUsage, ConnectionUsage, ConstraintUsage, PartUsage, RequirementDefinition, RequirementUsage]
license: EPL-2.0
---

# VehicleRequirementDerivation

Verbatim SysML model from `sysml/src/examples/Requirements Examples/VehicleRequirementDerivation.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package VehicleRequirementDerivation {
	private import RequirementDerivation::*;
	
	part vehicle {
		attribute mass :> ISQ::mass;
		
		part chassis {
			attribute mass :> ISQ::mass;
		}
		
		part engine {
			attribute mass :> ISQ::mass;
		}
	}
	
	requirement def MassRequirement {
		subject mass :> ISQ::mass;
		attribute massLimit :> ISQ::mass;
		require constraint { mass <= massLimit }
	}
	
	requirement vehicleMassRequirement : MassRequirement {
		subject :>> mass = vehicle.mass;
	}
	
	requirement chassisMassRequirement : MassRequirement {
		subject :>> mass = vehicle.chassis.mass;
	}
	
	requirement engineMassRequirement : MassRequirement {
		subject :>> mass = vehicle.engine.mass;
	}
	
	#derivation connection {
		end #original ::> vehicleMassRequirement;
		end #derive ::> chassisMassRequirement;
		end #derive ::> engineMassRequirement;
	}
	
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConnectionUsage](../metamodel/elements/ConnectionUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [RequirementDefinition](../metamodel/elements/RequirementDefinition.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
