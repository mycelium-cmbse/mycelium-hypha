---
name: Action Succession Example-1
kind: example
language: SysML
source: sysml/src/training/14. Action Definitions/Action Succession Example-1.sysml
elements: [ActionDefinition, ActionUsage, FlowUsage, ItemDefinition, ItemUsage]
license: EPL-2.0
---

# Action Succession Example-1

Verbatim SysML model from `sysml/src/training/14. Action Definitions/Action Succession Example-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Action Succession Example-1' {
	item def Scene;
	item def Image;
	item def Picture;
	
	action def Focus { in scene : Scene; out image : Image; }
	action def Shoot { in image: Image; out picture : Picture; }	
				
	action def TakePicture {
		in item scene : Scene;
		out item picture : Picture;
		
		bind focus.scene = scene;
		
		action focus: Focus { in scene; out image; }
		
		flow from focus.image to shoot.image;
		
		first focus then shoot;
		
		action shoot: Shoot { in image; out picture; }
		
		bind shoot.picture = picture;
	}
	
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
