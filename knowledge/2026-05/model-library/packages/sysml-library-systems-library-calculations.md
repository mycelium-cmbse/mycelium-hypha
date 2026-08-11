---
name: Calculations
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/Calculations.sysml
declares: [Calculations, Calculations::Calculation, Calculations::Calculation::self, Calculations::Calculation::subcalculations, Calculations::calculations]
license: EPL-2.0
---

# Calculations

Verbatim SysML standard-library source from `sysml.library/Systems Library/Calculations.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package Calculations {
	doc
	/*
	 * This package defines the base types for calculations and related behavioral elements in the
	 * SysML language.
	 */

	private import Performances::Evaluation;
	private import Performances::evaluations;
	private import Actions::Action;
	private import Actions::actions;
	
	abstract calc def Calculation :> Action, Evaluation {
		doc
		/*
		 * Calculation is the most general class of evaluations of CalculationDefinitions in a
		 * system or part of a system. Calculation is the base class of all CalculationDefinitions.
		 */
	
		ref calc self: Calculation :>> Action::self, Evaluation::self;
		
		abstract calc subcalculations: Calculation :> calculations, subactions {
			doc
			/*
			 * The subactions of this Calculation that are Calculations.
			 */
		}
		
	}
	
	abstract calc calculations: Calculation[0..*] nonunique :> actions, evaluations {
		doc
		/*
		 * calculations is the base Feature for all CalculationUsages.
		 */
	}
}
```

## Declarations

- `Calculations` — standard library package
- `Calculations::Calculation` — calc def
- `Calculations::Calculation::self` — calc
- `Calculations::Calculation::subcalculations` — calc
- `Calculations::calculations` — calc
