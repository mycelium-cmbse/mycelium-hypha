---
name: ControlFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/ControlFunctions.kerml
declares: [ControlFunctions, ControlFunctions::., ControlFunctions::.::source, ControlFunctions::.::source::target, ControlFunctions::.::chain, ControlFunctions::if, ControlFunctions::if::thenValue, ControlFunctions::if::elseValue, ControlFunctions::??, ControlFunctions::??::secondValue, ControlFunctions::and, ControlFunctions::and::secondValue, ControlFunctions::or, ControlFunctions::or::secondValue, ControlFunctions::implies, ControlFunctions::implies::secondValue, ControlFunctions::collect, ControlFunctions::collect::mapper, ControlFunctions::select, ControlFunctions::select::selector, ControlFunctions::selectOne, ControlFunctions::selectOne::selector1, ControlFunctions::reject, ControlFunctions::reject::rejector, ControlFunctions::reduce, ControlFunctions::reduce::reducer, ControlFunctions::forAll, ControlFunctions::forAll::test, ControlFunctions::exists, ControlFunctions::exists::test, ControlFunctions::allTrue, ControlFunctions::anyTrue, ControlFunctions::minimize, ControlFunctions::minimize::fn, ControlFunctions::maximize, ControlFunctions::maximize::fn]
license: EPL-2.0
---

# ControlFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/ControlFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package ControlFunctions {
	doc
	/*
	 * This package defines functions that correspond to operators in the KerML expression notation 
	 * for which one or more operands are expressions whose evaluation is determined by another operand.
	 */

	private import Base::Anything;
	private import ScalarValues::ScalarValue;
	private import ScalarValues::Boolean;
	private import ScalarFunctions::min;
	private import ScalarFunctions::max;
	
	abstract function '.' {
		in feature source : Anything[0..*] nonunique {
	  		abstract feature target : Anything[0..*] nonunique;
	  	}	  	
	  	private feature chain chains source.target;
	    chain
	}
	
	abstract function 'if' { 
		in test: Boolean[1];
		in expr thenValue[0..1] { return : Anything[0..*] ordered nonunique; }
		in expr elseValue[0..1] { return : Anything[0..*] ordered nonunique; }
		return : Anything[0..*] ordered nonunique;
	}
	
	abstract function '??' {
		in firstValue: Anything[0..*] ordered nonunique;
		in expr secondValue[0..1] { return : Anything[0..*] ordered nonunique; }
		return : Anything[0..*] ordered nonunique;
	}
	
	function 'and' {
		in firstValue: Boolean[1];
		in expr secondValue[0..1] { return : Boolean[1]; }
		return : Boolean[1];
	}
	
	function 'or'{
		in firstValue: Boolean[1];
		in expr secondValue[0..1] { return : Boolean[1]; }
		return : Boolean[1];
	}
	
	function 'implies'{
		in firstValue: Boolean[1];
		in expr secondValue[0..1] { return : Boolean[1]; }
		return : Boolean[1];
	}
	
	abstract function collect { 
		in collection: Anything[0..*] ordered nonunique;
		in expr mapper[0..*] { in argument: Anything[1]; return : Anything[0..*] ordered nonunique; }
		return : Anything[0..*] ordered nonunique;
	}
	
	abstract function select { 
		in collection: Anything[0..*] ordered nonunique; 
		in expr selector[0..*] { in argument: Anything[1]; return : Boolean[1]; }
		return : Anything[0..*] ordered nonunique;
	}
	
	function selectOne { 
		in collection: Anything[0..*] ordered nonunique;
		in expr selector1[0..*] { in argument: Anything[1]; return : Boolean[1]; }
		return : Anything[0..1] = collection->select {in x; selector1(x)}#(1);
	}
	
	abstract function reject{ 
		in collection: Anything[0..*] ordered nonunique; 
		in expr rejector[0..*] { in argument: Anything[1]; return : Boolean[1]; }
		return : Anything[0..*] ordered nonunique;
	}
	
	abstract function reduce { 
		in collection: Anything[0..*] ordered nonunique; 
		in expr reducer[0..*] { in firstArg: Anything[1]; in secondArg: Anything[1]; return : Anything[1]; }
		return : Anything[0..*] ordered nonunique;
	}
	
	abstract function forAll { 
		in collection: Anything[0..*] ordered nonunique; 
		in expr test[0..*] { in argument: Anything[1]; return : Boolean[1]; }
		return : Boolean[1];
	}
	
	abstract function exists { 
		in collection: Anything[0..*] ordered nonunique;
		in expr test[0..*] { in argument: Anything[1]; return : Boolean[1]; }
		return : Boolean[1];
	}
	
	function allTrue {
		in collection: Boolean[0..*]; 
		return : Boolean[1] = collection->forAll {in x; x};
	}
	
	function anyTrue {
		in collection: Boolean[0..*];
		return : Boolean[1] = collection->exists {in x; x};
	}
	
	function minimize {
		in collection: ScalarValue[1..*];
		in expr fn[0..*] { in argument: ScalarValue[1]; return : ScalarValue[1]; }
		return : ScalarValue[1] = collection->collect {in x; fn(x)}->reduce min;
	}
	
	function maximize { 
		in collection: ScalarValue[1..*];
		in expr fn[0..*] { in argument: ScalarValue[1]; return : ScalarValue[1]; }
		return : ScalarValue = collection->collect {in x; fn(x)}->reduce max;
	}
	
}
```

## Declarations

- `ControlFunctions` — standard library package
- `ControlFunctions::.` — function
- `ControlFunctions::.::source` — feature
- `ControlFunctions::.::source::target` — feature
- `ControlFunctions::.::chain` — feature
- `ControlFunctions::if` — function
- `ControlFunctions::if::thenValue` — expr
- `ControlFunctions::if::elseValue` — expr
- `ControlFunctions::??` — function
- `ControlFunctions::??::secondValue` — expr
- `ControlFunctions::and` — function
- `ControlFunctions::and::secondValue` — expr
- `ControlFunctions::or` — function
- `ControlFunctions::or::secondValue` — expr
- `ControlFunctions::implies` — function
- `ControlFunctions::implies::secondValue` — expr
- `ControlFunctions::collect` — function
- `ControlFunctions::collect::mapper` — expr
- `ControlFunctions::select` — function
- `ControlFunctions::select::selector` — expr
- `ControlFunctions::selectOne` — function
- `ControlFunctions::selectOne::selector1` — expr
- `ControlFunctions::reject` — function
- `ControlFunctions::reject::rejector` — expr
- `ControlFunctions::reduce` — function
- `ControlFunctions::reduce::reducer` — expr
- `ControlFunctions::forAll` — function
- `ControlFunctions::forAll::test` — expr
- `ControlFunctions::exists` — function
- `ControlFunctions::exists::test` — expr
- `ControlFunctions::allTrue` — function
- `ControlFunctions::anyTrue` — function
- `ControlFunctions::minimize` — function
- `ControlFunctions::minimize::fn` — expr
- `ControlFunctions::maximize` — function
- `ControlFunctions::maximize::fn` — expr
