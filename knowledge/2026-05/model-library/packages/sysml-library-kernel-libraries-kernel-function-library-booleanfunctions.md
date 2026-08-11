---
name: BooleanFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/BooleanFunctions.kerml
declares: [BooleanFunctions, BooleanFunctions::not, BooleanFunctions::xor, BooleanFunctions::|, BooleanFunctions::&, BooleanFunctions::==, BooleanFunctions::ToString, BooleanFunctions::ToBoolean]
license: EPL-2.0
---

# BooleanFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/BooleanFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package BooleanFunctions {
	doc
	/*
	 * This package defines functions on Boolean values, including those corresponding to 
	 * (non-conditional) logical operators in the KerML expression notation.
	 */

	public import ScalarValues::*;
	
	function 'not' specializes ScalarFunctions::'not' { in x: Boolean[1]; return : Boolean[1]; }
	function 'xor' specializes ScalarFunctions::'xor' { in x: Boolean[1]; in y: Boolean[1]; return : Boolean[1]; }
	
	function '|' specializes ScalarFunctions::'|' { in x: Boolean[1]; in y: Boolean[1]; return : Boolean[1]; }
	function '&' specializes ScalarFunctions::'&' { in x: Boolean[1]; in y: Boolean[1]; return : Boolean[1]; }
	
	function '==' specializes DataFunctions::'==' { in x: Boolean[0..1]; in y: Boolean[0..1]; return : Boolean[1]; }
	
	function ToString specializes BaseFunctions::ToString { in x: Boolean[1]; return : String[1]; }
	function ToBoolean { in x: String[1]; return : Boolean[1]; }
	
}
	
```

## Declarations

- `BooleanFunctions` — standard library package
- `BooleanFunctions::not` — function
- `BooleanFunctions::xor` — function
- `BooleanFunctions::|` — function
- `BooleanFunctions::&` — function
- `BooleanFunctions::==` — function
- `BooleanFunctions::ToString` — function
- `BooleanFunctions::ToBoolean` — function
