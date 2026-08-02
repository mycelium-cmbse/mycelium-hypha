---
name: TakePicture
kind: example
language: KerML
source: kerml/src/examples/Behavior Examples/TakePicture.kerml
elements: [FlowUsage, SuccessionFlowUsage]
license: EPL-2.0
---

# TakePicture

Verbatim KerML model from `kerml/src/examples/Behavior Examples/TakePicture.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
behavior TakePicture {
	private import Camera;
	
	feature camera: Camera[1] subsets involvedObjects;
	
	class Exposure;
	
	behavior Focus { out xrsl: Exposure; }
	behavior Shoot { in xsf: Exposure; }
	
	step step1: Focus[1];	
	step step2: Shoot[1];
	
	succession flow exposure[1] of Exposure from step1.xrsl to step2.xsf;

	succession step1 then camera.focusedState;
	succession step2 then camera.shotState;
}
```

## Elements

- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [SuccessionFlowUsage](../metamodel/elements/SuccessionFlowUsage.md)
