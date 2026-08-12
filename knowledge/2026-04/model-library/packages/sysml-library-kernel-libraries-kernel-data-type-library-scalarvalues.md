---
name: ScalarValues
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Data Type Library/ScalarValues.kerml
declares: [ScalarValues, ScalarValues::ScalarValue, ScalarValues::Boolean, ScalarValues::String, ScalarValues::NumericalValue, ScalarValues::Number, ScalarValues::Complex, ScalarValues::Real, ScalarValues::Rational, ScalarValues::Integer, ScalarValues::Natural, ScalarValues::Positive]
license: EPL-2.0
---

# ScalarValues

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Data Type Library/ScalarValues.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package ScalarValues {
	doc
	/*
	 * This package contains a basic set of primitive scalar (non-collection) data types. 
	 * These include Boolean and String types and a hierarchy of concrete Number types, from 
	 * the most general type of Complex numbers to the most specific type of Positive integers.</p>
	 */

	private import Base::DataValue;
	
	abstract datatype ScalarValue specializes DataValue;
	datatype Boolean specializes ScalarValue;
	datatype String specializes ScalarValue;
	abstract datatype NumericalValue specializes ScalarValue;
	
    abstract datatype Number specializes NumericalValue;
	datatype Complex specializes Number;
	datatype Real specializes Complex;	
	datatype Rational specializes Real;
	datatype Integer specializes Rational;
	datatype Natural specializes Integer;
    datatype Positive specializes Natural;	
}		
```

## Declarations

- `ScalarValues` — standard library package
- `ScalarValues::ScalarValue` — datatype
- `ScalarValues::Boolean` — datatype
- `ScalarValues::String` — datatype
- `ScalarValues::NumericalValue` — datatype
- `ScalarValues::Number` — datatype
- `ScalarValues::Complex` — datatype
- `ScalarValues::Real` — datatype
- `ScalarValues::Rational` — datatype
- `ScalarValues::Integer` — datatype
- `ScalarValues::Natural` — datatype
- `ScalarValues::Positive` — datatype
