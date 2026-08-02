---
name: Use Case Definition Example
kind: example
language: SysML
source: sysml/src/training/35. Use Cases/Use Case Definition Example.sysml
elements: [CaseDefinition, CaseUsage, PartDefinition, PartUsage, UseCaseDefinition, UseCaseUsage]
license: EPL-2.0
---

# Use Case Definition Example

Verbatim SysML model from `sysml/src/training/35. Use Cases/Use Case Definition Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Use Case Definition Example' {
	
	part def Vehicle;
	part def Person;
	part def Environment;
	part def 'Fuel Station';
	
	use case def 'Provide Transportation' {
		subject vehicle : Vehicle;
		
		actor driver : Person;
		actor passengers : Person[0..4];
		actor environment : Environment;
		
		objective {
			doc 
			/* Transport driver and passengers from starting location 
			 * to ending location.
			 */
		}		
	}
	
	use case def 'Enter Vehicle' {
		subject vehicle : Vehicle;
		actor driver : Person;
		actor passengers : Person[0..4];
	}
	
	use case def 'Exit Vehicle' {
		subject vehicle : Vehicle;
		actor driver : Person;
		actor passengers : Person[0..4];
	}
}
```

## Elements

- [CaseDefinition](../metamodel/elements/CaseDefinition.md)
- [CaseUsage](../metamodel/elements/CaseUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [UseCaseDefinition](../metamodel/elements/UseCaseDefinition.md)
- [UseCaseUsage](../metamodel/elements/UseCaseUsage.md)
