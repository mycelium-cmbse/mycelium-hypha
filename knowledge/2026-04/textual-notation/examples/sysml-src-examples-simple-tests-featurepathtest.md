---
name: FeaturePathTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/FeaturePathTest.sysml
elements: [AttributeUsage, FlowUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# FeaturePathTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/FeaturePathTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package Q {
  part def F {
  	part a : A;
  }
  
  part f : F;
  
  part def A {
    part g = f.a;
  }
  
  part def B {
  	part f : F;
  	part a : A;
  }
  
  part def C {
	part b : B {
	  connect f.a to a.g;
	  bind f.a = a.g;
	}
  
	part c subsets b.f {
	  	part aa subsets a;
	}
	
	flow b.f.a to c.aa;
  }
  
  part e1 {
  	attribute x : E;
  	// Ensure that "e1" resolves correctly.
  	bind e1.x = E::e2;
  }
  
  enum def E {
  	enum e1;
  	enum e2;
  }
  
  part g = new A().g.g.g;
	
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
