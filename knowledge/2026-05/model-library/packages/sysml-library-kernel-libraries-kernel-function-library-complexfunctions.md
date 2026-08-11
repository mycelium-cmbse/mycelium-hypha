---
name: ComplexFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/ComplexFunctions.kerml
declares: [ComplexFunctions, ComplexFunctions::i, ComplexFunctions::rect, ComplexFunctions::polar, ComplexFunctions::re, ComplexFunctions::im, ComplexFunctions::isZero, ComplexFunctions::isUnit, ComplexFunctions::abs, ComplexFunctions::arg, ComplexFunctions::+, ComplexFunctions::-, ComplexFunctions::*, ComplexFunctions::/, ComplexFunctions::**, ComplexFunctions::^, ComplexFunctions::==, ComplexFunctions::ToString, ComplexFunctions::ToComplex, ComplexFunctions::sum, ComplexFunctions::product]
license: EPL-2.0
---

# ComplexFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/ComplexFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package ComplexFunctions {
	doc
	/*
	 * This package defines functions on Complex values, including concrete specializations of the 
	 * general arithmetic and comparison operations.
	 */

	public import ScalarValues::*;
		
	feature i: Complex[1] = rect(0.0, 1.0);
	
	function rect { in re: Real[1]; in im: Real[1]; return : Complex[1]; }
	function polar { in abs: Real[1]; in arg: Real[1]; return : Complex[1]; }
	
	function re { in x: Complex[1]; return : Real[1]; }
	function im { in x: Complex[1]; return : Real[1]; }
	
	function isZero specializes NumericalFunctions::isZero { in x : Complex[1];
		return : Boolean[1] = re(x) == 0.0 and im(x) == 0.0;
	}
	function isUnit specializes NumericalFunctions::isUnit { in x : Complex[1];
		return : Boolean[1] = re(x) == 1.0 and im(x) == 0.0;
	}
	
	function abs specializes NumericalFunctions::abs { in x: Complex[1]; return : Real[1]; }
	function arg { in x: Complex[1]; return : Real[1]; }
	
	function '+' specializes NumericalFunctions::'+' { in x: Complex[1]; in y: Complex[0..1]; return : Complex[1]; }
	function '-' specializes NumericalFunctions::'-' { in x: Complex[1]; in y: Complex[0..1]; return : Complex[1]; }
	function '*' specializes NumericalFunctions::'*' { in x: Complex[1]; in y: Complex[1]; return : Complex[1]; }
	function '/' specializes NumericalFunctions::'/' { in x: Complex[1]; in y: Complex[1]; return : Complex[1]; }
	function '**' specializes NumericalFunctions::'**' { in x: Complex[1]; in y: Complex[1]; return : Complex[1]; }
	function '^' specializes NumericalFunctions::'^' { in x: Complex[1]; in y: Complex[1]; return : Complex[1]; }
	
	function '==' specializes DataFunctions::'==' { in x: Complex[0..1]; in y: Complex[0..1]; return : Boolean[1]; }
	
	function ToString specializes BaseFunctions::ToString { in x: Complex[1]; return : String[1]; }
	function ToComplex { in x: String[1]; return : Complex[1]; }
	
	function sum specializes NumericalFunctions::sum { in collection: Complex[0..*];
		return : Complex[1] default NumericalFunctions::sum0(collection, rect(0.0, 0.0));
	}
	
	function product specializes NumericalFunctions::product { in collection: Complex[0..*];
		return : Complex[1] default NumericalFunctions::product1(collection, rect(1.0, 0.0));
	}	
}
```

## Declarations

- `ComplexFunctions` — standard library package
- `ComplexFunctions::i` — feature
- `ComplexFunctions::rect` — function
- `ComplexFunctions::polar` — function
- `ComplexFunctions::re` — function
- `ComplexFunctions::im` — function
- `ComplexFunctions::isZero` — function
- `ComplexFunctions::isUnit` — function
- `ComplexFunctions::abs` — function
- `ComplexFunctions::arg` — function
- `ComplexFunctions::+` — function
- `ComplexFunctions::-` — function
- `ComplexFunctions::*` — function
- `ComplexFunctions::/` — function
- `ComplexFunctions::**` — function
- `ComplexFunctions::^` — function
- `ComplexFunctions::==` — function
- `ComplexFunctions::ToString` — function
- `ComplexFunctions::ToComplex` — function
- `ComplexFunctions::sum` — function
- `ComplexFunctions::product` — function
