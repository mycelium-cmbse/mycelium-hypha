---
name: PartTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/PartTest.sysml
elements: [ActionUsage, AttributeUsage, ExhibitStateUsage, FlowUsage, ItemDefinition, ItemUsage, PartDefinition, PartUsage, PerformActionUsage, PortDefinition, PortUsage, StateUsage, SuccessionFlowUsage]
license: EPL-2.0
---

# PartTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/PartTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package PartTest {
	
	part f: A;

	public part def A {
		part <'1'> b: B;
		protected port c: C;
		constant attribute x[0..2];
		derived constant ref attribute y :> x;
		ref z : ScalarValues::Integer;
	}
	
	item def S;
	
	abstract part def <xx> B {
		public abstract part a: A[1..2];
		public abstract part b subsets a;
		public abstract part c[0..1] subsets a;
		port x: ~C {
		    port p;
		    ref port q;
		}
		package P { }
		
		succession flow x.p to a1.aa.receiver;
		
		action a1 {
			accept S via x;
			action aa accept S;
		}
		perform action a2;
		
		state s1;
		exhibit state s2;
	}
	
	private port def C {
		private in ref y: A, B {
		    part B_b redefines B::b;
		    part B_c redefines B::c;
		    port B_x redefines B::x;
		}
		alias z1 for y;
		alias z2 for y;
		port c1 : C;
		ref port c2 : C;
	}
	
    part p1 :> p2;
    part p2 :> p3; 
    part p3 :> p1;
    
    part p4 :> p4;
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ExhibitStateUsage](../metamodel/elements/ExhibitStateUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PerformActionUsage](../metamodel/elements/PerformActionUsage.md)
- [PortDefinition](../metamodel/elements/PortDefinition.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
- [StateUsage](../metamodel/elements/StateUsage.md)
- [SuccessionFlowUsage](../metamodel/elements/SuccessionFlowUsage.md)
