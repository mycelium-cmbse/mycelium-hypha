---
name: Action Decomposition
kind: example
language: SysML
source: sysml/src/training/15. Actions/Action Decomposition.sysml
elements: [ActionDefinition, ActionUsage, FlowUsage, ItemUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Action Decomposition

Verbatim SysML model from `sysml/src/training/15. Actions/Action Decomposition.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Action Decomposition' {
	part def Scene;
	part def Image;
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
		
		flow from focus.image to shoot.image;

		action shoot : Shoot {
			in item; 
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
