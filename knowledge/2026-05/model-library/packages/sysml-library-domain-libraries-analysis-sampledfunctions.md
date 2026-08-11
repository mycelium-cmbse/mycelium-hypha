---
name: SampledFunctions
kind: model-library-file
language: SysML
source: sysml.library/Domain Libraries/Analysis/SampledFunctions.sysml
declares: [SampledFunctions, SampledFunctions::SamplePair, SampledFunctions::SamplePair::domainValue, SampledFunctions::SamplePair::rangeValue, SampledFunctions::SampledFunction, SampledFunctions::SampledFunction::samples, SampledFunctions::Domain, SampledFunctions::Range, SampledFunctions::Sample, SampledFunctions::Sample::domainValues, SampledFunctions::Interpolate, SampledFunctions::Interpolate::fn, SampledFunctions::Interpolate::value, SampledFunctions::Interpolate::result, SampledFunctions::interpolateLinear, SampledFunctions::interpolateLinear::fn, SampledFunctions::interpolateLinear::value, SampledFunctions::interpolateLinear::domainValues, SampledFunctions::interpolateLinear::index, SampledFunctions::interpolateLinear::Linear, SampledFunctions::interpolateLinear::Linear::lowerSample, SampledFunctions::interpolateLinear::Linear::upperSample, SampledFunctions::interpolateLinear::Linear::value, SampledFunctions::interpolateLinear::Linear::f]
license: EPL-2.0
---

# SampledFunctions

Verbatim SysML standard-library source from `sysml.library/Domain Libraries/Analysis/SampledFunctions.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package SampledFunctions {
	doc
	/*
	 * This package provides a library model of discretely sampled mathematical functions.
	 */

	private import Base::Anything;
	private import ScalarValues::Positive;
	private import Collections::KeyValuePair;
	private import Collections::OrderedMap;
	private import SequenceFunctions::size;
	private import ControlFunctions::forAll;
	private import ControlFunctions::collect;
	private import ControlFunctions::select;
	
    attribute def SamplePair :> KeyValuePair {
		doc
		/*
		 * SamplePair is a key-value pair of a domain-value and a range-value, used as a sample element in SampledFunction.
		 */
	
        attribute domainValue :>> key;
        attribute rangeValue :>> val;
    }

	attribute def SampledFunction :> OrderedMap {
		doc
		/*
	     * SampledFunction is a variable-size, ordered collection of 'SamplePair' elements that represents a generic, discretely sampled, 
	     * uni-variate or multi-variate mathematical function. The function must be montonic, either strictly increasing or strictly
	     * decreasing.
	     * 
	     * It maps discrete domain values to discrete range values.
	     * The domain of the function is represented by the sequence of 'domainValue' of each 'SamplePair' in 'samples', and 
	     * the range of the function is represented by the sequence of 'rangeValue' of each 'SamplePair' in 'samples'.
	     */
	
		attribute samples: SamplePair[0..*] ordered :>> elements;
		
		assert constraint {
			// Note: Assumes the functions '<' and '>' are defined for the domain type.
			(1..size(samples)-1)->forAll { in i; (samples.domainValue#(i) < samples.domainValue#(i+1)) } or  // Strictly increasing
            (1..size(samples)-1)->forAll { in i; (samples.domainValue#(i) > samples.domainValue#(i+1)) }     // Strictly decreasing
		}
	}
	
	calc def Domain { 
		doc
		/* 
		 * Domain returns the sequence of the domainValues of all samples in a SampledFunction.
		 */
		 
		in fn : SampledFunction; 
		return : Anything[0..*] = fn.samples.domainValue;
	}
	
	calc def Range { 
		doc
		/* 
		 * Range returns the sequence of the rangeValues of all samples in a SampledFunction.
		 */
			
		in fn : SampledFunction; 
		return : Anything[0..*] = fn.samples.rangeValue;
	}
	
	calc def Sample {
		doc
		/* 
		 * Sample returns a SampledFunction that samples a given calculation over a sequence of domainValues.
		 */
		 
		in calc calculation { in x; }
		in attribute domainValues [0..*];
		return sampling = new SampledFunction (
			samples = domainValues->collect { in x; new SamplePair(x, calculation(x)) }
		);
	}
	
	calc def Interpolate {
		doc
		/*
		 * An Interpolate calculation returns an interpolated range value from a given SampledFunction for a given domain value.
		 * If the input domain value is outside the bounds of the domainValues of the SampleFunction, null is returned.
		 */
	
		in attribute fn : SampledFunction;
		in attribute value;
		return attribute result;
	}
		
	calc interpolateLinear : Interpolate {
		doc
		/*
		 * interpolateLinear is an Interpolate calculation assuming a linear functional form between SamplePairs.
		 */
	
		in attribute fn : SampledFunction;
		in attribute value;
		
		private attribute domainValues = Domain(fn);
		private attribute index : Positive[0..1] =
			(1..size(domainValues))->select { in i : Positive; domainValues#(i) <= value }#(1);
			
		private calc def Linear {
			in attribute lowerSample : SamplePair;
			in attribute upperSample : SamplePair;
			in attribute value;
			private attribute f = (value - lowerSample.domainValue) / (lowerSample.domainValue - upperSample.domainValue);
			return result = upperSample.rangeValue + f * (lowerSample.rangeValue - upperSample.rangeValue);				
		}
		
		return result [0..1] =
			if index == null or index == size(domainValues)? null
			else if domainValues#(index) < domainValues#(index+1)? Linear(fn.samples#(index), fn.samples#(index+1), value)
			else Linear(fn.samples#(index+1), fn.samples#(index), value);
	}
	
}
```

## Declarations

- `SampledFunctions` — standard library package
- `SampledFunctions::SamplePair` — attribute def
- `SampledFunctions::SamplePair::domainValue` — attribute
- `SampledFunctions::SamplePair::rangeValue` — attribute
- `SampledFunctions::SampledFunction` — attribute def
- `SampledFunctions::SampledFunction::samples` — attribute
- `SampledFunctions::Domain` — calc def
- `SampledFunctions::Range` — calc def
- `SampledFunctions::Sample` — calc def
- `SampledFunctions::Sample::domainValues` — attribute
- `SampledFunctions::Interpolate` — calc def
- `SampledFunctions::Interpolate::fn` — attribute
- `SampledFunctions::Interpolate::value` — attribute
- `SampledFunctions::Interpolate::result` — attribute
- `SampledFunctions::interpolateLinear` — calc
- `SampledFunctions::interpolateLinear::fn` — attribute
- `SampledFunctions::interpolateLinear::value` — attribute
- `SampledFunctions::interpolateLinear::domainValues` — attribute
- `SampledFunctions::interpolateLinear::index` — attribute
- `SampledFunctions::interpolateLinear::Linear` — calc def
- `SampledFunctions::interpolateLinear::Linear::lowerSample` — attribute
- `SampledFunctions::interpolateLinear::Linear::upperSample` — attribute
- `SampledFunctions::interpolateLinear::Linear::value` — attribute
- `SampledFunctions::interpolateLinear::Linear::f` — attribute
