---
name: MedicalDeviceFailure
kind: example
language: SysML
source: sysml/src/examples/Cause and Effect Examples/MedicalDeviceFailure.sysml
elements: [ConnectionUsage, EventOccurrenceUsage, OccurrenceUsage, PartUsage]
license: EPL-2.0
---

# MedicalDeviceFailure

Verbatim SysML model from `sysml/src/examples/Cause and Effect Examples/MedicalDeviceFailure.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package MedicalDeviceFailure {
	private import CauseAndEffect::*;
	
	part medicalDevice {
		part battery {
			event occurrence depleted;
			event occurrence cannotBeCharged;
		}
		
		event occurrence deviceFails;
		
		ref patient {
			event occurrence therapyDelayed;
		}
		
		#multicausation connection {
			end #cause ::> battery.depleted;
			end #cause ::> battery.cannotBeCharged;
			end #effect ::> deviceFails;
		}
		
		#causation connect deviceFails to patient.therapyDelayed;
	}	
	
}
```

## Elements

- [ConnectionUsage](../metamodel/elements/ConnectionUsage.md)
- [EventOccurrenceUsage](../metamodel/elements/EventOccurrenceUsage.md)
- [OccurrenceUsage](../metamodel/elements/OccurrenceUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
