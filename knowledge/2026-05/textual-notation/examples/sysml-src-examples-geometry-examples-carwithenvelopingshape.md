---
name: CarWithEnvelopingShape
kind: example
language: SysML
source: sysml/src/examples/Geometry Examples/CarWithEnvelopingShape.sysml
elements: [ItemUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# CarWithEnvelopingShape

Verbatim SysML model from `sysml/src/examples/Geometry Examples/CarWithEnvelopingShape.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package CarWithEnvelopingShape {
	private import ShapeItems::Box;
	private import SI::mm;

	part def Car {
		doc
		/*
		 * Example car with simple enveloping shape that is a solid box
		 */
	
		item boundingBox : Box [1] :> boundingShapes {
			:>> length = 4800 [mm];
			:>> width  = 1840 [mm];
			:>> height = 1350 [mm];
		}
	}
}
```

## Elements

- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
