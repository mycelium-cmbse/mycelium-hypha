---
name: HSUVRequirements
kind: example
language: SysML
source: sysml/src/examples/Requirements Examples/HSUVRequirements.sysml
elements: [ReferenceUsage, RequirementUsage]
license: EPL-2.0
---

# HSUVRequirements

Verbatim SysML model from `sysml/src/examples/Requirements Examples/HSUVRequirements.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package HSUVRequirements {
	private import Requirements::*;
	
	requirement <'UR1.1'> Load: FunctionalRequirementCheck {
		// The following requirements are composite sub-requirements.
		requirement Passengers;
		requirement FuelCapacity;
		requirement Cargo;
	}
	
	requirement <'UR1.2'> EcoFriendliness: PerformanceRequirementCheck {
		requirement <'URI1.2.1'> Emissions: PerformanceRequirementCheck {
			/* The car shall meet 2010 Kyoto Accord emissions standards. */
		}
	}
	
	requirement <'UR1.3'> Performance: PerformanceRequirementCheck {
		requirement Acceleration;
		requirement <'UR1.3.1'> FuelEconomy: PerformanceRequirementCheck {
			/* User shall obtain fuel economy better than that provided by
			 * 95% of cars built in 2004.
			 */
		}
		requirement Braking;
		requirement Range;
		requirement Power;
	}
	
	requirement <'UR1.4'> Ergonomics;
	
	// Syntactically, should this be explicitly marked as a "group"?
	requirement HybridSUVSpec {		
		// The following requirements are required by reference.
		require Load;
		require EcoFriendliness;
		require Performance;
		require Ergonomics;
	}
}
```

## Elements

- [ReferenceUsage](../metamodel/elements/ReferenceUsage.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
