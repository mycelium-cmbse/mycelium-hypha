---
name: AnalysisTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/AnalysisTest.sysml
elements: [PartDefinition, PartUsage, RequirementDefinition, RequirementUsage]
license: EPL-2.0
---

# AnalysisTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/AnalysisTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package AnalysisTest {

	part def V {
		m;
	}
	
	part vv : V;
	
	requirement def AnalysisObjective {
		doc /* ... */
	}

	analysis def AnalysisCase {
		subject v : V;
		
		objective obj : AnalysisObjective { 
			subject = result;
		}
		
		v.m
	}
	
	analysis def AnalysisPlan {
		subject v : V;
		
		objective {
			doc /* ... */
		}
		
		analysis analysisCase : AnalysisCase { return mass; }
	}
	
	part analysisContext {
		analysis analysisPlan : AnalysisPlan {
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
