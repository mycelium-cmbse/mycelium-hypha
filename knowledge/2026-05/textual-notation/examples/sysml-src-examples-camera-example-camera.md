---
name: Camera
kind: example
language: SysML
source: sysml/src/examples/Camera Example/Camera.sysml
elements: [ActionUsage, PartDefinition, PartUsage, PerformActionUsage]
license: EPL-2.0
---

# Camera

Verbatim SysML model from `sysml/src/examples/Camera Example/Camera.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
part def Camera {
	private import PictureTaking::*;
	
	perform action takePicture[*] :> PictureTaking::takePicture;
	
	part focusingSubsystem {
		perform takePicture.focus;
	}
	
	part imagingSubsystem {
		perform takePicture.shoot;
	}
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PerformActionUsage](../metamodel/elements/PerformActionUsage.md)
