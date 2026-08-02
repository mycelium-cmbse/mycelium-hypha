---
name: VerificationMetadataExample
kind: example
language: SysML
source: sysml/src/examples/Metadata Examples/VerificationMetadataExample.sysml
elements: [ActionUsage]
license: EPL-2.0
---

# VerificationMetadataExample

Verbatim SysML model from `sysml/src/examples/Metadata Examples/VerificationMetadataExample.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package VerificationMetadataExample {
	private import VerificationCases::*;
	private import VerificationMethodKind::*;
	
    verification def MassTest;
    verification massTests:MassTest {
        @VerificationMethod{ kind = (test,demo); }
        objective {
        }
        action weighVehicle {
        	@VerificationMethod{ kind = analyze; }
        }
    }
	
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
