---
name: Model Library Example
kind: example
language: SysML
source: sysml/src/training/41. Language Extension/Model Library Example.sysml
elements: [AttributeUsage, ConnectionDefinition, ConnectionUsage, ItemDefinition, ItemUsage, OccurrenceDefinition, OccurrenceUsage]
license: EPL-2.0
---

# Model Library Example

Verbatim SysML model from `sysml/src/training/41. Language Extension/Model Library Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
library package 'Model Library Example' {
	private import ScalarValues::Real;
	private import RiskMetadata::Level;
	
	abstract occurrence def Situation;
	
	abstract occurrence situations : Situation[*] nonunique;
	
	abstract occurrence def Cause {
		attribute probability : Real;
	}
	
	abstract occurrence causes : Cause[*] nonunique :> situations;
	
	abstract occurrence def Failure {
		attribute severity : Level;
	}
	
	abstract occurrence failures : Failure[*] nonunique :> situations;
	
	abstract connection def Causation :> Occurrences::HappensBefore {
		end [*] ref cause : Situation;
		end [*] ref effect : Situation;
	}
	
	abstract connection causations : Causation[*] nonunique;
	
	item def Scenario {
		occurrence :>> situations;
		occurrence :>> causes :> situations;
		occurrence :>> failures :> situations;
	}
	
	item scenarios : Scenario[*] nonunique;
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConnectionDefinition](../metamodel/elements/ConnectionDefinition.md)
- [ConnectionUsage](../metamodel/elements/ConnectionUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [OccurrenceDefinition](../metamodel/elements/OccurrenceDefinition.md)
- [OccurrenceUsage](../metamodel/elements/OccurrenceUsage.md)
