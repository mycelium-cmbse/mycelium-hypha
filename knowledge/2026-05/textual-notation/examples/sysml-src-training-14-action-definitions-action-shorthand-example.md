---
name: Action Shorthand Example
kind: example
language: SysML
source: sysml/src/training/14. Action Definitions/Action Shorthand Example.sysml
elements: [ActionDefinition, ActionUsage, FlowUsage, ItemDefinition, ItemUsage]
license: EPL-2.0
---

# Action Shorthand Example

Verbatim SysML model from `sysml/src/training/14. Action Definitions/Action Shorthand Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Action Shorthand Example' {
	item def Scene;
	item def Image;
	item def Picture;
	
	action def Focus { in scene : Scene; out image : Image; }
	action def Shoot { in image: Image; out picture : Picture; }	
				
	action def TakePicture {
		in item scene : Scene;
		out item picture : Picture;
		
		action focus: Focus {
			in item scene = TakePicture::scene;
			out item image;
		}
		
		flow from focus.image to shoot.image;
		
		then action shoot: Shoot {
			in item;
			out item picture = TakePicture::picture;
		}
	}
	
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
