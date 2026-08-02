---
name: 11a-View-Viewpoint
kind: example
language: SysML
source: sysml/src/validation/11-View and Viewpoint/11a-View-Viewpoint.sysml
elements: [AttributeUsage, ConcernUsage, PartDefinition, PartUsage, ViewUsage, ViewpointUsage]
license: EPL-2.0
---

# 11a-View-Viewpoint

Verbatim SysML model from `sysml/src/validation/11-View and Viewpoint/11a-View-Viewpoint.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '11a-View-Viewpoint' {
	
	package SystemModel {
		private import SI::*;
		
		part def Vehicle;
		part def AxleAssembly;
		part def Axle;
		part def Wheel;
		
		part vehicle : Vehicle {
			attribute mass :> ISQ::mass = 2500[SI::kg];
			part frontAxleAssembly : AxleAssembly[1] {
				attribute mass :> ISQ::mass = 150[kg];
				part frontWheel : Wheel[2];
				part frontAxle : Axle[1] {
					attribute mass;
					attribute steeringAngle;
				}
			}
			part rearAxleAssembly : AxleAssembly[1] {
				attribute mass :> ISQ::mass = 250[kg];
				part rearWheel : Wheel[2];
				part rearAxle : Axle[1] {
					attribute mass;
				}
			}
		}
		
	}
	
	package ViewModel {
		private import Views::*;
	
		part 'systems engineer';
		
		concern 'system breakdown' {
			subject;
			stakeholder :>> 'systems engineer';
		}
		
		viewpoint 'system structure perspective' {		
			frame 'system breakdown';
		}
		
		view 'system structure generation' {
			satisfy 'system structure perspective';
			expose SystemModel::vehicle::**[@SysML::PartUsage];
			render asElementTable {
				view :>> columnView[1] {
					render asTextualNotation;
				}
			}
		}
	
	}
}
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConcernUsage](../metamodel/elements/ConcernUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [ViewUsage](../metamodel/elements/ViewUsage.md)
- [ViewpointUsage](../metamodel/elements/ViewpointUsage.md)
