---
name: Camera
kind: example
language: SysML
source: sysml/src/training/17. Control/Camera.sysml
elements: [FlowUsage, ItemUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Camera

Verbatim SysML model from `sysml/src/training/17. Control/Camera.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package Camera {
	private import 'Action Decomposition'::*;
	
	part def Camera;
	part def FocusingSubsystem;
	part def ImagingSubsystem;
	
	part camera : Camera {
		ref item scene : Scene;
		part photos : Picture[*];
				
		part autoFocus {
			in ref item scene : Scene = camera::scene;		
			out ref item realImage : Image;
		}
		
		flow autoFocus.realImage to imager.focusedImage;
		
		part imager {
			in item focusedImage : Image;		
			out item photo : Picture :> photos;
		}
		
	}
}
```

## Elements

- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
