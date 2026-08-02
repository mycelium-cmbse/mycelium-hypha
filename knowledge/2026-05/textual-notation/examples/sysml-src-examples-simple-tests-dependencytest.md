---
name: DependencyTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/DependencyTest.sysml
elements: [AttributeUsage]
license: EPL-2.0
---

# DependencyTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/DependencyTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package DependencyTest {
	
	package System {
		package 'Application Layer';
		package 'Service Layer';
		package 'Data Layer';
	}
	
	private import System::*;
	
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
