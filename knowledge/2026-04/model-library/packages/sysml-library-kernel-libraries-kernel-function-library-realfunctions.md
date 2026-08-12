---
name: RealFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/RealFunctions.kerml
declares: [RealFunctions, RealFunctions::re, RealFunctions::im, RealFunctions::abs, RealFunctions::arg, RealFunctions::+, RealFunctions::-, RealFunctions::*, RealFunctions::/, RealFunctions::**, RealFunctions::^, RealFunctions::<, RealFunctions::>, RealFunctions::<=, RealFunctions::>=, RealFunctions::max, RealFunctions::min, RealFunctions::==, RealFunctions::sqrt, RealFunctions::floor, RealFunctions::round, RealFunctions::ToString, RealFunctions::ToInteger, RealFunctions::ToRational, RealFunctions::ToReal, RealFunctions::sum, RealFunctions::product]
license: EPL-2.0
---

# RealFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/RealFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package RealFunctions {
	doc
	/*
	 * This package defines Functions on Real values, including concrete specializations of the 
	 * general arithmetic and comparison operations.
	 */

	public import ScalarValues::*;
	
	function re :> ComplexFunctions::re{ in x: Real[1]; 
        return : Real[1] = x;
	}
	function im :> ComplexFunctions::im{ in x: Real[1]; 
        return : Real[1] = 0.0;
	}
	
	function abs specializes ComplexFunctions::abs { in x: Real[1]; return : Real[1]; }
	function arg specializes ComplexFunctions::arg { in x: Real[1]; 
        return : Real[1] = 0.0;
	}

	function '+' specializes ComplexFunctions::'+' { in x: Real[1]; in y: Real[0..1]; return : Real[1]; }
	function '-' specializes ComplexFunctions::'-' { in x: Real[1]; in y: Real[0..1]; return : Real[1]; }
	function '*' specializes ComplexFunctions::'*' { in x: Real[1]; in y: Real[1]; return : Real[1]; }
	function '/' specializes ComplexFunctions::'/' { in x: Real[1]; in y: Real[1]; return : Real[1]; }
	function '**' specializes ComplexFunctions::'**' { in x: Real[1]; in y: Real[1]; return : Real[1]; }
	function '^' specializes ComplexFunctions::'^' { in x: Real[1]; in y: Real[1]; return : Real[1]; }
	
	function '<' specializes NumericalFunctions::'<' { in x: Real[1]; in y: Real[1]; return : Boolean[1]; }
	function '>' specializes NumericalFunctions::'>' { in x: Real[1]; in y: Real[1]; return : Boolean[1]; }
	function '<=' specializes NumericalFunctions::'<=' { in x: Real[1]; in y: Real[1]; return : Boolean[1]; }
	function '>=' specializes NumericalFunctions::'>=' { in x: Real[1]; in y: Real[1]; return : Boolean[1]; }

	function max specializes NumericalFunctions::max { in x: Real[1]; in y: Real[1]; return : Real[1]; }
	function min specializes NumericalFunctions::min { in x: Real[1]; in y: Real[1]; return : Real[1]; }

	function '==' specializes ComplexFunctions::'==' { in x: Real[0..1]; in y: Real[0..1]; return : Boolean[1]; }
			
	function sqrt{ in x: Real[1]; return : Real[1]; }

	function floor{ in x: Real[1]; return : Integer[1]; }
	function round{ in x: Real[1]; return : Integer[1]; }
	
	function ToString specializes ComplexFunctions::ToString { in x: Real[1]; return : String[1]; }
	function ToInteger{ in x: Real[1]; return : Integer[1]; }
	function ToRational{ in x: Real[1]; return : Rational[1]; }
	function ToReal{ in x: String[1]; return : Real[1]; }
	
	function sum specializes ComplexFunctions::sum { in collection: Real[0..*]; 
        return : Real default NumericalFunctions::sum0(collection, 0.0);
	}
	
	function product specializes ComplexFunctions::product { in collection: Real[0..*]; 
        return : Real default NumericalFunctions::product1(collection, 1.0);
	}	
}
```

## Declarations

- `RealFunctions` — standard library package
- `RealFunctions::re` — function
- `RealFunctions::im` — function
- `RealFunctions::abs` — function
- `RealFunctions::arg` — function
- `RealFunctions::+` — function
- `RealFunctions::-` — function
- `RealFunctions::*` — function
- `RealFunctions::/` — function
- `RealFunctions::**` — function
- `RealFunctions::^` — function
- `RealFunctions::<` — function
- `RealFunctions::>` — function
- `RealFunctions::<=` — function
- `RealFunctions::>=` — function
- `RealFunctions::max` — function
- `RealFunctions::min` — function
- `RealFunctions::==` — function
- `RealFunctions::sqrt` — function
- `RealFunctions::floor` — function
- `RealFunctions::round` — function
- `RealFunctions::ToString` — function
- `RealFunctions::ToInteger` — function
- `RealFunctions::ToRational` — function
- `RealFunctions::ToReal` — function
- `RealFunctions::sum` — function
- `RealFunctions::product` — function
