---
name: CollectionFunctions
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Function Library/CollectionFunctions.kerml
declares: [CollectionFunctions, CollectionFunctions::==, CollectionFunctions::size, CollectionFunctions::isEmpty, CollectionFunctions::notEmpty, CollectionFunctions::contains, CollectionFunctions::containsAll, CollectionFunctions::head, CollectionFunctions::tail, CollectionFunctions::last, CollectionFunctions::#, CollectionFunctions::array#, CollectionFunctions::array#::n, CollectionFunctions::array#::index]
license: EPL-2.0
---

# CollectionFunctions

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Function Library/CollectionFunctions.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package CollectionFunctions {
	doc
	/*
	 * This package defines functions on Collections (as defined in the Collections package). 
	 * For functions on general sequences of values, see the SequenceFunctions package.
	 */

	private import Base::Anything;
	private import ScalarValues::*;
	private import SequenceFunctions::equals;
	private import SequenceFunctions::includes;
	private import ControlFunctions::exists;
	public import Collections::*;
	
	function '==' specializes BaseFunctions::'==' { in col1: Collection[0..1]; in col2: Collection[0..1];
		return : Boolean[1] = col1.elements->equals(col2.elements);
	}
	
	function size { in col: Collection[1];
		return : Natural[1] = SequenceFunctions::size(col.elements);
	}
	
	function isEmpty { in col: Collection[1]; 
		return : Boolean[1] = SequenceFunctions::isEmpty(col.elements);
	}
	
	function notEmpty { in col: Collection[1]; 
		return : Boolean[1] = SequenceFunctions::notEmpty(col.elements);
	}
	
	function contains { in col: Collection[1]; in values: Anything[*];
		return : Boolean[1] = col.elements->includes(values);
	}
	
	function containsAll { in col1: Collection[1]; in col2: Collection[2]; 
		return : Boolean[1] = contains(col1, col2.elements);
	}	
	
	function head { in col: OrderedCollection[1]; 
		return : Anything[0..1] = SequenceFunctions::head(col.elements);
	}
	
	function tail { in col: OrderedCollection[1]; 
		return : Anything[0..*] ordered nonunique = SequenceFunctions::tail(col.elements);	
	}
	
	function last { in col: OrderedCollection[1]; 
		return : Anything[0..1] = SequenceFunctions::last(col.elements);
	}
	
	function '#' specializes BaseFunctions::'#' { in col: OrderedCollection[1]; in index: Positive[1];
		// Cast ensures this function is not called recursively if the elements of col are OrderedCollections.
		return : Anything[0..1] = (col.elements as Anything)#(index);		
	}
	
	function 'array#' specializes BaseFunctions::'#' { in arr: Array[1]; in indexes: Positive[n] ordered nonunique;		
		private feature n : Natural[1] = arr.rank;
		
		// Assumes row-major ordering for elements.
		private function index { in arr: Array[1]; in i : Natural; in indexes : Positive[1..*];
			if i <= 1? indexes#(1) else arr.dimensions#(i) * (index(arr, i-1, indexes) - 1) + indexes#(i)
		}
		
		return : Anything[0..1] =
			if n == 0 or (1..n)->exists {in i; indexes#(i) > arr.dimensions#(i)}? null 
			else arr.elements#(index(arr, n, indexes));
	}	
}
```

## Declarations

- `CollectionFunctions` — standard library package
- `CollectionFunctions::==` — function
- `CollectionFunctions::size` — function
- `CollectionFunctions::isEmpty` — function
- `CollectionFunctions::notEmpty` — function
- `CollectionFunctions::contains` — function
- `CollectionFunctions::containsAll` — function
- `CollectionFunctions::head` — function
- `CollectionFunctions::tail` — function
- `CollectionFunctions::last` — function
- `CollectionFunctions::#` — function
- `CollectionFunctions::array#` — function
- `CollectionFunctions::array#::n` — feature
- `CollectionFunctions::array#::index` — function
