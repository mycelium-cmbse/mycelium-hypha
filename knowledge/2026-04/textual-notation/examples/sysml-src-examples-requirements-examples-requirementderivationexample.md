---
name: RequirementDerivationExample
kind: example
language: SysML
source: sysml/src/examples/Requirements Examples/RequirementDerivationExample.sysml
elements: [ConnectionDefinition, ConnectionUsage, PartDefinition, PartUsage, RequirementDefinition, RequirementUsage, SatisfyRequirementUsage]
license: EPL-2.0
---

# RequirementDerivationExample

Verbatim SysML model from `sysml/src/examples/Requirements Examples/RequirementDerivationExample.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package RequirementDerivationExample {
	private import RequirementDerivation::*;
	
	requirement def Req1;
	
	requirement def Req1_1;
	requirement def Req1_2;
	
	#derivation connection def Req1_Derivation {
		end #original r1 : Req1;
		end #derive r1_1 : Req1_1;
		end #derive r1_2 : Req1_2;
	}
	
	part def System;
	part def Subsystem1;
	part def Subsystem2;
	
	part system : System {
		part sub1 : Subsystem1;
		part sub2 : Subsystem2;
	}
	
	part satisfactionContext {
		ref :>> system;
		
		satisfy requirement req1 : Req1 by system;
		satisfy requirement req1_1 : Req1_1 by system.sub1;
		satisfy requirement req1_2 : Req1_2 by system.sub2;
		
		#derivation connection : Req1_Derivation {
			end r1 ::> req1;
			end r1_1 ::> req1_1;
			end r1_2 ::> req1_1;
		}
		
	}
	
}
```

## Elements

- [ConnectionDefinition](../metamodel/elements/ConnectionDefinition.md)
- [ConnectionUsage](../metamodel/elements/ConnectionUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [RequirementDefinition](../metamodel/elements/RequirementDefinition.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
- [SatisfyRequirementUsage](../metamodel/elements/SatisfyRequirementUsage.md)
