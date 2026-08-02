---
name: Camera
kind: example
language: KerML
source: kerml/src/examples/Behavior Examples/Camera.kerml
elements: []
license: EPL-2.0
---

# Camera

Verbatim KerML model from `kerml/src/examples/Behavior Examples/Camera.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
class Camera {
	private import ScalarValues::*;
	
	portion focusedState: Camera subsets timeSlices;
	portion shotState: Camera subsets timeSlices;
	
	succession focusedState then shotState;
}
```
