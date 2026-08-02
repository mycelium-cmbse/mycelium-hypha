---
name: Requirement Groups
kind: example
language: SysML
source: sysml/src/training/32. Requirements/Requirement Groups.sysml
elements: [ActionUsage, PartDefinition, PartUsage, PerformActionUsage, PortUsage, RequirementUsage]
license: EPL-2.0
---

# Requirement Groups

Verbatim SysML model from `sysml/src/training/32. Requirements/Requirement Groups.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Requirement Groups' {
	private import 'Requirement Definitions'::*;
	private import 'Requirement Usages'::*;
	
	part def Engine {
		port clutchPort: ClutchPort;
		perform action generateTorque: GenerateTorque;
	}
	
	requirement vehicleSpecification {
		doc /* Overall vehicle requirements group */
		
		subject vehicle : Vehicle;
		
		require fullVehicleMassLimit;
		require emptyVehicleMassLimit;
	}
	
	requirement engineSpecification {
		doc /* Engine power requirements group */
		
		subject engine : Engine;
		
		requirement drivePowerInterface : DrivePowerInterface {
			subject = engine.clutchPort;
		}
		
		requirement torqueGeneration : TorqueGeneration {
			subject = engine.generateTorque;	
		}
	}
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PerformActionUsage](../metamodel/elements/PerformActionUsage.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
