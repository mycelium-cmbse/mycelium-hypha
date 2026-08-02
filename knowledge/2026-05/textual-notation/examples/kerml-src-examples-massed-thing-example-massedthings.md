---
name: MassedThings
kind: example
language: KerML
source: kerml/src/examples/Massed Thing Example/MassedThings.kerml
elements: []
license: EPL-2.0
---

# MassedThings

Verbatim KerML model from `kerml/src/examples/Massed Thing Example/MassedThings.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
private import ScalarValues::*;
package MassedThings {
	
	public class MassedThing {
		public name: String;
		public mass: Real = 0;
	}
	
	public assoc MassedThingAssembly {
		public end [0..1] feature assembly: MassedThing;
		public end [0..*] feature parts: MassedThing;
	}
}
```
