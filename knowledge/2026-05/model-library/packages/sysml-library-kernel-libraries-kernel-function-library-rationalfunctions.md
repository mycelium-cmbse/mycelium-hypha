---
name: RationalFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/RationalFunctions.kerml
declares: [RationalFunctions, RationalFunctions::rat, RationalFunctions::numer, RationalFunctions::denom, RationalFunctions::abs, RationalFunctions::+, RationalFunctions::-, RationalFunctions::*, RationalFunctions::/, RationalFunctions::**, RationalFunctions::^, RationalFunctions::<, RationalFunctions::>, RationalFunctions::<=, RationalFunctions::>=, RationalFunctions::max, RationalFunctions::min, RationalFunctions::==, RationalFunctions::gcd, RationalFunctions::floor, RationalFunctions::round, RationalFunctions::ToString, RationalFunctions::ToInteger, RationalFunctions::ToRational, RationalFunctions::sum, RationalFunctions::product]
license: EPL-2.0
---

# RationalFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/RationalFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package RationalFunctions {
	doc
	/*
	 * This package defines Functions on Rational values, including concrete specializations of the 
	 * general arithmetic and comparison operations.
	 */

	public import ScalarValues::*;
	
	function rat { in numer: Integer[1]; in denum: Integer[1]; return : Rational[1]; }
	function numer { in rat: Rational[1]; return : Integer[1]; }
	function denom { in rat: Rational[1]; return : Integer[1]; }
	
	function abs specializes RealFunctions::abs { in x: Rational[1]; return : Rational[1]; }

	function '+' specializes RealFunctions::'+' { in x: Rational[1]; in y: Rational[0..1]; return : Rational[1]; }
	function '-' specializes RealFunctions::'-' { in x: Rational[1]; in y: Rational[0..1]; return : Rational[1]; }
	function '*' specializes RealFunctions::'*' { in x: Rational[1]; in y: Rational[1]; return : Rational[1]; }
	function '/' specializes RealFunctions::'/' { in x: Rational[1]; in y: Rational[1]; return : Rational[1]; }
	function '**' specializes RealFunctions::'**' { in x: Rational[1]; in y: Rational[1]; return : Rational[1]; }
	function '^' specializes RealFunctions::'^' { in x: Rational[1]; in y: Rational[1]; return : Rational[1]; }
	
	function '<' specializes RealFunctions::'<' { in x: Rational[1]; in y: Rational[1]; return : Boolean[1]; }
	function '>' specializes RealFunctions::'>' { in x: Rational[1]; in y: Rational[1]; return : Boolean[1]; }
	function '<=' specializes RealFunctions::'<=' { in x: Rational[1]; in y: Rational[1]; return : Boolean[1]; }
	function '>=' specializes RealFunctions::'>=' { in x: Rational[1]; in y: Rational[1]; return : Boolean[1]; }

	function max specializes RealFunctions::max { in x: Rational[1]; in y: Rational[1]; return : Rational[1]; }
	function min specializes RealFunctions::min { in x: Rational[1]; in y: Rational[1]; return : Rational[1]; }

	function '==' specializes RealFunctions::'==' { in x: Rational[0..1]; in y: Rational[0..1]; return : Boolean[1]; }
	
	function gcd{ in x: Rational[1]; in y: Rational[1]; return : Integer[1]; }
		
	function floor specializes RealFunctions::floor { in x: Rational[1]; return : Integer[1]; }
	function round specializes RealFunctions::round { in x: Rational[1]; return : Integer[1]; }
	
	function ToString specializes RealFunctions::ToString { in x: Rational[1]; return : String[1]; }
	function ToInteger{ in x: Rational[1]; return : Integer[1]; }
	function ToRational{ in x: String[1]; return : Rational[1]; }
	
	function sum specializes RealFunctions::sum { in collection: Rational[0..*];
		return : Rational[1] default NumericalFunctions::sum0(collection, rat(0, 1));
	}
	
	function product specializes RealFunctions::product { in collection: Rational[0..*];
		return : Rational[1] default NumericalFunctions::product1(collection, rat(1, 1));
	}	
}
```

## Declarations

- `RationalFunctions` — standard library package
- `RationalFunctions::rat` — function
- `RationalFunctions::numer` — function
- `RationalFunctions::denom` — function
- `RationalFunctions::abs` — function
- `RationalFunctions::+` — function
- `RationalFunctions::-` — function
- `RationalFunctions::*` — function
- `RationalFunctions::/` — function
- `RationalFunctions::**` — function
- `RationalFunctions::^` — function
- `RationalFunctions::<` — function
- `RationalFunctions::>` — function
- `RationalFunctions::<=` — function
- `RationalFunctions::>=` — function
- `RationalFunctions::max` — function
- `RationalFunctions::min` — function
- `RationalFunctions::==` — function
- `RationalFunctions::gcd` — function
- `RationalFunctions::floor` — function
- `RationalFunctions::round` — function
- `RationalFunctions::ToString` — function
- `RationalFunctions::ToInteger` — function
- `RationalFunctions::ToRational` — function
- `RationalFunctions::sum` — function
- `RationalFunctions::product` — function
