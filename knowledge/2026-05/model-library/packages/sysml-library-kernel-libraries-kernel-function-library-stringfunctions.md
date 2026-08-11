---
name: StringFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/StringFunctions.kerml
declares: [StringFunctions, StringFunctions::+, StringFunctions::Length, StringFunctions::Substring, StringFunctions::<, StringFunctions::>, StringFunctions::<=, StringFunctions::>=, StringFunctions::==, StringFunctions::ToString]
license: EPL-2.0
---

# StringFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/StringFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package StringFunctions {
	doc
	/*
	 * This package defines functions on String values, including those corresponding to string concatenation 
	 * and comparison operators in the KerML expression notation.
	 */

	public import ScalarValues::*;
	
	function '+' specializes ScalarFunctions::'+' { in x: String[1]; in y:String[1]; return : String[1]; }
	
	function Length{ in x: String[1]; return : Natural[1]; }
	function Substring{ in x: String[1]; in lower: Integer[1]; in upper: Integer[1]; return : String[1]; }
	
	function '<' specializes ScalarFunctions::'<' { in x: String[1]; in y: String[1]; return : Boolean[1]; }
	function '>' specializes ScalarFunctions::'>' { in x: String[1]; in y: String[1]; return : Boolean[1]; }
	function '<=' specializes ScalarFunctions::'<=' { in x: String[1]; in y: String[1]; return : Boolean[1]; }
	function '>=' specializes ScalarFunctions::'>=' { in x: String[1]; in y: String[1]; return : Boolean[1]; }

	function '==' specializes DataFunctions::'==' { in x: String[0..1]; in y: String[0..1]; return : Boolean[1]; }
	
	function ToString specializes BaseFunctions::ToString { in x: String[1];
		return : String[1] = x;
	}
}
```

## Declarations

- `StringFunctions` — standard library package
- `StringFunctions::+` — function
- `StringFunctions::Length` — function
- `StringFunctions::Substring` — function
- `StringFunctions::<` — function
- `StringFunctions::>` — function
- `StringFunctions::<=` — function
- `StringFunctions::>=` — function
- `StringFunctions::==` — function
- `StringFunctions::ToString` — function
