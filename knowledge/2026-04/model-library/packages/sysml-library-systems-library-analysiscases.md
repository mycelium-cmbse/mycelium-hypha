---
name: AnalysisCases
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/AnalysisCases.sysml
declares: [AnalysisCases, AnalysisCases::subj]
license: EPL-2.0
---

# AnalysisCases

Verbatim SysML standard-library source from `sysml.library/Systems Library/AnalysisCases.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package AnalysisCases {
	doc
	/*
	 * This package defines the base types for analysis cases and related behavioral elements 
	 * in the SysML language.
	 */

	private import Performances::Evaluation;
	private import Performances::evaluations;
	private import Calculations::Calculation;
	private import Cases::Case;
	private import Cases::cases;
	
	abstract analysis def AnalysisCase :> Case {
		doc
		/*
		 * AnalysisCase is the most general class of performances of AnalysisCaseDefinitions. 
		 * AnalysisCase is the base class of all AnalysisCaseDefinitions.
		 */
	
		ref analysis self : AnalysisCase :>> Case::self;		
		subject subj :>> Case::subj;
		
		abstract analysis subAnalysisCases : AnalysisCase[0..*] :> analysisCases, subcases {
			doc
			/*
			 * Other AnalysisCases carried out as part of the performance of this AnalysisCase.
			 */
		}
	}
	
	abstract analysis analysisCases : AnalysisCase[0..*] nonunique :> cases {
		doc
		/*
		 * analysisCases is the base feature of all AnalysisCaseUsages.
		 */
	}
}
```

## Declarations

- `AnalysisCases` — standard library package
- `AnalysisCases::subj` — subject
