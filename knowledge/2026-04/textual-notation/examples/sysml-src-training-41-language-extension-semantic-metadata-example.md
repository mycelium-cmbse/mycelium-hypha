---
name: Semantic Metadata Example
kind: example
language: SysML
source: sysml/src/training/41. Language Extension/Semantic Metadata Example.sysml
elements: [MetadataDefinition, MetadataUsage]
license: EPL-2.0
---

# Semantic Metadata Example

Verbatim SysML model from `sysml/src/training/41. Language Extension/Semantic Metadata Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
library package 'Semantic Metadata Example' {
	private import 'Model Library Example'::*;
	private import Metaobjects::SemanticMetadata;

	metadata def situation :> SemanticMetadata {
		:>> baseType = situations meta SysML::Usage;
	}
	
	metadata def cause :> SemanticMetadata {
		:>> baseType = causes meta SysML::Usage;
	}
	
	metadata def failure :> SemanticMetadata {
		:>> baseType = failures meta SysML::Usage;
	}
	
	metadata def causation :> SemanticMetadata {
		:>> baseType = causations meta SysML::Usage;
	}
	
	metadata def scenario :> SemanticMetadata {
		:>> baseType = scenarios meta SysML::Usage;
	}
	
}
```

## Elements

- [MetadataDefinition](../metamodel/elements/MetadataDefinition.md)
- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
