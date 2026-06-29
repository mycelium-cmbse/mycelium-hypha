---
name: Generalization and specialization
kind: example
language: SysML
constructs: [abstract, specializes, ":>", "multiple specialization"]
elements: [PartDefinition, Subclassification, Specialization, ReferenceUsage]
source: "sources/textual/sysml/training/03. Generalization/Generalization Example.sysml"
license: EPL-2.0
---

# Generalization and specialization

A worked SysML v2 textual-notation example: abstract definitions and single/multiple specialization,
in both the `specializes` and `:>` forms.

## Notation

```sysml
package 'Generalization Example' {

	abstract part def Vehicle;

	part def HumanDrivenVehicle specializes Vehicle {
		ref part driver : Person;
	}

	part def PoweredVehicle :> Vehicle {
		part eng : Engine;
	}

	part def HumanDrivenPoweredVehicle :>
		HumanDrivenVehicle, PoweredVehicle;

	part def Engine;
	part def Person;
}
```

## Constructs & elements

- `abstract part def` — an abstract [PartDefinition](../../sysml2/elements/PartDefinition.md) (cannot be instantiated directly).
- `specializes` / `:>` — [Subclassification](../../sysml2/elements/Subclassification.md), a kind of [Specialization](../../sysml2/elements/Specialization.md): the subtype inherits the supertype's features.
- `:> A, B` — multiple specialization: several `Subclassification` relationships on one definition.
- `ref part` — [ReferenceUsage](../../sysml2/elements/ReferenceUsage.md).

## Source

`sources/textual/sysml/training/03. Generalization/Generalization Example.sysml` — from
Systems-Modeling/SysML-v2-Release (EPL-2.0); see [NOTICE](../../../NOTICE).
