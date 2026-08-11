---
name: RiskMetadata
kind: model-library-file
language: SysML
source: sysml.library/Domain Libraries/Metadata/RiskMetadata.sysml
declares: [RiskMetadata, RiskMetadata::Level, RiskMetadata::LevelEnum, RiskMetadata::RiskLevel, RiskMetadata::RiskLevel::probability, RiskMetadata::RiskLevel::impact, RiskMetadata::RiskLevelEnum, RiskMetadata::Risk, RiskMetadata::Risk::totalRisk, RiskMetadata::Risk::technicalRisk, RiskMetadata::Risk::scheduleRisk, RiskMetadata::Risk::costRisk]
license: EPL-2.0
---

# RiskMetadata

Verbatim SysML standard-library source from `sysml.library/Domain Libraries/Metadata/RiskMetadata.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package RiskMetadata {
	doc
	/*
	 * This package defines metadata for annotating model elements with assessments of risk.
	 */

	private import ScalarValues::Real;
	
	attribute def Level :> Real {
		doc
		/*
		 * A Level is a Real number in the interval 0.0 to 1.0, inclusive.
		 */
	
		assert constraint { that >= 0.0 and that <= 1.0 }
	}
	
	enum def LevelEnum :> Level {
		doc
		/*
		 * LevelEnum provides standard probability Levels for low, medium and high risks.
		 */
	
		low = 0.25;
		medium = 0.50;
		high = 0.75;
	}

	attribute def RiskLevel {
		doc
		/*
		 * RiskLevel gives the probability of a risk occurring and, optionally, the impact
		 * if the risk occurs.
		 */
	
		attribute probability : Level {
			doc
			/*
			 * The probability that a risk will occur.
			 */
		}
		
		attribute impact : Level [0..1] {
			doc
			/*
			 * The impact of the risk if it occurs (with 0.0 being no impact and 1.0 being 
			 * the most severe impact).
			 */
		}
	}
	
	enum def RiskLevelEnum :> RiskLevel {
		doc
		/*
		 * RiskLevelEnum enumerates standard RiskLevels for low, medium and high risks
		 * (without including impact).
		 */

		low = new RiskLevel(probability = LevelEnum::low);
		medium = new RiskLevel(probability = LevelEnum::medium);
		high = new RiskLevel(probability = LevelEnum::high);
	}
	
	metadata def Risk {
		doc
		/*
		 * Risk is used to annotate a model element with an assessment of the risk related to it
		 * in some typical risk areas.
		 */
	
		attribute totalRisk : RiskLevel [0..1] {
			doc
			/*
			 * The total risk associated with the annotated element.
			 */
		}
		
		attribute technicalRisk : RiskLevel [0..1] {
			doc
			/*
			 * The risk of unresolved technical issues regarding the annotated element.
			 */
		}
		
		attribute scheduleRisk : RiskLevel [0..1] {
			doc
			/*
			 * The risk that work on the annotated element will not be completed on schedule.
			 */
		}
		
		attribute costRisk : RiskLevel [0..1] {
			doc
			/*
			 * The risk that work on the annotated element will exceed its planned cost.
			 */
		}
	}
	
}
```

## Declarations

- `RiskMetadata` — standard library package
- `RiskMetadata::Level` — attribute def
- `RiskMetadata::LevelEnum` — enum def
- `RiskMetadata::RiskLevel` — attribute def
- `RiskMetadata::RiskLevel::probability` — attribute
- `RiskMetadata::RiskLevel::impact` — attribute
- `RiskMetadata::RiskLevelEnum` — enum def
- `RiskMetadata::Risk` — metadata def
- `RiskMetadata::Risk::totalRisk` — attribute
- `RiskMetadata::Risk::technicalRisk` — attribute
- `RiskMetadata::Risk::scheduleRisk` — attribute
- `RiskMetadata::Risk::costRisk` — attribute
