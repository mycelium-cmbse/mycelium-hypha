---
name: 14a-Language Extensions
kind: example
language: SysML
source: sysml/src/validation/14-Language Extensions/14a-Language Extensions.sysml
elements: [AttributeUsage, MetadataDefinition, MetadataUsage, PartUsage]
license: EPL-2.0
---

# 14a-Language Extensions

Verbatim SysML model from `sysml/src/validation/14-Language Extensions/14a-Language Extensions.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '14a-Language Extensions' {
	private import 'User Defined Extensions'::*;
	
	package 'User Defined Extensions' {
		
		enum def ClassificationLevel {
			uncl;
			conf;
			secret;
		}
		
		metadata def Classified {
			ref :>> annotatedElement : SysML::PartUsage;
			attribute classificationLevel : ClassificationLevel[1];
		}
	}
	
	part part_X {
		metadata Classified {
			classificationLevel = ClassificationLevel::conf;
		}
	}
	
	// Alternative shorthand notation
	part part_Y {
		@Classified {
			classificationLevel = ClassificationLevel::conf;
		}
	}

}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [MetadataDefinition](../metamodel/elements/MetadataDefinition.md)
- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
