---
name: 11b-Safety and Security Feature Views
kind: example
language: SysML
source: sysml/src/validation/11-View and Viewpoint/11b-Safety and Security Feature Views.sysml
elements: [AttributeUsage, MetadataDefinition, MetadataUsage, PartUsage, ViewDefinition, ViewUsage]
license: EPL-2.0
---

# 11b-Safety and Security Feature Views

Verbatim SysML model from `sysml/src/validation/11-View and Viewpoint/11b-Safety and Security Feature Views.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
private import Views::*; // private import library package, not internal Views package!
package '11b-Safety and Security Feaure Views' {
	private import ScalarValues::*;
	
	package AnnotationDefinitions {	
		metadata def Safety {
			attribute isMandatory : Boolean;
		}
		metadata def Security;
	}
	
	package PartsTree {
		public import AnnotationDefinitions::*;
		part vehicle {
			part interior {
				part alarm {@Security;}
				part seatBelt[2] {@Safety{isMandatory = true;}}
				part frontSeat[2];
				part driverAirBag {@Safety{isMandatory = false;}}
			}
			part bodyAssy {
				part body;
				part bumper {@Safety{isMandatory = true;}}
				part keylessEntry {@Security;}
			}
			part wheelAssy {
				part wheel[2];
				part antilockBrakes[2] {@Safety{isMandatory = false;}}
			}
		}
	}

	package ViewDefinitions {	
		public import AnnotationDefinitions::*;
		view def SafetyFeatureView {
			/* Parts that contribute to safety. */		
			filter @Safety;
			render asTreeDiagram;
		}
		
		view def SafetyOrSecurityFeatureView {
			/* Parts that contribute to safety OR security. */		 
			filter @Safety | @Security;
		}	
	}
	
	package Views {
		private import ViewDefinitions::*;
		private import PartsTree::vehicle;
		
		view vehicleSafetyFeatureView : SafetyFeatureView {
			expose vehicle;
		}
		
		view vehicleMandatorySafetyFeatureView :> vehicleSafetyFeatureView {
		    expose vehicle::*::**;
			filter @Safety and (as Safety).isMandatory;
		}
		
		view vehicleMandatorySafetyFeatureViewStandalone {
			expose vehicle::**[@Safety and (as Safety).isMandatory];
			render asElementTable;
		}	
	}
	
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [MetadataDefinition](../metamodel/elements/MetadataDefinition.md)
- [MetadataUsage](../metamodel/elements/MetadataUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [ViewDefinition](../metamodel/elements/ViewDefinition.md)
- [ViewUsage](../metamodel/elements/ViewUsage.md)
