---
name: UseCaseTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/UseCaseTest.sysml
elements: [CaseDefinition, CaseUsage, IncludeUseCaseUsage, PartDefinition, PartUsage, UseCaseDefinition, UseCaseUsage]
license: EPL-2.0
---

# UseCaseTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/UseCaseTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package UseCaseTest {

	part def System;	
	part def User;
	
	use case def UseSystem {
		subject system : System;
		actor user : User;
		
		objective  { 
			/* Goal */
		}
		
		include use case uc1 : UC1;	
		include use case uc2 {
			subject = system;
			actor user = UseSystem::user;
		}
	}
	
	use case def UC1;
	
	part user : User;
	
	use case uc2 {
	    subject;
		actor :>> user;
	}
	
	use case u : UseSystem;
	
	part system : System {
		include uc2;
		perform u;
		use case uc1 : UC1;
	}
	
	use case uc3 {
	    include u;
	    include system.uc1;
	}
	
}
```

## Elements

- [CaseDefinition](../metamodel/elements/CaseDefinition.md)
- [CaseUsage](../metamodel/elements/CaseUsage.md)
- [IncludeUseCaseUsage](../metamodel/elements/IncludeUseCaseUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [UseCaseDefinition](../metamodel/elements/UseCaseDefinition.md)
- [UseCaseUsage](../metamodel/elements/UseCaseUsage.md)
