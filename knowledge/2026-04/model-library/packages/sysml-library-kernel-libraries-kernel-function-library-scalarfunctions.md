---
name: ScalarFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/ScalarFunctions.kerml
declares: [ScalarFunctions, ScalarFunctions::+, ScalarFunctions::-, ScalarFunctions::*, ScalarFunctions::/, ScalarFunctions::**, ScalarFunctions::^, ScalarFunctions::%, ScalarFunctions::not, ScalarFunctions::xor, ScalarFunctions::~, ScalarFunctions::|, ScalarFunctions::&, ScalarFunctions::<, ScalarFunctions::>, ScalarFunctions::<=, ScalarFunctions::>=, ScalarFunctions::max, ScalarFunctions::min, ScalarFunctions::..]
license: EPL-2.0
---

# ScalarFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/ScalarFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package ScalarFunctions {
	doc
	/*
	 * This package defines abstract functions that specialize the DataFunctions for use with ScalarValues. 
	 */

	public import ScalarValues::*;
	
	abstract function '+' specializes DataFunctions::'+' { in x: ScalarValue[1]; in y: ScalarValue[0..1]; return : ScalarValue[1]; }
	abstract function '-' specializes DataFunctions::'-' { in x: ScalarValue[1]; in y: ScalarValue[0..1]; return : ScalarValue[1]; }
	abstract function '*' specializes DataFunctions::'*' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : ScalarValue[1]; }
	abstract function '/' specializes DataFunctions::'/' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : ScalarValue[1]; }
	abstract function '**' specializes DataFunctions::'**' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : ScalarValue[1]; }
	abstract function '^' specializes DataFunctions::'^' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : ScalarValue[1]; }
	abstract function '%' specializes DataFunctions::'%' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : ScalarValue[1]; }
	
	abstract function 'not' specializes DataFunctions::'not' { in x: ScalarValue[1]; return : ScalarValue[1]; }
	abstract function 'xor' specializes DataFunctions::'xor' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : ScalarValue[1]; }

	abstract function '~' specializes DataFunctions::'~' { in x: ScalarValue[1]; return : ScalarValue[1]; }	
	abstract function '|' specializes DataFunctions::'|' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : ScalarValue[1]; }
	abstract function '&' specializes DataFunctions::'&' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : ScalarValue[1]; }
	
	abstract function '<' specializes DataFunctions::'<' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : Boolean[1]; }
	abstract function '>' specializes DataFunctions::'>' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : Boolean[1]; }
	abstract function '<=' specializes DataFunctions::'<=' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : Boolean[1]; }
	abstract function '>=' specializes DataFunctions::'>=' { in x: ScalarValue[1]; in y: ScalarValue[1]; return : Boolean[1]; }
	
	abstract function max specializes DataFunctions::max { in x: ScalarValue[1]; in y: ScalarValue[1]; return : ScalarValue[1]; }
	abstract function min specializes DataFunctions::min { in x: ScalarValue[1]; in y: ScalarValue[1]; return : ScalarValue[1]; }
	
	abstract function '..' specializes DataFunctions::'..' { in lower: ScalarValue[1]; in upper: ScalarValue[1]; return : ScalarValue[0..*]; }
}
```

## Declarations

- `ScalarFunctions` — standard library package
- `ScalarFunctions::+` — function
- `ScalarFunctions::-` — function
- `ScalarFunctions::*` — function
- `ScalarFunctions::/` — function
- `ScalarFunctions::**` — function
- `ScalarFunctions::^` — function
- `ScalarFunctions::%` — function
- `ScalarFunctions::not` — function
- `ScalarFunctions::xor` — function
- `ScalarFunctions::~` — function
- `ScalarFunctions::|` — function
- `ScalarFunctions::&` — function
- `ScalarFunctions::<` — function
- `ScalarFunctions::>` — function
- `ScalarFunctions::<=` — function
- `ScalarFunctions::>=` — function
- `ScalarFunctions::max` — function
- `ScalarFunctions::min` — function
- `ScalarFunctions::..` — function
