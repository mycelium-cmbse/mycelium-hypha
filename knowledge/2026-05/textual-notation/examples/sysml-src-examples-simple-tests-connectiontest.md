---
name: ConnectionTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/ConnectionTest.sysml
elements: [ConnectionDefinition, ConnectionUsage, FlowDefinition, FlowUsage, ItemUsage, MetadataDefinition, MetadataUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# ConnectionTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/ConnectionTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ConnectionTest {
	
	part p {
		part x {
			part x1;
		}
	}
	
	part def P {
		part y;

		connect p to y;
		
		part p1 :> p;
	
		connect p1.x to y;
		connect p1.x.x1 to y;
		
		part a;
		part b;
		
		bind a = b;
		binding ab bind a = b;
		binding ab1 : AB bind a = b;
		
		first a then b;
		succession s first a then b;
		succession s1 : AB first a then b;
	}

	abstract connection def C {
		part p;
		end end1;
		end end2;
		end end3;
	}
	
	part d1;
	part d2;
	part d3;
	part d4;
	
	connection bus : C connect (d1, d2, d3, d4);
	
	connection : C {
	    end :>> end1 ::> d1;
	    end end2 ::> d2;
	    end end3 ::> d3;
	}
	
	connection {
		part q;
		end ref end1 ::> d1 :> q;
		end end2 ::> d2;
	}
	
	abstract flow def F;
	
	message : F from p to p;
	
	part def A {
	    ref b : B;
	}
	
	part def B;
	
	connection def AB {
	    end [1] item a : A {
	    	@M;
	    }
	    end b : B;
	}
	
	metadata def M;
	
	
}
```

## Elements

- [ConnectionDefinition](../metamodel/elements/ConnectionDefinition.md)
- [ConnectionUsage](../metamodel/elements/ConnectionUsage.md)
- [FlowDefinition](../metamodel/elements/FlowDefinition.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [MetadataDefinition](../metamodel/elements/MetadataDefinition.md)
- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
