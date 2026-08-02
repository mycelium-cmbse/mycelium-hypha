---
name: IssueMetadataExample
kind: example
language: SysML
source: sysml/src/examples/Metadata Examples/IssueMetadataExample.sysml
elements: [InterfaceDefinition, InterfaceUsage, MetadataUsage, PartUsage, PortDefinition, PortUsage]
license: EPL-2.0
---

# IssueMetadataExample

Verbatim SysML model from `sysml/src/examples/Metadata Examples/IssueMetadataExample.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package IssueMetadataExample {
	private import ModelingMetadata::Issue;
	
    //Example: the following identifies an issue with the interface
    
    metadata InterfaceCompatibilityIssue : Issue about engineToTransmissionInterface {
    	text = "This issue is about the interface compatability between the engine and transmission." +
               "The interface def includes an end defined by a ClutchPort." +
               "However, the interface usage connects the transmission port that is defined by ~DrivePwrPort." +
               "This should have surfaced a compatibility issue, since the interface is not really compatible with its definition";
    }
    
    interface def EngineToTransmissionInterface{
        end p1:DrivePwrPort;
        end p2:ClutchPort;
    }
    port def DrivePwrPort;
    port def ClutchPort;
    
    part engine{
        port drivePwrPort:DrivePwrPort;
    }
    part transmission{
        port clutchPort:~DrivePwrPort;
    }

    interface engineToTransmissionInterface:EngineToTransmissionInterface
        connect engine.drivePwrPort to transmission.clutchPort;       

}
```

## Elements

- [InterfaceDefinition](../metamodel/elements/InterfaceDefinition.md)
- [InterfaceUsage](../metamodel/elements/InterfaceUsage.md)
- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PortDefinition](../metamodel/elements/PortDefinition.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
