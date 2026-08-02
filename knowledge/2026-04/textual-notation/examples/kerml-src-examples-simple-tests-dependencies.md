---
name: Dependencies
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Dependencies.kerml
elements: []
license: EPL-2.0
---

# Dependencies

Verbatim KerML model from `kerml/src/examples/Simple Tests/Dependencies.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Dependencies {
	
	package System {
		package 'Application Layer';
		package 'Service Layer';
		package 'Data Layer';
	}
	
	public import System::*;
	
	dependency Use from 'Application Layer' to 'Service Layer';
	dependency from 'Service Layer' to 'Data Layer';
	
	feature x;
	feature y;
	feature z;
	
	dependency z to x, y {
		feature e;
	}
	
}
```
