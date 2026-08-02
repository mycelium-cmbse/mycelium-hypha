---
name: Associations
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Associations.kerml
elements: []
license: EPL-2.0
---

# Associations

Verbatim KerML model from `kerml/src/examples/Simple Tests/Associations.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Associations {
    datatype X;
    class Y;
    
	assoc A {
		end x_cross [1..1] feature x : X; 
		end y_cross [1..*] feature y : Y;
	}
	
	assoc B specializes A {
		end x1;
		end [0..*] feature y1 redefines y;
	}
	
	assoc struct C {
		const end [1] feature a;
		const end feature b;
	}
	
	metaclass M;	
	assoc XY {
		end [0..1] feature x : X {
			@M;
		}
		end feature y : Y;
	}
}
```
