---
name: Messaging with Ports
kind: example
language: SysML
source: sysml/src/training/21. Asynchronous Messaging/Messaging with Ports.sysml
elements: [ActionDefinition, ActionUsage, AttributeDefinition, AttributeUsage, FlowUsage, ItemDefinition, ItemUsage, PartUsage, PortUsage]
license: EPL-2.0
---

# Messaging with Ports

Verbatim SysML model from `sysml/src/training/21. Asynchronous Messaging/Messaging with Ports.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Messaging Example' {
	item def Scene;
	item def Image;
	item def Picture;
	
	attribute def Show {
		item picture : Picture;
	}
	
	action def Focus { in item scene : Scene; out item image : Image; }
	action def Shoot { in item image : Image; out item picture : Picture; }
	action def TakePicture;
	
	part screen {
		port displayPort;
	}
	
	part camera {
		port viewPort;
		port displayPort;
		
		action takePicture : TakePicture {
			action trigger accept scene : Scene via viewPort;
			
			then action focus : Focus {
				in item scene = trigger.scene;
				out item image;
			}
			
			flow from focus.image to shoot.image;
		
			then action shoot : Shoot {
				in item image; 
				out item picture;
			}
			
			then send new Show(shoot.picture) via displayPort;
		}
	}
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
