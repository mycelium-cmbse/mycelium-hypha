---
name: Viewpoint Example
kind: example
language: SysML
source: sysml/src/training/42. Views/Viewpoint Example.sysml
elements: [ConcernUsage, ConstraintUsage, PartDefinition, PartUsage, ViewUsage, ViewpointUsage]
license: EPL-2.0
---

# Viewpoint Example

Verbatim SysML model from `sysml/src/training/42. Views/Viewpoint Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Viewpoint Example' {	
	part def 'Systems Engineer';
	part def 'IV&V';
	
	concern 'system breakdown' {
		doc /* 
		 * To ensure that a system covers all its required capabilities,
		 * it is necessary to understand how it is broken down into
		 * subsystems and components that provide those capabilities.
		 */
		subject;
		stakeholder se : 'Systems Engineer';
		stakeholder ivv : 'IV&V';
	}
	
	concern 'modularity' {
		doc /*
		 * There should be well defined interfaces between the parts of
		 * a system that allow each part to be understood individually,
		 * as well as being part of the whole system.
		 */		 
        subject;
		stakeholder se : 'Systems Engineer';
	}
	
	viewpoint 'system structure perspective' {		
		frame 'system breakdown';
		frame 'modularity';
		
		require constraint {
			doc /*
			 * A system structure view shall show the hierarchical 
			 * part decomposition of a system, starting with a 
			 * specified root part.
			 */
		}
	}
}
```

## Elements

- [ConcernUsage](../metamodel/elements/ConcernUsage.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [ViewUsage](../metamodel/elements/ViewUsage.md)
- [ViewpointUsage](../metamodel/elements/ViewpointUsage.md)
