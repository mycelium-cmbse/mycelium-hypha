---
name: Comment Example
kind: example
language: SysML
source: sysml/src/training/01. Packages/Comment Example.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Comment Example

Verbatim SysML model from `sysml/src/training/01. Packages/Comment Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Comment Example' {
	/* This is a comment, which is a part of the model, 
	 * annotating (by default) it's owning namespace. */
	
	comment Comment1 /* This is a named comment. */
	
	comment about Automobile
	/* This is an unnamed comment, annotating an 
	 * explicitly specified element. 
	 */
	 
	part def Automobile;
	
	alias Car for Automobile {
		/*
		 * This is a comment annotating its owning
		 * element.
		 */
	}	                         
	
	// This is a note. It is in the text, but not part 
	// of the model.
	alias Torque for ISQ::TorqueValue;
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
