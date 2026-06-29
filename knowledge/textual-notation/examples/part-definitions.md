---
name: Part and attribute definitions
kind: example
language: SysML
constructs: ["part def", "attribute def", attribute, part, "ref part"]
elements: [PartDefinition, AttributeDefinition, AttributeUsage, PartUsage, ReferenceUsage]
source: "sources/textual/sysml/training/02. Part Definitions/Part Definition Example.sysml"
license: EPL-2.0
---

# Part and attribute definitions

A worked SysML v2 textual-notation example: defining parts and value attributes, and using them as
features (including a reference part).

## Notation

```sysml
package 'Part Definition Example' {
	private import ScalarValues::*;

	part def Vehicle {
		attribute mass : Real;
		attribute status : VehicleStatus;

		part eng : Engine;

		ref part driver : Person;
	}

	attribute def VehicleStatus {
		attribute gearSetting : Integer;
		attribute acceleratorPosition : Real;
	}

	part def Engine;
	part def Person;
}
```

## Constructs & elements

- `part def` — [PartDefinition](../../sysml2/elements/PartDefinition.md): definition of a kind of part.
- `attribute def` — [AttributeDefinition](../../sysml2/elements/AttributeDefinition.md): definition of a kind of value.
- `attribute x : T` — [AttributeUsage](../../sysml2/elements/AttributeUsage.md): a value-typed feature.
- `part eng : Engine` — [PartUsage](../../sysml2/elements/PartUsage.md): a composite part feature.
- `ref part driver : Person` — [ReferenceUsage](../../sysml2/elements/ReferenceUsage.md): a part feature held by reference, not composition.

## Source

`sources/textual/sysml/training/02. Part Definitions/Part Definition Example.sysml` — from
Systems-Modeling/SysML-v2-Release (EPL-2.0); see [NOTICE](../../../NOTICE).
