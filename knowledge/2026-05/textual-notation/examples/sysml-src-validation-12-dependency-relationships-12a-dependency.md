---
name: 12a-Dependency
kind: example
language: SysML
source: sysml/src/validation/12-Dependency Relationships/12a-Dependency.sysml
elements: [AttributeUsage]
license: EPL-2.0
---

# 12a-Dependency

Verbatim SysML model from `sysml/src/validation/12-Dependency Relationships/12a-Dependency.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '12a-Dependency' {
	
	package 'Application Layer';
	package 'Service Layer';
	package 'Data Layer';
	
	dependency Use from 'Application Layer' to 'Service Layer';
	dependency from 'Service Layer' to 'Data Layer';
	
	attribute x;
	attribute y;
	attribute z;
	
	dependency z to x, y;
	
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
