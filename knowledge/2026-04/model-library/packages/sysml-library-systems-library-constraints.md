---
name: Constraints
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/Constraints.sysml
declares: [Constraints, Constraints::ConstraintCheck, Constraints::ConstraintCheck::self, Constraints::constraintChecks, Constraints::assertedConstraintChecks, Constraints::negatedConstraintChecks]
license: EPL-2.0
---

# Constraints

Verbatim SysML standard-library source from `sysml.library/Systems Library/Constraints.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package Constraints {
	doc
	/*
	 * This package defines the base types for constraints and related elements in the
	 * SysML language.
	 */

	private import Performances::BooleanEvaluation;
	private import Performances::booleanEvaluations;
	private import Performances::trueEvaluations;
	private import Performances::falseEvaluations;
	
	abstract constraint def ConstraintCheck :> BooleanEvaluation {
		doc
		/*
		 * ConstraintCheck is the most general class for constraint checking. ConstraintCheck is the base
		 * type of all ConstraintDefinitions.
		 */
	
		ref constraint self: ConstraintCheck :>> BooleanEvaluation::self;
	}
	
	abstract constraint constraintChecks: ConstraintCheck[0..*] nonunique :> booleanEvaluations {
		doc
		/*
		 * constraintChecks is the base feature of all ConstraintUsages.
		 */
	}
	
	abstract constraint assertedConstraintChecks :> constraintChecks, trueEvaluations {
		doc
		/*
		 * assertedConstraintChecks is the subset of constraintChecks for ConstraintChecks asserted to be true.
		 */
	}
		
	abstract constraint negatedConstraintChecks :> constraintChecks, falseEvaluations {
		doc
		/*
		 * negatedConstraintChecks is the subset of constraintChecks for ConstraintChecks asserted to be false.
		 */
	}
		
}
```

## Declarations

- `Constraints` — standard library package
- `Constraints::ConstraintCheck` — constraint def
- `Constraints::ConstraintCheck::self` — constraint
- `Constraints::constraintChecks` — constraint
- `Constraints::assertedConstraintChecks` — constraint
- `Constraints::negatedConstraintChecks` — constraint
