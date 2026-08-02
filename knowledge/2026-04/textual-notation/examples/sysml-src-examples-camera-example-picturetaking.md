---
name: PictureTaking
kind: example
language: SysML
source: sysml/src/examples/Camera Example/PictureTaking.sysml
elements: [ActionDefinition, ActionUsage, FlowUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# PictureTaking

Verbatim SysML model from `sysml/src/examples/Camera Example/PictureTaking.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package PictureTaking {
	part def Exposure;
	
	action def Focus { out xrsl: Exposure; }
	action def Shoot { in xsf: Exposure; }	
		
	action takePicture {		
		action focus: Focus[1];
		flow of Exposure from focus.xrsl to shoot.xsf;
		action shoot: Shoot[1];
	}
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
