---
name: 15_08-Range Restriction
kind: example
language: SysML
source: sysml/src/validation/15-Properties-Values-Expressions/15_08-Range Restriction.sysml
elements: [AssertConstraintUsage, AttributeDefinition, AttributeUsage, ConstraintUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# 15_08-Range Restriction

Verbatim SysML model from `sysml/src/validation/15-Properties-Values-Expressions/15_08-Range Restriction.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '15_08-Range Restriction' {
	private import ISQ::*;
	private import SI::*;
	private import '15_01-Constants'::'Mathematical Constants'::pi;
	
	part def HeadLightsTiltKnob {
		attribute headLightsTile : LightBeamTiltAngleValue[1];
	}
	
	attribute def LightBeamTiltAngleValue :> PlaneAngleValue {
		attribute angle: LightBeamTiltAngleValue :>> self {
			doc
			/*
			 * Tilt angle shall be limited to the range between 50 and 80 degrees (inclusive).
			 */
		}
		assert constraint { angle >= 50 ['°'] and angle <= 80 ['°'] }
	}
}
```

## Elements

- [AssertConstraintUsage](../metamodel/elements/AssertConstraintUsage.md)
- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
