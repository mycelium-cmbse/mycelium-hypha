---
name: UseCases
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/UseCases.sysml
declares: [UseCases, UseCases::UseCase, UseCases::UseCase::self, UseCases::UseCase::subj, UseCases::UseCase::obj, UseCases::UseCase::start, UseCases::UseCase::done, UseCases::UseCase::subUseCases, UseCases::UseCase::includedUseCases, UseCases::useCases]
license: EPL-2.0
---

# UseCases

Verbatim SysML standard-library source from `sysml.library/Systems Library/UseCases.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package UseCases {
	doc
	/*
	 * This package defines the base types for use cases and related behavioral elements in the SysML language.
	 */
	 
	private import Cases::Case;
	private import Cases::cases;
	
	use case def UseCase :> Case {
		doc
		/*
		 * UseCase is the most general class of performances of UseCaseDefinitions. 
		 * UseCase is the base class of all UseCaseDefinitions.
		 */
	
		ref use case self : UseCase :>> Case::self;
		subject subj :>> Case::subj;
		objective obj :>> Case::obj;
		
		ref use case start: UseCase :>> start {
			doc
			/*
			 * The starting snapshot of a Use Case. 
			 */
		}
		
		ref use case done: UseCase :>> done {
			doc
			/*
			 * The ending snapshot of a Use Case.
			 */
		}

		abstract use case subUseCases : UseCase[0..*] :> useCases, subcases {
			doc
			/*
			 * Other UseCases carried out as part of the performance of this UseCase.
			 */
		}
		
		abstract ref use case includedUseCases : UseCase[0..*] :> useCases, enclosedPerformances {
			doc
			/*
			 * Other UseCases included by this UseCase (i.e., as modeled by an 
			 * IncludeUseCaseUsage).
			 */
		}
	}
	
	use case useCases : UseCase[0..*] nonunique :> cases {
		doc
		/*
		 * useCases is the base feature of all UseCaseUsages.
		 */
	}
}
```

## Declarations

- `UseCases` — standard library package
- `UseCases::UseCase` — use case def
- `UseCases::UseCase::self` — use case
- `UseCases::UseCase::subj` — subject
- `UseCases::UseCase::obj` — objective
- `UseCases::UseCase::start` — use case
- `UseCases::UseCase::done` — use case
- `UseCases::UseCase::subUseCases` — use case
- `UseCases::UseCase::includedUseCases` — use case
- `UseCases::useCases` — use case
