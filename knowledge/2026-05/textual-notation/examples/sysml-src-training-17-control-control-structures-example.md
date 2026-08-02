---
name: Control Structures Example
kind: example
language: SysML
source: sysml/src/training/17. Control/Control Structures Example.sysml
elements: [ActionDefinition, ActionUsage, AttributeDefinition, AttributeUsage, LoopActionUsage, PartUsage]
license: EPL-2.0
---

# Control Structures Example

Verbatim SysML model from `sysml/src/training/17. Control/Control Structures Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Control Structures Example' {
	private import ScalarValues::*;
	
	attribute def BatteryCharged;
	
	part battery;
	part powerSystem;
	
	action def MonitorBattery { out charge : Real; }
	action def AddCharge { in charge : Real; }
	action def EndCharging;
	
	action def ChargeBattery {
		loop action charging {
			action monitor : MonitorBattery {
				out charge;
			}
			
			then if monitor.charge < 100 {
				action addCharge : AddCharge {
					in charge = monitor.charge;
				}
			}				
		} until charging.monitor.charge >= 100;
		
		then action endCharging : EndCharging;
		then done;
	}
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [LoopActionUsage](../metamodel/elements/LoopActionUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
