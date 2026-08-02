---
name: Action Performance Example
kind: example
language: SysML
source: sysml/src/training/18. Action Performance/Action Performance Example.sysml
elements: [ActionUsage, PartDefinition, PartUsage, PerformActionUsage]
license: EPL-2.0
---

# Action Performance Example

Verbatim SysML model from `sysml/src/training/18. Action Performance/Action Performance Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Action Performance Example' {
	private import 'Action Decomposition'::*;
	
	part def Camera;
	part def AutoFocus;
	part def Imager;
	
	part camera : Camera {
		
		perform action takePhoto[*] ordered 
			references takePicture;
		
		part f : AutoFocus {
			perform takePhoto.focus;			
		}
		
		part i : Imager {
			perform takePhoto.shoot;
		}		
	}
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PerformActionUsage](../metamodel/elements/PerformActionUsage.md)
