---
name: DataFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/DataFunctions.kerml
declares: [DataFunctions, DataFunctions::==, DataFunctions::===, DataFunctions::+, DataFunctions::-, DataFunctions::*, DataFunctions::/, DataFunctions::**, DataFunctions::^, DataFunctions::%, DataFunctions::not, DataFunctions::xor, DataFunctions::~, DataFunctions::|, DataFunctions::&, DataFunctions::<, DataFunctions::>, DataFunctions::<=, DataFunctions::>=, DataFunctions::max, DataFunctions::min, DataFunctions::..]
license: EPL-2.0
---

# DataFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/DataFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package DataFunctions {
	doc
	/*
	 * This package defines the abstract base functions corresponding to all the unary and binary operators 
	 * in the KerML expression notation that might be defined on various kinds of DataValues.
	 */

	private import Base::DataValue;
	private import ScalarValues::Boolean;
	private import ControlFunctions::reduce;	
	
	abstract function '==' specializes BaseFunctions::'==' { in x: DataValue[0..1]; in y: DataValue[0..1]; 
		return : Boolean[1];
	}
	function '===' specializes BaseFunctions::'==='{ in x: DataValue[0..1]; in y: DataValue[0..1]; 
		return : Boolean[1] = x == y;
	}
	
	abstract function '+' { in x: DataValue[1]; in y: DataValue[0..1]; return : DataValue[1]; }
	abstract function '-' { in x: DataValue[1]; in y: DataValue[0..1]; return : DataValue[1]; }
	abstract function '*' { in x: DataValue[1]; in y: DataValue[1]; return : DataValue[1]; }
	abstract function '/' { in x: DataValue[1]; in y: DataValue[1]; return : DataValue[1]; }
	abstract function '**' { in x: DataValue[1]; in y: DataValue[1]; return : DataValue[1]; }
	abstract function '^' { in x: DataValue[1]; in y: DataValue[1]; return : DataValue[1]; }
	abstract function '%' { in x: DataValue[1]; in y: DataValue[1]; return : DataValue[1]; }
	
	abstract function 'not' { in x: DataValue[1]; return : DataValue[1]; }
	abstract function 'xor' { in x: DataValue[1]; in y: DataValue[1]; return : DataValue[1]; }

	abstract function '~' { in x: DataValue[1]; return : DataValue[1]; }
	abstract function '|' { in x: DataValue[1]; in y: DataValue[1]; return : DataValue[1]; }
	abstract function '&' { in x: DataValue[1]; in y: DataValue[1]; return : DataValue[1]; }
	
	abstract function '<' { in x: DataValue[1]; in y: DataValue[1]; return : Boolean[1]; }
	abstract function '>' { in x: DataValue[1]; in y: DataValue[1]; return : Boolean[1]; }
	abstract function '<=' { in x: DataValue[1]; in y: DataValue[1]; return : Boolean[1]; }
	abstract function '>=' { in x: DataValue[1]; in y: DataValue[1]; return : Boolean[1]; }
	
	abstract function max { in x: DataValue[1]; in y: DataValue[1]; return : DataValue[1]; }
	abstract function min { in x: DataValue[1]; in y: DataValue[1]; return : DataValue[1]; }
	
	abstract function '..' { in lower: DataValue[1]; in upper: DataValue[1]; return : DataValue[0..*] ordered; }	
}
```

## Declarations

- `DataFunctions` — standard library package
- `DataFunctions::==` — function
- `DataFunctions::===` — function
- `DataFunctions::+` — function
- `DataFunctions::-` — function
- `DataFunctions::*` — function
- `DataFunctions::/` — function
- `DataFunctions::**` — function
- `DataFunctions::^` — function
- `DataFunctions::%` — function
- `DataFunctions::not` — function
- `DataFunctions::xor` — function
- `DataFunctions::~` — function
- `DataFunctions::|` — function
- `DataFunctions::&` — function
- `DataFunctions::<` — function
- `DataFunctions::>` — function
- `DataFunctions::<=` — function
- `DataFunctions::>=` — function
- `DataFunctions::max` — function
- `DataFunctions::min` — function
- `DataFunctions::..` — function
