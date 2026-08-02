---
name: AliasImport
kind: example
language: SysML
source: sysml/src/examples/Import Tests/AliasImport.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# AliasImport

Verbatim SysML model from `sysml/src/examples/Import Tests/AliasImport.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package AliasImport {
	package Definitions {
	    part def Vehicle;
	    
	    alias Car for Vehicle;
	}
	
	package Usages {
	    private import Definitions::Car;
	
	    part vehicle : Car;
	}
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
