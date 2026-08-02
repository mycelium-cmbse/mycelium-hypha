---
name: JohnIndividualExample
kind: example
language: SysML
source: sysml/src/examples/Individuals Examples/JohnIndividualExample.sysml
elements: [AssertConstraintUsage, AttributeUsage, ConstraintUsage, ItemDefinition, ItemUsage]
license: EPL-2.0
---

# JohnIndividualExample

Verbatim SysML model from `sysml/src/examples/Individuals Examples/JohnIndividualExample.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package JohnIndividualExample {
	
	item def Person {
		doc	
		/*
		 * This is the definition of the class of persons, each of whom has an age.
		 */

		attribute age : ScalarValues::Natural;		
		
		timeslice asPresident : Person [0..*] {
			doc
			/*
			 * These are the periods during which a Person is president.
			 */
		}
	}
	
	individual item def John :> Person {
		doc
		/*
		 * This the definition of the individual Person who is John.
		 * There is at most one such person.
		 */
	}
	
	item def Country {
		doc
		/*
		 * This is the definition of the class of countries, each of which may have 
		 * at most one president (at any point in time).
		 */
		ref presidentOfCountry[0..1] : Person :> presidentOfCountry.asPresident;
	}
	
	individual item def UnitedStates :> Country {
		doc
		/*
		 * This is the definition of the individual country that is the
		 * United States. It contains a single instance. The United States
		 * always has a president who must be at least 35 years old.
		 */
		 
		ref presidentOfUS[1] :>> presidentOfCountry {
	   		assert constraint { age >= 35 } 
	  	}
	}
	
	individual UnitedStatesWithJohnAsPresident : UnitedStates {
    	timeslice item UnitedStatesWhenJohnIsPresident[*] : UnitedStates {
    		doc
    		/*
    		 * These are the time slices of the United States during
    		 * which John is president of the United States.
    		 */
    		ref :>> presidentOfUS : John;
    	}
	}
}
```

## Elements

- [AssertConstraintUsage](../metamodel/elements/AssertConstraintUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
