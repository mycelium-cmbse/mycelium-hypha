---
name: Time Constraints
kind: example
language: SysML
source: sysml/src/training/31. Constraints/Time Constraints.sysml
elements: [AssertConstraintUsage, AttributeDefinition, AttributeUsage, ConstraintUsage, PartDefinition, PartUsage, StateUsage]
license: EPL-2.0
---

# Time Constraints

Verbatim SysML model from `sysml/src/training/31. Constraints/Time Constraints.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Time Constraints' {
	private import ISQ::TemperatureValue;
	private import ISQ::DurationValue;
	private import Time::TimeInstantValue;
	private import Time::TimeOf;
	private import Time::DurationOf;
	private import SI::h;
	private import SI::s;

	attribute def MaintenanceDone;
	
	part def Vehicle {
		attribute maintenanceTime : TimeInstantValue;
		attribute maintenanceInterval : DurationValue;
		attribute maxTemperature : TemperatureValue;
	}
	
	state healthStates {
		in vehicle : Vehicle;
		
		entry; then normal;
		
		state normal;
		accept at vehicle.maintenanceTime
			then maintenance;
		
		state maintenance {
			assert constraint { TimeOf(maintenance) > vehicle.maintenanceTime }
			assert constraint { TimeOf(maintenance) - TimeOf(normal.done) < 2 [s] }
			entry assign vehicle.maintenanceTime := vehicle.maintenanceTime + vehicle.maintenanceInterval;
		}
		accept MaintenanceDone
			then normal;
		
		constraint { DurationOf(maintenance) <= 48 [h] }
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
- [StateUsage](../metamodel/elements/StateUsage.md)
