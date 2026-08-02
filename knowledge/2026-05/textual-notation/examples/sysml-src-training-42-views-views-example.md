---
name: Views Example
kind: example
language: SysML
source: sysml/src/training/42. Views/Views Example.sysml
elements: [RenderingUsage, ViewDefinition, ViewUsage]
license: EPL-2.0
---

# Views Example

Verbatim SysML model from `sysml/src/training/42. Views/Views Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Views Example' {
	private import Views::*;
	private import 'Viewpoint Example'::*;
	private import 'Filtering Example-2'::*;
	
	view def 'Part Structure View' {
		satisfy 'system structure perspective';		
		filter @SysML::PartUsage;
	}
	
	view 'vehicle structure view' : 'Part Structure View' {
		expose vehicle::**;
		render asTreeDiagram;
	}
	
	rendering asTextualNotationTable :> asElementTable {
		view :>> columnView[1] {
			render asTextualNotation;
		}
	}

	view 'vehicle tabular views' {
		
		view 'safety features view' : 'Part Structure View' {
			expose vehicle::**[@Safety];
			render asTextualNotationTable;
		}
		
		view 'non-safety features view' : 'Part Structure View' {
			expose vehicle::**[not (@Safety)];
			render asTextualNotationTable;
		}
	}
	
}
```

## Elements

- [RenderingUsage](../metamodel/elements/RenderingUsage.md)
- [ViewDefinition](../metamodel/elements/ViewDefinition.md)
- [ViewUsage](../metamodel/elements/ViewUsage.md)
