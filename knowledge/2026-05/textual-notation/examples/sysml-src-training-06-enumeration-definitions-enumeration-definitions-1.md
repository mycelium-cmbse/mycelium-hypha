---
name: Enumeration Definitions-1
kind: example
language: SysML
source: sysml/src/training/06. Enumeration Definitions/Enumeration Definitions-1.sysml
elements: [AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Enumeration Definitions-1

Verbatim SysML model from `sysml/src/training/06. Enumeration Definitions/Enumeration Definitions-1.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Enumeration Definitions-1' {
	private import ScalarValues::Real;
	
	enum def TrafficLightColor {
		enum green;
		enum yellow;
		enum red;
	}
	
	part def TrafficLight {
		attribute currentColor : TrafficLightColor;
	}
	
	part def TrafficLightGo specializes TrafficLight {
		attribute redefines currentColor = TrafficLightColor::green;
	}
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
