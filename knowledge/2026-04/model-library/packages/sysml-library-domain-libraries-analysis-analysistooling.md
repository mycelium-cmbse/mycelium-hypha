---
name: AnalysisTooling
kind: model-library-file
language: SysML
source: sysml.library/Domain Libraries/Analysis/AnalysisTooling.sysml
declares: [AnalysisTooling, AnalysisTooling::ToolExecution, AnalysisTooling::ToolExecution::toolName, AnalysisTooling::ToolExecution::uri, AnalysisTooling::ToolVariable, AnalysisTooling::ToolVariable::name]
license: EPL-2.0
---

# AnalysisTooling

Verbatim SysML standard-library source from `sysml.library/Domain Libraries/Analysis/AnalysisTooling.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package AnalysisTooling {
	doc
	/*
	 * This package contains definitions for metadata annotations related
	 * to analysis tool integration.
	 */

	private import ScalarValues::*;
	
	metadata def ToolExecution {
		doc
		/*
		 * ToolExecution metadata identifies an external analysis tool to be
		 * used to implement the annotated action.
		 */
	
		attribute toolName : String;
		attribute uri : String;
	}
	
	metadata def ToolVariable {
		doc
		/*
		 * ToolVariable metadata is used in the context of an action that has
		 * been annotated with ToolExecution metadata. It is used to annotate
		 * a parameter or other feature of the action with the name of the
		 * variable in the tool that is to correspond to the annotated
		 * feature.
		 */
	
		attribute name : String;
	}
	
}
```

## Declarations

- `AnalysisTooling` — standard library package
- `AnalysisTooling::ToolExecution` — metadata def
- `AnalysisTooling::ToolExecution::toolName` — attribute
- `AnalysisTooling::ToolExecution::uri` — attribute
- `AnalysisTooling::ToolVariable` — metadata def
- `AnalysisTooling::ToolVariable::name` — attribute
