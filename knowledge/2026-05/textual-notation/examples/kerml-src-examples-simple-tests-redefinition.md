---
name: Redefinition
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Redefinition.kerml
elements: []
license: EPL-2.0
---

# Redefinition

Verbatim KerML model from `kerml/src/examples/Simple Tests/Redefinition.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Redefinition {
	
	classifier A {
	    feature f;
	}
	
	classifier B specializes A {
	    feature redefines f {
	        feature g;
	    }
	}
	
	classifier C specializes A, B {
	    feature subsets f {
	        feature redefines g;
	    }
	}

	class X {
		feature redefines startShot;
		feature redefines endShot;
	}
}
```
