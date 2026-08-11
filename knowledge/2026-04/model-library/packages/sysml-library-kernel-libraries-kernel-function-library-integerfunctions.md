---
name: IntegerFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/IntegerFunctions.kerml
declares: [IntegerFunctions, IntegerFunctions::abs, IntegerFunctions::+, IntegerFunctions::-, IntegerFunctions::*, IntegerFunctions::/, IntegerFunctions::**, IntegerFunctions::^, IntegerFunctions::%, IntegerFunctions::<, IntegerFunctions::>, IntegerFunctions::<=, IntegerFunctions::>=, IntegerFunctions::max, IntegerFunctions::min, IntegerFunctions::==, IntegerFunctions::.., IntegerFunctions::ToString, IntegerFunctions::ToNatural, IntegerFunctions::ToInteger, IntegerFunctions::sum, IntegerFunctions::product]
license: EPL-2.0
---

# IntegerFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/IntegerFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package IntegerFunctions {
	doc
	/*
	 * This package defines functions on Integer values, including concrete specializations of the 
	 * general arithmetic and comparison operations.
	 */

	public import ScalarValues::*;
	
	function abs specializes RationalFunctions::abs { in x: Integer[1]; return : Natural[1]; }
	
	function '+' specializes RationalFunctions::'+' { in x: Integer[1]; in y: Integer[0..1]; return : Integer[1]; }
	function '-' specializes RationalFunctions::'-' { in x: Integer[1]; in y: Integer[0..1]; return : Integer[1]; }
	function '*' specializes RationalFunctions::'*' { in x: Integer[1]; in y: Integer[1]; return : Integer[1]; }
	function '/' specializes RationalFunctions::'/' { in x: Integer[1]; in y: Integer[1]; return : Rational[1]; }
	function '**' specializes RationalFunctions::'**' { in x: Integer[1]; in y: Natural[1]; return : Integer[1]; }
	function '^' specializes RationalFunctions::'^' { in x: Integer[1]; in y: Natural[1]; return : Integer[1]; }
	function '%' specializes NumericalFunctions::'%' { in x: Integer[1]; in y: Integer[1]; return : Integer[1]; }
	
	function '<' specializes RationalFunctions::'<' { in x: Integer[1]; in y: Integer[1]; return : Boolean[1]; }
	function '>' specializes RationalFunctions::'>' { in x: Integer[1]; in y: Integer[1]; return : Boolean[1]; }
	function '<=' specializes RationalFunctions::'<=' { in x: Integer[1]; in y: Integer[1]; return : Boolean[1]; }
	function '>=' specializes RationalFunctions::'>=' { in x: Integer[1]; in y: Integer[1]; return : Boolean[1]; }

	function max specializes RationalFunctions::max { in x: Integer[1]; in y: Integer[1]; return : Integer[1]; }
	function min specializes RationalFunctions::min { in x: Integer[1]; in y: Integer[1]; return : Integer[1]; }

	function '==' specializes DataFunctions::'==' { in x: Integer[0..1]; in y: Integer[0..1]; return : Boolean[1]; }
	
	function '..' specializes ScalarFunctions::'..' { in lower: Integer[1]; in upper: Integer[1]; return : Integer[0..*]; }
	
	function ToString specializes RationalFunctions::ToString { in x: Integer[1]; return : String[1]; }
	function ToNatural { in x: Integer[1]; return : Natural[1]; }
	function ToInteger { in x: String[1]; return : Integer[1]; }
	
	function sum specializes RationalFunctions::sum { in collection: Integer[0..*]; 
		return : Integer[1] default NumericalFunctions::sum0(collection, 0);
	}
	
	function product specializes RationalFunctions::product { in collection: Integer[0..*];
		return : Integer[1] default NumericalFunctions::product1(collection, 1);
	}
}	
```

## Declarations

- `IntegerFunctions` — standard library package
- `IntegerFunctions::abs` — function
- `IntegerFunctions::+` — function
- `IntegerFunctions::-` — function
- `IntegerFunctions::*` — function
- `IntegerFunctions::/` — function
- `IntegerFunctions::**` — function
- `IntegerFunctions::^` — function
- `IntegerFunctions::%` — function
- `IntegerFunctions::<` — function
- `IntegerFunctions::>` — function
- `IntegerFunctions::<=` — function
- `IntegerFunctions::>=` — function
- `IntegerFunctions::max` — function
- `IntegerFunctions::min` — function
- `IntegerFunctions::==` — function
- `IntegerFunctions::..` — function
- `IntegerFunctions::ToString` — function
- `IntegerFunctions::ToNatural` — function
- `IntegerFunctions::ToInteger` — function
- `IntegerFunctions::sum` — function
- `IntegerFunctions::product` — function
