---
name: Opaque Action Example
kind: example
language: SysML
source: sysml/src/training/22. Opaque Actions/Opaque Action Example.sysml
elements: [ActionDefinition, ActionUsage, AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Opaque Action Example

Verbatim SysML model from `sysml/src/training/22. Opaque Actions/Opaque Action Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Opaque Action Example' {
	
	part def Sensor {
		attribute ready : ScalarValues::Boolean;
	}
	
	action def UpdateSensors {
		in sensors : Sensor[*];
		language "Alf" 
			/* 
			 * for (sensor in sensors) {
			 *     if (sensor.ready) {
			 *         Update(sensor);
			 *     }
			 * }
			 */
	}
	
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
