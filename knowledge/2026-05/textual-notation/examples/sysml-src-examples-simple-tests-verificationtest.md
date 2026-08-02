---
name: VerificationTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/VerificationTest.sysml
elements: [PartDefinition, PartUsage, RequirementDefinition, RequirementUsage]
license: EPL-2.0
---

# VerificationTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/VerificationTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package VerificationTest {

	part def V {
		m : ScalarValues::Integer;
	}
	
	part vv : V;
	
	requirement def R {
		doc /* ... */
	}
	
	requirement r : R;

	verification def VerificationCase {		
		subject v : V;	
		objective {
			verify requirement : R;
		}
		
		VerificationCases::PassIf(v.m == 0)
	}
	
	verification def VerificationPlan {
		subject v : V;
		
		objective {
			verify r;
		}
		
		verification verificationCase : VerificationCase;
	}
	
	part verificationContext {
		verification verificationPlan : VerificationPlan {
			subject v = vv;
		}
	}
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [RequirementDefinition](../metamodel/elements/RequirementDefinition.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
