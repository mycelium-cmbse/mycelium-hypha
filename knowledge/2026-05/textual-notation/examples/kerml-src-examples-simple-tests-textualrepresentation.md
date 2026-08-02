---
name: TextualRepresentation
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/TextualRepresentation.kerml
elements: []
license: EPL-2.0
---

# TextualRepresentation

Verbatim KerML model from `kerml/src/examples/Simple Tests/TextualRepresentation.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package TextualRepresentation {
	private import ScalarValues::Real;
	
	class C {
	    feature x: Real;
	    inv x_constraint {
		    rep inOCL language "ocl" 
		        /* self.x > 0.0 */
	    }
	}
	
	behavior setX { in c : C; in newX : Real;
	    language "alf" 
	        /* c.x = newX;
	         * WriteLine("Set new x");
	         */
	}
	
}
```
