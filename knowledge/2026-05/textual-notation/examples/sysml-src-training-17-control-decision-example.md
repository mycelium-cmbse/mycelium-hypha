---
name: Decision Example
kind: example
language: SysML
source: sysml/src/training/17. Control/Decision Example.sysml
elements: [ActionDefinition, ActionUsage, AttributeDefinition, AttributeUsage, PartUsage]
license: EPL-2.0
---

# Decision Example

Verbatim SysML model from `sysml/src/training/17. Control/Decision Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Decision Example' {
	private import ScalarValues::*;
	
	attribute def BatteryCharged;
	
	part battery;
	part powerSystem;
	
	action def MonitorBattery { out charge : Real; }
	action def AddCharge { in charge : Real; }
	action def EndCharging;
	
	action def ChargeBattery {
		first start;

		then merge continueCharging;
		
		then action monitor : MonitorBattery {
			out batteryCharge : Real;
		}
		
		then decide;
			if monitor.batteryCharge < 100 then addCharge;
			if monitor.batteryCharge >= 100 then endCharging;
			
		action addCharge : AddCharge {
			in charge = monitor.batteryCharge;
		}
		then continueCharging;
		
		action endCharging : EndCharging;
		then done;
	}
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
