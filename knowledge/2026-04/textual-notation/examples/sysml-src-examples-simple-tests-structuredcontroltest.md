---
name: StructuredControlTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/StructuredControlTest.sysml
elements: [ActionUsage, AttributeUsage]
license: EPL-2.0
---

# StructuredControlTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/StructuredControlTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package StructuredControlTest {
	
	action {
		attribute i : ScalarValues::Integer := 0;
		attribute b : ScalarValues::Boolean;
		
		if i < 0 {
			assign i := 0;
		} else if i == 0 {
			assign i := 1;
		} else {
			assign i := i + 1;
		}
		
		if i > 0 {
			assign i := i + 1;
		}
		
		then action aLoop
		while i > 0 {
			assign i := i - 1;
		} until b;
		
		then while i > 0 {
			assign i := i - 1;
		}
		
		loop {
			assign i := i - 1;
		} until b;
				
		for n : ScalarValues::Integer in (1, 2, 3) {
			assign i := i * n;
		}
	}
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
