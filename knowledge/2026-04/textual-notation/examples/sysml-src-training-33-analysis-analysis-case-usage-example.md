---
name: Analysis Case Usage Example
kind: example
language: SysML
source: sysml/src/training/33. Analysis/Analysis Case Usage Example.sysml
elements: [AttributeUsage, PartUsage, RequirementUsage]
license: EPL-2.0
---

# Analysis Case Usage Example

Verbatim SysML model from `sysml/src/training/33. Analysis/Analysis Case Usage Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Analysis Case Usage Example' {
	private import 'Analysis Case Definition Example'::*;
	
	part vehicleFuelEconomyAnalysisContext {
		requirement vehicleFuelEconomyRequirements {
			subject vehicle : Vehicle;
			// ...
		}
		
		attribute cityScenario : WayPoint[*] = ( //* ... */ );
		attribute highwayScenario : WayPoint[*] = ( //* ... */ );
		
		analysis cityAnalysis : FuelEconomyAnalysis {
			subject vehicle = vehicle_c1;
			in scenario = cityScenario;
		}
		
		analysis highwayAnalysis : FuelEconomyAnalysis {
			subject vehicle = vehicle_c1;
			in scenario = highwayScenario;
		}
		
		part vehicle_c1 : Vehicle {
			// ...
			
			attribute :>> fuelEconomy_city = cityAnalysis.fuelEconomyResult;
			attribute :>> fuelEconomy_highway = highwayAnalysis.fuelEconomyResult;
		}
		
		satisfy vehicleFuelEconomyRequirements by vehicle_c1;
	}

}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
