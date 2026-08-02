---
name: DecisionTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/DecisionTest.sysml
elements: [ActionDefinition, ActionUsage, AttributeUsage]
license: EPL-2.0
---

# DecisionTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/DecisionTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
action def DecisionTest {
	attribute x = 1;
	
	decide 'test x';
	if x == 1 then A1; 
	if x > 1 then A2;
	else A3; 
	
	then decide D; 
	if true then A1;
	if false then A2;
	
	action A1;
	action A2;
	action A3;
	
	succession S first A1 
		if x == 0 then A2;
		
	first A3;
		if x > 0 then 'test x';
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
