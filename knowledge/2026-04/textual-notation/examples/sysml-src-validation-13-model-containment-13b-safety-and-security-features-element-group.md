---
name: 13b-Safety and Security Features Element Group
kind: example
language: SysML
source: sysml/src/validation/13-Model Containment/13b-Safety and Security Features Element Group.sysml
elements: [PartUsage]
license: EPL-2.0
---

# 13b-Safety and Security Features Element Group

Verbatim SysML model from `sysml/src/validation/13-Model Containment/13b-Safety and Security Features Element Group.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '13b-Safety and Security Features Element Group' {
	
	part vehicle1_c1 {
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
	
	package 'Safety Features' {
		/* Parts that contribute to safety. */
		
		public import vehicle1_c1::interior::seatBelt;
		public import vehicle1_c1::interior::driverAirBag;
		public import vehicle1_c1::bodyAssy::bumper;		
	}
	
	package 'Security Features' {
		/* Parts that contribute to security. */
		
		public import vehicle1_c1::interior::alarm;
		public import vehicle1_c1::bodyAssy::keylessEntry;
	}
	
	package 'Safety & Security Features' {
		/* Parts that contribute to safety AND
		 * parts that contribute to security.
		 */
		 
		public import 'Safety Features'::*;
		public import 'Security Features'::*;
	}
}
```

## Elements

- [PartUsage](../metamodel/elements/PartUsage.md)
