---
name: Conditional Succession Example-2
kind: example
language: SysML
source: sysml/src/training/16. Conditional Succession/Conditional Succession Example-2.sysml
elements: [ActionDefinition, ActionUsage, FlowUsage, ItemUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Conditional Succession Example-2

Verbatim SysML model from `sysml/src/training/16. Conditional Succession/Conditional Succession Example-2.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Conditional Succession Example-2' {
	part def Scene;
	part def Image {
		isWellFocused: ScalarValues::Boolean;
	}
	part def Picture;
	
	action def Focus { in scene : Scene; out image : Image; }
	action def Shoot { in image: Image; out picture : Picture; }	
	action def TakePicture { in scene : Scene; out picture : Picture; }
	
	action takePicture : TakePicture {
		in item scene;
		out item picture;
		
		action focus : Focus {
			in item scene = takePicture::scene; 
			out item image;
		}
		
		if focus.image.isWellFocused then shoot;
		
		flow from focus.image to shoot.image;

		action shoot : Shoot {
			in item image; 
			out item picture = takePicture::picture;
		}
	}
	
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
