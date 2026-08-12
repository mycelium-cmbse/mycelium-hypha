---
name: NumericalFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/NumericalFunctions.kerml
declares: [NumericalFunctions, NumericalFunctions::isZero, NumericalFunctions::isUnit, NumericalFunctions::abs, NumericalFunctions::+, NumericalFunctions::-, NumericalFunctions::*, NumericalFunctions::/, NumericalFunctions::**, NumericalFunctions::^, NumericalFunctions::%, NumericalFunctions::<, NumericalFunctions::>, NumericalFunctions::<=, NumericalFunctions::>=, NumericalFunctions::max, NumericalFunctions::min, NumericalFunctions::sum, NumericalFunctions::product, NumericalFunctions::sum0, NumericalFunctions::product1]
license: EPL-2.0
---

# NumericalFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/NumericalFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package NumericalFunctions {
	doc
	/*
	 * This package defines abstract functions on Numerical values for general arithmetic and comparison operations.
	 */

	public import ScalarValues::*;
	private import ControlFunctions::reduce;
	
	abstract function isZero{ in x: NumericalValue[1]; return : Boolean; }
	abstract function isUnit{ in x : NumericalValue[1]; return : Boolean; }
	
	abstract function abs{ in x: NumericalValue[1]; return : NumericalValue[1]; }
		
	abstract function '+' specializes ScalarFunctions::'+' { in x: NumericalValue[1]; in y: NumericalValue[0..1]; return : NumericalValue[1]; }
	abstract function '-' specializes ScalarFunctions::'-' { in x: NumericalValue[1]; in y: NumericalValue[0..1]; return : NumericalValue[1]; }
	abstract function '*' specializes ScalarFunctions::'*' { in x: NumericalValue[1]; in y: NumericalValue[1]; return : NumericalValue[1]; }
	abstract function '/' specializes ScalarFunctions::'/' { in x: NumericalValue[1]; in y: NumericalValue[1]; return : NumericalValue[1]; }
	abstract function '**' specializes ScalarFunctions::'**' { in x: NumericalValue[1]; in y: NumericalValue[1]; return : NumericalValue[1]; }
	abstract function '^' specializes ScalarFunctions::'^' { in x: NumericalValue[1]; in y: NumericalValue[1]; return : NumericalValue[1]; }
	abstract function '%' specializes ScalarFunctions::'%' { in x: NumericalValue[1]; in y: NumericalValue[1]; return : NumericalValue[1]; }
	
	abstract function '<' specializes ScalarFunctions::'<' { in x: NumericalValue[1]; in y: NumericalValue[1]; return : Boolean[1]; }
	abstract function '>' specializes ScalarFunctions::'>' { in x: NumericalValue[1]; in y: NumericalValue[1]; return : Boolean[1]; }
	abstract function '<=' specializes ScalarFunctions::'<=' { in x: NumericalValue[1]; in y: NumericalValue[1]; return : Boolean[1]; }
	abstract function '>=' specializes ScalarFunctions::'>=' { in x: NumericalValue[1]; in y: NumericalValue[1]; return : Boolean[1]; }
	
	abstract function max specializes ScalarFunctions::max { in x: NumericalValue[1]; in y: NumericalValue[1]; return : NumericalValue[1]; }
	abstract function min specializes ScalarFunctions::min { in x: NumericalValue[1]; in y: NumericalValue[1]; return : NumericalValue[1]; }
	
	abstract function sum { in collection: ScalarValue[0..*]; return : ScalarValue[1]; }	
	abstract function product { in collection: ScalarValue[0..*]; return : ScalarValue[1]; }
	
	function sum0 { in collection: NumericalValue[0..*]; in zero: ScalarValue[1]; 
 		inv { isZero(zero) }		
        return : ScalarValue = collection->reduce '+' ?? zero;
	}
	
	function product1 { in collection: ScalarValue[0..*]; in one: ScalarValue[1]; 
		inv { isUnit(one) }		
        return : ScalarValue = collection->reduce '*' ?? one;
	}
}
```

## Declarations

- `NumericalFunctions` — standard library package
- `NumericalFunctions::isZero` — function
- `NumericalFunctions::isUnit` — function
- `NumericalFunctions::abs` — function
- `NumericalFunctions::+` — function
- `NumericalFunctions::-` — function
- `NumericalFunctions::*` — function
- `NumericalFunctions::/` — function
- `NumericalFunctions::**` — function
- `NumericalFunctions::^` — function
- `NumericalFunctions::%` — function
- `NumericalFunctions::<` — function
- `NumericalFunctions::>` — function
- `NumericalFunctions::<=` — function
- `NumericalFunctions::>=` — function
- `NumericalFunctions::max` — function
- `NumericalFunctions::min` — function
- `NumericalFunctions::sum` — function
- `NumericalFunctions::product` — function
- `NumericalFunctions::sum0` — function
- `NumericalFunctions::product1` — function
