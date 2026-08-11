---
name: CausationConnections
kind: model-library-file
language: SysML
source: sysml.library/Domain Libraries/Cause and Effect/CausationConnections.sysml
declares: [CausationConnections, CausationConnections::causes, CausationConnections::effects, CausationConnections::Multicausation, CausationConnections::Multicausation::causes, CausationConnections::Multicausation::effects, CausationConnections::Multicausation::disjointCauseEffect, CausationConnections::Multicausation::causalOrdering, CausationConnections::Multicausation::causalOrdering::nCauses, CausationConnections::Multicausation::causalOrdering::nEffects, CausationConnections::multicausations, CausationConnections::Causation, CausationConnections::Causation::theCause, CausationConnections::Causation::theEffect, CausationConnections::causations]
license: EPL-2.0
---

# CausationConnections

Verbatim SysML standard-library source from `sysml.library/Domain Libraries/Cause and Effect/CausationConnections.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package CausationConnections {	
	doc 
	/* 
	 * This package provides a library model modeling causes, effects, and causation connections 
	 * between them.
	 */

	private import SequenceFunctions::isEmpty;
	private import SequenceFunctions::size;
	private import SequenceFunctions::intersection;
		 
	abstract occurrence causes[*] {
		doc /* Occurrences that are causes. */
	}
	
	abstract occurrence effects[*]  {
		doc /* Occurrences that are effects. */
	}
	
	abstract connection def Multicausation {
		doc
		/*
		 * A Multicausation connection models the situation in which one set of
		 * occurrences causes another.
		 * 
		 * To create a Multicausation connection, specialize this connection definition
		 * adding specific end features of the relavent types. Ends representing causes
		 * should subset 'causes', while ends representing effects should subset 'effects'.
		 * There must be at least one cause and at least one effect.
		 */
		 
		abstract constant ref occurrence causes[1..*] :>> causes :> participant {
			doc 
			/* 
			 * The causing occurrences. (Constant for each Multicausation instance.)
			 */
		}
		abstract constant ref occurrence effects[1..*] :>> effects :> participant {
			doc 
			/* 
			 * The effect occurrences caused by the causing occurrences. 
			 * (Constant for each Multicausation instance.)
			 */
		}
		
		private assert constraint disjointCauseEffect {
			doc /* causes must be disjoint from effects. */
			isEmpty(intersection(causes, effects))
		}
		
		private succession causalOrdering first [nCauses] causes.startShot then [nEffects] effects {
			doc /* All causes must exist before all effects. */
			attribute nCauses = size(causes);
			attribute nEffects = size(effects);
		}
	}
	
	abstract connection multicausations : Multicausation[*] {
		doc /* multicausations is the base feature for Multicausation ConnectionUsages. */
	}
	
	connection def Causation :> Multicausation {
		doc
		/*
		 * A Causation is a binary Multicausation in which a single cause occurrence
		 * causes a single effect occurrence. (However, a single cause can separately
		 * have multiple effects, and a single effect can have separate Causation
		 * connections with multiple causes.)
		 */
		
		end theCauses [*] occurrence theCause :> causes :>> source {
		    doc /* The single causing occurrence. */
		}
		
		end theEffects [*] occurrence theEffect :> effects :>> target {
			doc /* The single effect occurrence resulting from the cause. */
		}
	}
	
	abstract connection causations : Causation[*] :> multicausations {
		doc /* causations is the base feature for Causation ConnectionUsages. */
	}
}
```

## Declarations

- `CausationConnections` — standard library package
- `CausationConnections::causes` — occurrence
- `CausationConnections::effects` — occurrence
- `CausationConnections::Multicausation` — connection def
- `CausationConnections::Multicausation::causes` — occurrence
- `CausationConnections::Multicausation::effects` — occurrence
- `CausationConnections::Multicausation::disjointCauseEffect` — assert constraint
- `CausationConnections::Multicausation::causalOrdering` — succession
- `CausationConnections::Multicausation::causalOrdering::nCauses` — attribute
- `CausationConnections::Multicausation::causalOrdering::nEffects` — attribute
- `CausationConnections::multicausations` — connection
- `CausationConnections::Causation` — connection def
- `CausationConnections::Causation::theCause` — occurrence
- `CausationConnections::Causation::theEffect` — occurrence
- `CausationConnections::causations` — connection
