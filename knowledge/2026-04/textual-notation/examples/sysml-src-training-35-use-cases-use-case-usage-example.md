---
name: Use Case Usage Example
kind: example
language: SysML
source: sysml/src/training/35. Use Cases/Use Case Usage Example.sysml
elements: [CaseUsage, IncludeUseCaseUsage, PartDefinition, PartUsage, UseCaseUsage]
license: EPL-2.0
---

# Use Case Usage Example

Verbatim SysML model from `sysml/src/training/35. Use Cases/Use Case Usage Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Use Case Usage Example' {
	
	private import 'Use Case Definition Example'::*;
	
	part def 'Fuel Station';
	
	use case 'provide transportation' : 'Provide Transportation' {
	    subject vehicle;
	    	
		first start;
		
		then include use case 'enter vehicle' : 'Enter Vehicle' {
		    subject vehicle;
			actor driver = 'provide transportation'::driver;
			actor passengers = 'provide transportation'::passengers;
		}
		
		then use case 'drive vehicle' {
            subject vehicle;
			actor driver = 'provide transportation'::driver;
			actor environment = 'provide transportation'::environment;
			
			include 'add fuel'[0..*] { 
                subject vehicle;
				actor fueler = driver;
			}
		}
		
		then include use case 'exit vehicle' : 'Exit Vehicle' {
            subject vehicle;
			actor driver = 'provide transportation'::driver;
			actor passengers = 'provide transportation'::passengers;
		}
		
		then done;		
	}
	
	use case 'add fuel' {
		subject vehicle : Vehicle;
		actor fueler : Person;
		actor 'fuel station' : 'Fuel Station';
	}
}
```

## Elements

- [CaseUsage](../metamodel/elements/CaseUsage.md)
- [IncludeUseCaseUsage](../metamodel/elements/IncludeUseCaseUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [UseCaseUsage](../metamodel/elements/UseCaseUsage.md)
