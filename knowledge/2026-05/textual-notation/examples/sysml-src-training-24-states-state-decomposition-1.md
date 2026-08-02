---
name: State Decomposition-1
kind: example
language: SysML
source: sysml/src/training/24. States/State Decomposition-1.sysml
elements: [AttributeDefinition, AttributeUsage, StateDefinition, StateUsage]
license: EPL-2.0
---

# State Decomposition-1

Verbatim SysML model from `sysml/src/training/24. States/State Decomposition-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'State Decomposition-1' {
	
	attribute def VehicleStartSignal;
	attribute def VehicleOnSignal;
	attribute def VehicleOffSignal;
	
	state def VehicleStates;
		
	state vehicleStates : VehicleStates {
		first start then off;
		
		state off;
		accept VehicleStartSignal 
			then starting;
			
		state starting;
		accept VehicleOnSignal
			then on;
			
		state on;
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
