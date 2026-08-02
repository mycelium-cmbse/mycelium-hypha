---
name: Car Mass Rollup Example 1
kind: example
language: SysML
source: sysml/src/training/29. Expressions/Car Mass Rollup Example 1.sysml
elements: [AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Car Mass Rollup Example 1

Verbatim SysML model from `sysml/src/training/29. Expressions/Car Mass Rollup Example 1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Car Mass Rollup Example 1' {
	private import ScalarValues::*;
	private import MassRollup1::*;
	
	part def CarPart :> MassedThing {			
		attribute serialNumber: String;
	}
	
	part car: CarPart :> compositeThing {	
		attribute vin :>> serialNumber;
		
		part carParts: CarPart[*] :>> subcomponents;
		
		part engine :> simpleThing, carParts {
			//...
		}
		
		part transmission :> simpleThing, carParts {
			//...
		}
	}

	// Example usage
	
	private import SI::kg;
	part c :> car {
		attribute :>> simpleMass = 1000[kg];
		part :>> engine {
			attribute :>> simpleMass = 100[kg];
		}
		
		part redefines transmission {
			attribute :>> simpleMass = 50[kg];
		}	
	}
	
	// c::totalMass --> 1150.0[kg]
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
