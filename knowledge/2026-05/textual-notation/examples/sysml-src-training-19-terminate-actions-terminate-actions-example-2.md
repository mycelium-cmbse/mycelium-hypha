---
name: Terminate Actions Example-2
kind: example
language: SysML
source: sysml/src/training/19. Terminate Actions/Terminate Actions Example-2.sysml
elements: [ActionDefinition, ActionUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Terminate Actions Example-2

Verbatim SysML model from `sysml/src/training/19. Terminate Actions/Terminate Actions Example-2.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Terminate Actions Example-2' {
	action def WorkflowProcess;
	
	part def Processor {
		ref action workflowProcess : WorkflowProcess;
		
		action internalProcess {
			// ...
		}
	}
		
	action terminateProcessing {
		in processor : Processor;
		
		terminate processor.workflowProcess;
				
		terminate processor;
	}

}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
