---
name: RequirementMetadataExample
kind: example
language: SysML
source: sysml/src/examples/Metadata Examples/RequirementMetadataExample.sysml
elements: [ConstraintUsage, MetadataDefinition, MetadataUsage, RequirementDefinition, RequirementUsage]
license: EPL-2.0
---

# RequirementMetadataExample

Verbatim SysML model from `sysml/src/examples/Metadata Examples/RequirementMetadataExample.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package RequirementMetadataExample {
	private import Metaobjects::SemanticMetadata;
	private import ModelingMetadata::*;
	private import RiskMetadata::*;
	private import RiskLevelEnum::*;
	
	requirement def Goal;
	requirement goals : Goal[*] nonunique;
	metadata def goal :> SemanticMetadata {
	    :>> baseType = goals meta SysML::RequirementUsage;
	}

    requirement <'1'> vehicleMassRequirement {
        doc /* The total mass of a vehicle shall be less than or equal to the required mass. */
 
        @StatusInfo {
            status = StatusKind::tbd;
            risk {
		    	totalRisk = high;
		    	technicalRisk = medium;
		    	scheduleRisk = low;
		    	costRisk = medium;
		    }            
		    originator = "Bob";
            owner = "Mary";
        }
    }
    
    #goal requirement deliverPayload {
    	assume #goal constraint payloadMassLimit;
    	require #goal vehicleMassRequirement;
    }
    
}
```

## Elements

- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [MetadataDefinition](../metamodel/elements/MetadataDefinition.md)
- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
- [RequirementDefinition](../metamodel/elements/RequirementDefinition.md)
- [RequirementUsage](../metamodel/elements/RequirementUsage.md)
