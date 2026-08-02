---
name: Imports
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Imports.kerml
elements: []
license: EPL-2.0
---

# Imports

Verbatim KerML model from `kerml/src/examples/Simple Tests/Imports.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Imports {

	package P {
		class A;
		class B;
		class C;
	}
	
	package Q {
		class A;
		class D {
			class E;
		}
		package Q1 {
			class D;
			class E;
			private package Q1a {
				class G;
			}
		}
		package Q2 {
			class F;
		}
	}
	
	package R {
		public import Q::*;
	}

	
	package S {
		public import P::*;
		public import Q::**;
		
		class X :> A;
		class Y :> D;
		class Z :> F;
	}
	
	package S1 {
		public import P::*;
		public import R::*;
		
		class X :> A;
	}
}
```
