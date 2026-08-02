---
name: State Definition Example-1
kind: example
language: SysML
source: sysml/src/training/23. State Definitions/State Definition Example-1.sysml
elements: [AttributeDefinition, AttributeUsage, StateDefinition, StateUsage, TransitionUsage]
license: EPL-2.0
---

# State Definition Example-1

Verbatim SysML model from `sysml/src/training/23. State Definitions/State Definition Example-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'State Definition Example-1' {
	
	attribute def VehicleStartSignal;
	attribute def VehicleOnSignal;
	attribute def VehicleOffSignal;
		
	state def VehicleStates {
		first start then off;
		
		state off;
		
		transition off_to_starting
			first off
			accept VehicleStartSignal 
			then starting;
			
		state starting;
		
		transition starting_to_on
			first starting
			accept VehicleOnSignal
			then on;
			
		state on;
		
		transition on_to_off
			first on
			accept VehicleOffSignal
			then off;
	}
	
}
```

## Elements

- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [StateDefinition](../metamodel/elements/StateDefinition.md)
- [StateUsage](../metamodel/elements/StateUsage.md)
- [TransitionUsage](../metamodel/elements/TransitionUsage.md)
