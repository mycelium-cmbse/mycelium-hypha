---
name: TradeStudyTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/TradeStudyTest.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# TradeStudyTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/TradeStudyTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package TradeStudyTest {
	private import ScalarValues::Real;
	private import TradeStudies::*;
	
	part def Engine;
	part engine1: Engine;
	part engine2: Engine;
	
	analysis engineTradeStudy : TradeStudy {
		subject : Engine[1..*] = (engine1, engine2);
		objective : MaximizeObjective;

		calc :>> evaluationFunction {
			in part : Engine;
			return : Real;
		}
		
		return part : Engine;
	}
	
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
