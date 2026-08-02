---
name: User Keyword Example
kind: example
language: SysML
source: sysml/src/training/41. Language Extension/User Keyword Example.sysml
elements: [AttributeUsage, ConstraintUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# User Keyword Example

Verbatim SysML model from `sysml/src/training/41. Language Extension/User Keyword Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'User Keyword Example' {
	private import ScalarValues::Real;
	private import 'Semantic Metadata Example'::*;
	private import RiskMetadata::LevelEnum;
	
	part def Device {
		part battery {
			attribute power : Real;
		}
	}
	
	#scenario def DeviceFailure {
		ref device : Device;
		attribute minPower : Real;
		
		#cause 'battery old' {
			:>> probability = 0.01;			
		}
		
		#causation connect 'battery old' to 'power low';
		
		#situation 'power low' {
			constraint { device.battery.power < minPower }			
		}
		
		#causation connect 'power low' to 'device shutoff';
		
		#failure 'device shutoff' {
			:>> severity = LevelEnum::high;
		}
	}
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
