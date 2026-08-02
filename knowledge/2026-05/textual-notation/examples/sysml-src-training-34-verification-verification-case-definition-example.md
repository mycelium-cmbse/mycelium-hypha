---
name: Verification Case Definition Example
kind: example
language: SysML
source: sysml/src/training/34. Verification/Verification Case Definition Example.sysml
elements: [ActionUsage, AttributeUsage, ConstraintUsage, PartDefinition, PartUsage, RequirementUsage]
license: EPL-2.0
---

# Verification Case Definition Example

Verbatim SysML model from `sysml/src/training/34. Verification/Verification Case Definition Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Verification Case Definition Example' {
	
	part def Vehicle {
		attribute mass :> ISQ::mass;
	}
	
	requirement vehicleMassRequirement {
		subject vehicle : Vehicle;
		in massActual :> ISQ::mass;
		doc /* The vehicle mass shall be less than or equal to 2500 kg. */
		
		require constraint { 
		    massActual == vehicle.mass and 
		    massActual <= 2500[SI::kg]
		}
	}
		
	verification def VehicleMassTest {
		private import VerificationCases::*;

		subject testVehicle : Vehicle;
		objective vehicleMassVerificationObjective {
			// The subject of the verify is automatically bound to 'testVehicle' here.
			verify vehicleMassRequirement;
		}
		
		action collectData {
			in part testVehicle : Vehicle = VehicleMassTest::testVehicle;
			out massMeasured :> ISQ::mass;
		}
		
		action processData {
			in massMeasured :> ISQ::mass = collectData.massMeasured;
			out massProcessed :> ISQ::mass;
		}
		
		action evaluateData {
			in massProcessed :> ISQ::mass = processData.massProcessed;
			out verdict : VerdictKind = 
				// Check that 'testVehicle' statisfies 'vehicleMassRequirement' if its mass equals 'massProcessed'.
				PassIf(vehicleMassRequirement(vehicle = testVehicle, massActual = massProcessed));
		}
		
		return verdict : VerdictKind = evaluateData.verdict;
	}
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
