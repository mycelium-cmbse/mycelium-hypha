---
name: RiskMetadataExample
kind: example
language: SysML
source: sysml/src/examples/Metadata Examples/RiskMetadataExample.sysml
elements: [PartUsage]
license: EPL-2.0
---

# RiskMetadataExample

Verbatim SysML model from `sysml/src/examples/Metadata Examples/RiskMetadataExample.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package RiskMetadataExample {
	private import RiskMetadata::*;
	private import RiskLevelEnum::*;
	
    part engine4cyl{
        @Risk {
            totalRisk = high;
            technicalRisk = medium;
            scheduleRisk = medium;
        }
        @Risk {
        	totalRisk { 
        		probability = 0.3;
        		impact = 0.7;
        	}        	
        }
    }
        
}
```

## Elements

- [PartUsage](../metamodel/elements/PartUsage.md)
