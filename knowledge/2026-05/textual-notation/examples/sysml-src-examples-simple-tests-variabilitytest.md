---
name: VariabilityTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/VariabilityTest.sysml
elements: [ActionDefinition, ActionUsage, AttributeDefinition, AttributeUsage, CaseUsage, PartDefinition, PartUsage, RequirementUsage, UseCaseUsage]
license: EPL-2.0
---

# VariabilityTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/VariabilityTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package VariabilityTest {
	part def P {
		attribute a;
	}
	
	part def Q :> P;
	attribute def B;
	variation part def V :> P {
		variant part x : Q {
			attribute b : B :>> a;
		}
	}
	
	part q : Q;
	variation part v : P {
		variant q {
			attribute b : B :>> a;
		}
	}
	
	part y : P = v::q;
	
	variation action def A {
		variant action a1;
		variant action a2;
	}
	
	variation use case uc1 {
    	variant use case uc11;
    	variant use case uc12;
    }

    variation analysis a1;
    
    variation verification v1;
    
    variation requirement r {
    	variant requirement r1;
    }
	
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [CaseUsage](../metamodel/elements/CaseUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
- [UseCaseUsage](../metamodel/elements/UseCaseUsage.md)
