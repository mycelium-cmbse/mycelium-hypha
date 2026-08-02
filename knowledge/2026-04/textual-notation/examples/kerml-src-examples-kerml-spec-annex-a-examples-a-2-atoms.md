---
name: A-2-Atoms
kind: example
language: KerML
source: kerml/src/examples/KerML Spec Annex A Examples/A-2-Atoms.kerml
elements: []
license: EPL-2.0
---

# A-2-Atoms

Verbatim KerML model from `kerml/src/examples/KerML Spec Annex A Examples/A-2-Atoms.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Atoms {
	doc
	/* This package defines a keyword (atom) for classifiers with
	 * exactly one instance and are disjoint from any others
	 * marked with this keyword.
	 */

	private import Metaobjects::Metaobject;
	
	classifier Atom;
	metaclass <atom> AtomMetadata specializes Metaobject {
		baseType = Atom meta KerML::Classifier;
	}
}
```
