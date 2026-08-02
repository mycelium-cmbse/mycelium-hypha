---
name: 15_12-Compound Value Type
kind: example
language: SysML
source: sysml/src/validation/15-Properties-Values-Expressions/15_12-Compound Value Type.sysml
elements: [AttributeDefinition, AttributeUsage]
license: EPL-2.0
---

# 15_12-Compound Value Type

Verbatim SysML model from `sysml/src/validation/15-Properties-Values-Expressions/15_12-Compound Value Type.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '15_12-Compound Value Type' {
	private import ScalarValues::*;
	private import USCustomaryUnits::'in';
	
	/*
	 * Real world user models would use quantity and vector types
	 * from library models. They are included here for the purpose
	 * of showing how such attribute defs can be defined.
	 */

    attribute def PositionVector {
        attribute x: Real[1];
        attribute y: Real[1];
        attribute z: Real[1];
    }
    
    attribute def LengthValue :> Real;

    attribute def TireInfo {
    	attribute manufacturer: String;
        attribute hubDiameter: LengthValue;
        attribute width: Integer;
        attribute placement: PositionVector[0..1];
    }
    
    attribute frenchTireInfo: TireInfo {
    	attribute :>> manufacturer = "Michelin";
    	attribute :>> hubDiameter = 18.0['in'];
    	attribute :>> width = 245;
    }
}
```

## Elements

- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
