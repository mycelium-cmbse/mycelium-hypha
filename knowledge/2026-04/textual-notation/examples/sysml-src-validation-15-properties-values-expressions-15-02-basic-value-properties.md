---
name: 15_02-Basic Value Properties
kind: example
language: SysML
source: sysml/src/validation/15-Properties-Values-Expressions/15_02-Basic Value Properties.sysml
elements: [AttributeDefinition, AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# 15_02-Basic Value Properties

Verbatim SysML model from `sysml/src/validation/15-Properties-Values-Expressions/15_02-Basic Value Properties.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '15_02-Basic Value Properties' {
	private import ScalarValues::*;
	
    attribute def LengthValue :> Real {
		doc
		/*
		 * Real world user models would use a quantity type
		 * from the library model. A attribute def is defined
		 * here to show that it is possible.
		 */
	}

    part def Tire {
    	attribute manufacturer: String;
        attribute hubDiameter: LengthValue;
        attribute width: Integer;
    }
    
    part frenchTire: Tire {
    	attribute :>> manufacturer = "Michelin";
    	attribute :>> hubDiameter = 18.0;
    	attribute :>> width = 245;
    }
    
}
```

## Elements

- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
