---
name: Variation Configuration
kind: example
language: SysML
source: sysml/src/training/36. Variability/Variation Configuration.sysml
elements: [PartUsage]
license: EPL-2.0
---

# Variation Configuration

Verbatim SysML model from `sysml/src/training/36. Variability/Variation Configuration.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Variation Configuration' {
	private import 'Variation Usages'::*;
	
	part vehicle4Cyl :> vehicleFamily {
		part redefines engine = engine::'4cylEngine';
		part redefines transmission = transmission::manualTransmission;
	}
	
	part vehicle6Cyl :> vehicleFamily {
		part redefines engine = engine::'6cylEngine';
		part redefines transmission = transmission::manualTransmission;
	}
	
}
```

## Elements

- [PartUsage](../metamodel/elements/PartUsage.md)
