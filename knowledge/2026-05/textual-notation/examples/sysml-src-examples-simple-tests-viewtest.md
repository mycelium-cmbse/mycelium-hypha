---
name: ViewTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/ViewTest.sysml
elements: [ConcernDefinition, ConcernUsage, PartDefinition, PartUsage, RenderingDefinition, RenderingUsage, ViewDefinition, ViewUsage, ViewpointDefinition, ViewpointUsage]
license: EPL-2.0
---

# ViewTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/ViewTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ViewTest {
	package P {
		public part p1;
		private part p2;
	}
	
	part def S;
	
	concern def C {
	    subject;
		stakeholder s : S;
	}
	
	concern c : C {
	    subject;
		stakeholder s1;
	}
	
	viewpoint def VP {
		frame c;
	}
	
	rendering def R;
	
	rendering r : R;
	
	view def V {
		viewpoint vp: VP {
			frame concern c1;
			concern c2;
		}
		render rendering r1: R[0..1]; 
		
		view v: V[0..*] {
			expose P::*;
			render r;
			
			rendering r2;
			
			alias vp1 for p1;
			// Note: "expose" imports all.
			alias vp2 for p2;
		}
	}

}
```

## Elements

- [ConcernDefinition](../metamodel/elements/ConcernDefinition.md)
- [ConcernUsage](../metamodel/elements/ConcernUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [RenderingDefinition](../metamodel/elements/RenderingDefinition.md)
- [RenderingUsage](../metamodel/elements/RenderingUsage.md)
- [ViewDefinition](../metamodel/elements/ViewDefinition.md)
- [ViewUsage](../metamodel/elements/ViewUsage.md)
- [ViewpointDefinition](../metamodel/elements/ViewpointDefinition.md)
- [ViewpointUsage](../metamodel/elements/ViewpointUsage.md)
