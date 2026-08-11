---
name: VerificationCases
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/VerificationCases.sysml
declares: [VerificationCases, VerificationCases::subj, VerificationCases::obj, VerificationCases::obj::subj, VerificationCases::obj::requirementVerifications, VerificationCases::requirementVerifications, VerificationCases::VerdictKind, VerificationCases::PassIf, VerificationCases::PassIf::isPassing, VerificationCases::PassIf::verdict, VerificationCases::VerificationMethod, VerificationCases::VerificationMethod::kind, VerificationCases::VerificationMethodKind]
license: EPL-2.0
---

# VerificationCases

Verbatim SysML standard-library source from `sysml.library/Systems Library/VerificationCases.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package VerificationCases {
	doc
	/*
	 * This package defines the base types for verification cases and related behavioral elements 
	 * in the SysML language.
	 */

	private import Cases::Case;
	private import Cases::cases;
	private import Requirements::RequirementCheck;
	private import ScalarValues::Boolean;
	
	abstract verification def VerificationCase :> Case {
		doc
		/*
		 * VerificationCase is the most general class of performances of VerificationCaseDefinitions. 
		 * VericationCase is the base class of all VerificationCaseDefinitions.
		 */
	
		ref verification self : VerificationCase :>> Case::self;		
		subject subj :>> Case::subj;
		return verdict : VerdictKind :>> result;
		
		objective obj :>> Case::obj {
			subject subj = VerificationCase::subj;
			
			requirement requirementVerifications : RequirementCheck[0..*] :> subrequirements {
				doc
				/*
				 * A record of the evaluations of the RequirementChecks of requirements being verified.
				 */
			}
		}
		
		ref requirement requirementVerifications : RequirementCheck[0..*] = obj.requirementVerifications {
			doc
			/*
			 * Checks on whether the verifiedRequirements of the VerificationCase have been satisfied.
			 */
		}
		
		abstract verification subVerificationCases : VerificationCase[0..*] :> verificationCases, subcases {
			doc
			/*
			 * The subcases of this VerificationCase that are VerificationCaseUsages.
			 */
		}
		
	}
	
	abstract verification verificationCases : VerificationCase[0..*] nonunique  :> cases {
		doc
		/*
		 * verificationCases is the base feature of all VerificationCaseUsages.
		 */
	}
	
	enum def VerdictKind {
		doc
		/*
		 * VerdictKind is an enumeration of the possible results of a VerificationCase.
		 */
	
		pass;
		fail;
		inconclusive;
		error;
	}
	
	calc def PassIf {
		doc
		/*
		 * PassIf returns a pass or fail VerdictKind depending on whether its argument is
		 * true or false.
		 */
	
		in attribute isPassing : Boolean;
		return attribute verdict : VerdictKind = if isPassing? VerdictKind::pass else VerdictKind::fail;
	}
	
	metadata def VerificationMethod {
		doc
		/*
		 * VerificationMethod can be used as metadata annotating a verification case or action.
		 */
	
		attribute kind : VerificationMethodKind[1..*];
	}
	
	enum def VerificationMethodKind {
		doc
		/*
		 * VerificationMethodKind is an enumeration of the standard methods by which verification
		 * can be carried out.
		 */
	
		inspect;
		analyze;
		demo;
		test;
	}
	
}
```

## Declarations

- `VerificationCases` — standard library package
- `VerificationCases::subj` — subject
- `VerificationCases::obj` — objective
- `VerificationCases::obj::subj` — subject
- `VerificationCases::obj::requirementVerifications` — requirement
- `VerificationCases::requirementVerifications` — requirement
- `VerificationCases::VerdictKind` — enum def
- `VerificationCases::PassIf` — calc def
- `VerificationCases::PassIf::isPassing` — attribute
- `VerificationCases::PassIf::verdict` — attribute
- `VerificationCases::VerificationMethod` — metadata def
- `VerificationCases::VerificationMethod::kind` — attribute
- `VerificationCases::VerificationMethodKind` — enum def
