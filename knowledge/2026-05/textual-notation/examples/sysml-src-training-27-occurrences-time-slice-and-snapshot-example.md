---
name: Time Slice and Snapshot Example
kind: example
language: SysML
source: sysml/src/training/27. Occurrences/Time Slice and Snapshot Example.sysml
elements: [AttributeDefinition, AttributeUsage, ItemDefinition, ItemUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# Time Slice and Snapshot Example

Verbatim SysML model from `sysml/src/training/27. Occurrences/Time Slice and Snapshot Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Time Slice and Snapshot Example' {
		
	attribute def Date;
	item def Person;
	
	part def Vehicle {
		timeslice assembly;
		
		first assembly then delivery;
		
		snapshot delivery {
			attribute deliveryDate : Date;
		}
		
		then timeslice ownership[0..*] ordered {
			snapshot sale = start;
			
			ref item owner : Person[1];
			
			timeslice driven[0..*] {
				ref item driver : Person[1];
			}
		}
		
		snapshot junked = done;
	}
}
```

## Elements

- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
