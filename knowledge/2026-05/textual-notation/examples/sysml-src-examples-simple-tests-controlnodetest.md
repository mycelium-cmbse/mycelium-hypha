---
name: ControlNodeTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/ControlNodeTest.sysml
elements: [ActionDefinition, ActionUsage, FlowUsage]
license: EPL-2.0
---

# ControlNodeTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/ControlNodeTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
action def ControlNodeTest {
	action A1;
	then J;
	
	action A2 {
	    out a;
	}
	then J;
	
	flow A2.a to F.a;
	
	join J;
	then fork F {
	    in a;
	    out b1;
	    out b2;
	}
	then B1;
	then B2;
	
	flow F.b1 to B1.b;
	flow F.b2 to B2.b;
		
	action B1 {
	    in b;
	}
	then M;
	
	action B2 {
	    in b;
	}
	then M; 
	
	merge M;
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
