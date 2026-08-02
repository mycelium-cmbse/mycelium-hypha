---
name: TextualRepresentationTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/TextualRepresentationTest.sysml
elements: [ActionDefinition, ActionUsage, AssertConstraintUsage, AttributeUsage, ConstraintUsage, ItemDefinition, ItemUsage]
license: EPL-2.0
---

# TextualRepresentationTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/TextualRepresentationTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package TextualRepresentationTest {
	private import ScalarValues::Real;
	
	item def C {
	    attribute x: Real;
	    assert constraint x_constraint {
		    rep inOCL language "ocl" 
		        /* self.x > 0.0 */
	    }
	}
	
	action def setX {
		in c : C;
		in newX : Real;
		
	    language "alf" 
	        /* c.x = newX;
	         * WriteLine("Set new x");
	         */
	}
	
}
```

## Elements

- [ActionDefinition](../metamodel/elements/ActionDefinition.md)
- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AssertConstraintUsage](../metamodel/elements/AssertConstraintUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
