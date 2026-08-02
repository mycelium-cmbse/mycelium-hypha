---
name: Metadata Example-1
kind: example
language: SysML
source: sysml/src/training/39. Metadata/Metadata Example-1.sysml
elements: [MetadataDefinition, MetadataUsage, PartUsage]
license: EPL-2.0
---

# Metadata Example-1

Verbatim SysML model from `sysml/src/training/39. Metadata/Metadata Example-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Metadata Example-1' {
	
	metadata def SafetyFeature;
	metadata def SecurityFeature {
		:> annotatedElement : SysML::PartDefinition;
		:> annotatedElement : SysML::PartUsage;
	}
	
	metadata SafetyFeature about 
		vehicle::interior::seatBelt,
		vehicle::interior::driverAirBag,
		vehicle::bodyAssy::bumper;
	
	metadata SecurityFeature about
		vehicle::interior::alarm,
		vehicle::bodyAssy::keylessEntry;
		
	part vehicle {
		part interior {
			part alarm;
			part seatBelt[2];
			part frontSeat[2];
			part driverAirBag;
		}
		part bodyAssy {
			part body;
			part bumper;
			part keylessEntry;
		}
	}
	
}
```

## Elements

- [MetadataDefinition](../metamodel/elements/MetadataDefinition.md)
- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
