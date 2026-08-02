---
name: RationaleMetadataExample
kind: example
language: SysML
source: sysml/src/examples/Metadata Examples/RationaleMetadataExample.sysml
elements: [MetadataUsage, PartUsage]
license: EPL-2.0
---

# RationaleMetadataExample

Verbatim SysML model from `sysml/src/examples/Metadata Examples/RationaleMetadataExample.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package RationaleMetadataExample {
	private import ModelingMetadata::Rationale;
	
    /* Example: the following provides the rationale for selecting the engine4cyl based on a trade study analysis. 
    The rationale could be contained in the vehicle configuration with the selected engine */
    
    part engine;
    part engine4cyl :> engine;
    part engine6cyl :> engine;
    
    metadata engineSelectionRationale : Rationale about engine4cyl {
    	text = "This rationale for selecting the engine4cyl refers to the engineTradeOffAnalysis.";
    	explanation = engineTradeOffAnalysis;
    }
    
    private import TradeStudies::*;
    analysis engineTradeOffAnalysis:TradeStudy{
        subject alternatives :> engine [2] = (engine4cyl, engine6cyl);

        /* ... */
        
        return selectedEngine :> engine;
     }
}
```

## Elements

- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
