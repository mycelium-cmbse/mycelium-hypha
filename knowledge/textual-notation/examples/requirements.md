---
name: Requirements
kind: example
language: SysML
constructs: ["requirement def", subject, "require constraint", "assume constraint", doc]
elements: [RequirementDefinition, SubjectMembership, ConstraintUsage, RequirementConstraintMembership, Documentation, Subclassification, AttributeUsage]
source: "sources/textual/sysml/training/32. Requirements/Requirement Definitions.sysml"
license: EPL-2.0
---

# Requirements

A worked SysML v2 textual-notation example: requirement definitions with a subject, documentation,
required and assumed constraints, and specialization between requirements.

## Notation

```sysml
package 'Requirement Definitions' {
	private import ISQ::*;
	private import SI::*;

	requirement def MassLimitationRequirement {
		doc /* The actual mass shall be less than or equal to the required mass. */
		
		attribute massActual: MassValue;
		attribute massReqd: MassValue;
		
		require constraint { massActual <= massReqd }
	}
	
	part def Vehicle {
		attribute dryMass: MassValue;
		attribute fuelMass: MassValue;
		attribute fuelFullMass: MassValue;
	}
	
	requirement def <'1'> VehicleMassLimitationRequirement :> MassLimitationRequirement {
		doc /* The total mass of a vehicle shall be less than or equal to the required mass. */
		
		subject vehicle : Vehicle;
		
		attribute redefines massActual = vehicle.dryMass + vehicle.fuelMass;
		
		assume constraint { vehicle.fuelMass > 0[kg] }
	}
	
	port def ClutchPort;
	action def GenerateTorque;
	
	requirement def <'2'> DrivePowerInterface {
		doc /* The engine shall transfer its generated torque to the transmission via the clutch interface. */
		subject clutchPort: ClutchPort;
	}
		
	requirement def <'3'> TorqueGeneration {
		doc /* The engine shall generate torque as a function of RPM as shown in Table 1. */
		subject generateTorque: GenerateTorque;
	}
}
```

## Constructs & elements

- `requirement def { … }` – [RequirementDefinition](../../metamodel/elements/RequirementDefinition.md): a constraint-based definition of a requirement.
- `subject x : T` – [SubjectMembership](../../metamodel/elements/SubjectMembership.md): the thing the requirement is about.
- `require constraint { … }` – a required [ConstraintUsage](../../metamodel/elements/ConstraintUsage.md) carried by a [RequirementConstraintMembership](../../metamodel/elements/RequirementConstraintMembership.md) (kind `requirement`).
- `assume constraint { … }` – an assumed constraint (a `RequirementConstraintMembership` of kind `assumption`).
- `doc /* … */` – [Documentation](../../metamodel/elements/Documentation.md) attached to the element.
- `<'1'>` – a declared short name (identifier) for the requirement.
- `:> MassLimitationRequirement` – [Subclassification](../../metamodel/elements/Subclassification.md): a requirement specializing another.
- `attribute redefines massActual = …` – a redefining [AttributeUsage](../../metamodel/elements/AttributeUsage.md) with a bound value.

## Source

`sources/textual/sysml/training/32. Requirements/Requirement Definitions.sysml` – from
Systems-Modeling/SysML-v2-Release (EPL-2.0); see [NOTICE](../../../NOTICE).
