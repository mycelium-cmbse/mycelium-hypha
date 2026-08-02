---
name: Metadata Example-2
kind: example
language: SysML
source: sysml/src/training/39. Metadata/Metadata Example-2.sysml
elements: [ActionUsage, MetadataUsage]
license: EPL-2.0
---

# Metadata Example-2

Verbatim SysML model from `sysml/src/training/39. Metadata/Metadata Example-2.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Metadata Example-2' {
	
	action computeDynamics {
		private import AnalysisTooling::*;
		
		metadata ToolExecution {
			toolName = "ModelCenter";
			uri = "aserv://localhost/Vehicle/Equation1";
		}
			
		in dt : ISQ::TimeValue             { @ToolVariable { name = "deltaT"; } }
		in a : ISQ::AccelerationValue      { @ToolVariable { name = "mass"; } }
		in v_in : ISQ::SpeedValue          { @ToolVariable { name = "v0"; } }
		in x_in : ISQ::LengthValue         { @ToolVariable { name = "x0"; } }
		
		out v_out : ISQ::SpeedValue        { @ToolVariable { name = "v"; } }
		out x_out : ISQ::LengthValue       { @ToolVariable { name = "x"; } }			
	}
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
