---
name: Enumeration Definitions-2
kind: example
language: SysML
source: sysml/src/training/06. Enumeration Definitions/Enumeration Definitions-2.sysml
elements: [AttributeDefinition, AttributeUsage]
license: EPL-2.0
---

# Enumeration Definitions-2

Verbatim SysML model from `sysml/src/training/06. Enumeration Definitions/Enumeration Definitions-2.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Enumeration Definitions-2' {
	private import ScalarValues::*;
	private import 'Enumeration Definitions-1'::*;
	
	attribute def ClassificationLevel {
		attribute code : String;
		attribute color : TrafficLightColor;
	}
	
	enum def ClassificationKind specializes ClassificationLevel {
		unclassified {
			:>> code = "uncl";
			:>> color = TrafficLightColor::green;
		}
		confidential {
			:>> code = "conf";
			:>> color = TrafficLightColor::yellow;
		}
		secret {
			:>> code = "secr";
			:>> color = TrafficLightColor::red;
		}
	}
	
	enum def GradePoints :> Real {
		A = 4.0;
		B = 3.0;
		C = 2.0;
		D = 1.0;
		F = 0.0;
	}
}
```

## Elements

- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
