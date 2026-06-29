---
name: State definitions and transitions
kind: example
language: SysML
constructs: ["state def", state, transition, accept, "first ... then"]
elements: [StateDefinition, StateUsage, TransitionUsage, AcceptActionUsage]
source: "sources/textual/sysml/training/23. State Definitions/State Definition Example-1.sysml"
license: EPL-2.0
---

# State definitions and transitions

A worked SysML v2 textual-notation example: a state definition with states, an initial transition,
and signal-triggered transitions.

## Notation

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

## Constructs & elements

- `state def { … }` — [StateDefinition](../../sysml2/elements/StateDefinition.md): a definition of behavior as states and transitions.
- `state off` — [StateUsage](../../sysml2/elements/StateUsage.md): a state of the definition.
- `first start then off` — the initial succession into the first state.
- `transition … first … accept … then …` — [TransitionUsage](../../sysml2/elements/TransitionUsage.md): a triggered transition between states.
- `accept VehicleStartSignal` — an [AcceptActionUsage](../../sysml2/elements/AcceptActionUsage.md): the transition's trigger.

## Source

`sources/textual/sysml/training/23. State Definitions/State Definition Example-1.sysml` — from
Systems-Modeling/SysML-v2-Release (EPL-2.0); see [NOTICE](../../../NOTICE).
