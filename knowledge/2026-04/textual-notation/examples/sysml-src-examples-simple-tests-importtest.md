---
name: ImportTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/ImportTest.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# ImportTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/ImportTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ImportTest {
    package Pkg1 {
    	private import Pkg2::Pkg21::Pkg211::P211;
    	private import Pkg2::Pkg21::*;
    	private import Pkg211::*::**;
        part p11 : Pkg211::P211;
        part def P12;
    }

    package Pkg2 {
        private import Pkg1::*;
        package Pkg21 {
        	package Pkg211 {
        		part def P211 :> P12;
        	}
        }
    }
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
