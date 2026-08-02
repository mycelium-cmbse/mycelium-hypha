---
name: Dependency Example
kind: example
language: SysML
source: sysml/src/training/37. Dependencies/Dependency Example.sysml
elements: [ItemDefinition, ItemUsage, PartUsage]
license: EPL-2.0
---

# Dependency Example

Verbatim SysML model from `sysml/src/training/37. Dependencies/Dependency Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Dependency Example' {
	
	part 'System Assembly' {
		part 'Computer Subsystem' {
			// ...
		}
		
		part 'Storage Subsystem' {
			// ...
		}
	}
	
	package 'Software Design' {
		item def MessageSchema {
			// ...
		}
		item def DataSchema {
			// ...
		}
	}
	
	dependency from 'System Assembly'::'Computer Subsystem' to 'Software Design';
	
	dependency Schemata 
		from 'System Assembly'::'Storage Subsystem' 
		to 'Software Design'::MessageSchema, 'Software Design'::DataSchema;
}
```

## Elements

- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
