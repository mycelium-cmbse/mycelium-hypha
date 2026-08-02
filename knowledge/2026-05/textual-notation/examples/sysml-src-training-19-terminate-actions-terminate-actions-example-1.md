---
name: Terminate Actions Example-1
kind: example
language: SysML
source: sysml/src/training/19. Terminate Actions/Terminate Actions Example-1.sysml
elements: [ActionDefinition, ActionUsage]
license: EPL-2.0
---

# Terminate Actions Example-1

Verbatim SysML model from `sysml/src/training/19. Terminate Actions/Terminate Actions Example-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Terminate Actions Example-1' {
	private import ScalarValues::Boolean;
	
	action monitorCriticalActivity;
	action criticalActivity;
	action waitForTimeOut;
	
	action def MonitoredActivity {
		first start;

		then fork;
			then performCriticalActivity;
			then waitForTimeOut;
					
		action performCriticalActivity {
			perform monitorCriticalActivity;
			
			perform criticalActivity;
			then terminate;
		}
		then stop;
		
		action waitForTimeOut;
		then stop;
				
		action stop terminate;
	}
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
