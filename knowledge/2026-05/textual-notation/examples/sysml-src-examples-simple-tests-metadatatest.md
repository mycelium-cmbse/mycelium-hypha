---
name: MetadataTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/MetadataTest.sysml
elements: [MetadataDefinition, MetadataUsage]
license: EPL-2.0
---

# MetadataTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/MetadataTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package MetadataTest {
	private import 'User Defined Extensions'::*;
	
	library package 'User Defined Extensions' {
		
		#Security enum def ClassificationLevel :> ScalarValues::Natural {
			uncl : ClassificationLevel = 0;
			conf : ClassificationLevel = 1;
			#Security enum secret : ClassificationLevel = 2;
		}
		
		metadata def Classified {
			ref :>> annotatedElement : SysML::Usage;
			ref classificationLevel : ClassificationLevel;
		}
		
		metadata def Security;
	}
	
	ref x {
		metadata Classified {
			classificationLevel = ClassificationLevel::conf;
		}
	}
	
	ref y {
		@Classified {
			classificationLevel = ClassificationLevel::conf;
		}
		@Security;
	}
	
	private ref #Classified #Security z1;
	abstract #Classified z2;
	
	ref z {
	    #Security #Classified metadata Classified {
	        classificationLevel = ClassificationLevel::secret;
	    }
	}	
	
}
```

## Elements

- [MetadataDefinition](../metamodel/elements/MetadataDefinition.md)
- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
