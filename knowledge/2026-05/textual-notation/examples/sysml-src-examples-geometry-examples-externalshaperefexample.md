---
name: ExternalShapeRefExample
kind: example
language: SysML
source: sysml/src/examples/Geometry Examples/ExternalShapeRefExample.sysml
elements: [AttributeUsage, ItemUsage, MetadataDefinition, MetadataUsage, PartUsage, ReferenceUsage]
license: EPL-2.0
---

# ExternalShapeRefExample

Verbatim SysML model from `sysml/src/examples/Geometry Examples/ExternalShapeRefExample.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ExternalShapeRefExample {
	private import ScalarValues::String;
	private import ShapeItems::*;
	private import ISQ::mass;
	private import SI::mm;

	metadata def ExternalShapeRef {
		doc
		/*
		 * Metadata to reference an externally defined shape.
		 */
	
		attribute purpose : String[1];
		attribute shapeIri : String[1];
	}
	
	part myBatteryUnit {
	    item :>> shape : Shell {
			metadata ExternalShapeRef {
				purpose = "highLoD";
				shapeIri = "file:/detailed-geometry/LEMS-250W_BatteryHousing_Example.step";
			}
		}		

		private item envelopingBoxBatteryUnit : Box :> envelopingShapes {
			:>> length = 140[mm];
			:>> width = 148[mm];
			:>> height = 90[mm];
		}
	}
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [MetadataDefinition](../metamodel/elements/MetadataDefinition.md)
- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [ReferenceUsage](../metamodel/elements/ReferenceUsage.md)
