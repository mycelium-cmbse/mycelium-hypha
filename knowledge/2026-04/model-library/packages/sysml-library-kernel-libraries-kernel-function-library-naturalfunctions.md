---
name: NaturalFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/NaturalFunctions.kerml
declares: [NaturalFunctions, NaturalFunctions::+, NaturalFunctions::*, NaturalFunctions::/, NaturalFunctions::%, NaturalFunctions::<, NaturalFunctions::>, NaturalFunctions::<=, NaturalFunctions::>=, NaturalFunctions::max, NaturalFunctions::min, NaturalFunctions::==, NaturalFunctions::ToString, NaturalFunctions::ToNatural]
license: EPL-2.0
---

# NaturalFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/NaturalFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package NaturalFunctions {
	doc
	/*
	 * This package defines functions on Natural values, including concrete specialization of the 
	 * general arithmetic and comparison operations.
	 */

	public import ScalarValues::*;
	
	function '+' specializes IntegerFunctions::'+' { in x: Natural[1]; in y: Natural[0..1]; return : Natural[1]; }
	function '*' specializes IntegerFunctions::'*' { in x: Natural[1]; in y: Natural[1]; return : Natural[1]; }
	function '/' specializes IntegerFunctions::'/' { in x: Natural[1]; in y: Natural[1]; return : Natural[1]; }
	function '%' specializes IntegerFunctions::'%' { in x: Natural[1]; in y: Natural[1]; return : Natural[1]; }
	
	function '<' specializes IntegerFunctions::'<' { in x: Natural[1]; in y: Natural[1]; return : Boolean[1]; }
	function '>' specializes IntegerFunctions::'>' { in x: Natural[1]; in y: Natural[1]; return : Boolean[1]; }
	function '<=' specializes IntegerFunctions::'<=' { in x: Natural[1]; in y: Natural[1]; return : Boolean[1]; }
	function '>=' specializes IntegerFunctions::'>=' { in x: Natural[1]; in y: Natural[1]; return : Boolean[1]; }	

	function max specializes IntegerFunctions::max { in x: Natural[1]; in y: Natural[1]; return : Natural[1]; }
	function min specializes IntegerFunctions::min { in x: Natural[1]; in y: Natural[1]; return : Natural[1]; }

	function '==' specializes IntegerFunctions::'==' { in x: Natural[0..1]; in y: Natural[0..1]; return : Boolean[1]; }
	
	function ToString specializes IntegerFunctions::ToString { in x: Natural[1]; return : String[1]; }
	function ToNatural{ in x: String[1]; return : Natural[1]; }
}	
```

## Declarations

- `NaturalFunctions` — standard library package
- `NaturalFunctions::+` — function
- `NaturalFunctions::*` — function
- `NaturalFunctions::/` — function
- `NaturalFunctions::%` — function
- `NaturalFunctions::<` — function
- `NaturalFunctions::>` — function
- `NaturalFunctions::<=` — function
- `NaturalFunctions::>=` — function
- `NaturalFunctions::max` — function
- `NaturalFunctions::min` — function
- `NaturalFunctions::==` — function
- `NaturalFunctions::ToString` — function
- `NaturalFunctions::ToNatural` — function
