---
name: StateTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/StateTest.sysml
elements: [ActionUsage, AttributeDefinition, AttributeUsage, PartUsage, StateDefinition, StateUsage, TransitionUsage]
license: EPL-2.0
---

# StateTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/StateTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package StateTest {
	attribute def Sig {
		x;
	}
	attribute def Exit;
	
	part p;
	
	action act;
	
	state def S {
		do action A;
		entry; then S1;
		
		state S1;
			accept s : Sig
			do action D
			then S2;
				
		state S2 {
			do send new Sig(T.s.x) to p;
			state S3;
		}
		accept Exit then done;
		
		transition
			first S1
			accept s : Sig
			do action D
			then S2.S3;
		
		transition T
			first S2.S3
			accept s : Sig via p
			if true
			do send s to p
			then S1;
			
		exit act;
		
		state S3 {
			state S3a;
		}
		
		transition first S3.S3a then S1; 
	}
	
	state s0 {
  		state s1 {
    		state s2;
  		}
  		state s3 {
  			state s4;
  		}
  		transition t1 first s1.s2 then s3.s4;
	}
	
	state s parallel {
		state s1;
		state s2;
	}
	
	state s4 {
		do action a;
  		action c;
	}
	
	state s5 :> s4 {
  		do action b :>> c;
	}
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [StateDefinition](../metamodel/elements/StateDefinition.md)
- [StateUsage](../metamodel/elements/StateUsage.md)
- [TransitionUsage](../metamodel/elements/TransitionUsage.md)
