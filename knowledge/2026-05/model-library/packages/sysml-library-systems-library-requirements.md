---
name: Requirements
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/Requirements.sysml
declares: [Requirements, Requirements::RequirementConstraintCheck, Requirements::RequirementConstraintCheck::assumptions, Requirements::RequirementConstraintCheck::constraints, Requirements::RequirementCheck, Requirements::RequirementCheck::subj, Requirements::RequirementCheck::actors, Requirements::RequirementCheck::stakeholders, Requirements::RequirementCheck::assumptions, Requirements::RequirementCheck::constraints, Requirements::RequirementCheck::subrequirements, Requirements::RequirementCheck::concerns, Requirements::FunctionalRequirementCheck, Requirements::InterfaceRequirementCheck, Requirements::PerformanceRequirementCheck, Requirements::PhysicalRequirementCheck, Requirements::DesignConstraintCheck, Requirements::ConcernCheck, Requirements::requirementChecks, Requirements::satisfiedRequirementChecks, Requirements::notSatisfiedRequirementChecks, Requirements::concernChecks]
license: EPL-2.0
---

# Requirements

Verbatim SysML standard-library source from `sysml.library/Systems Library/Requirements.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package Requirements {
	doc
	/*
	 * This package defines the base types for requirements and related elements in the SysML language.
	 */

	private import Base::Anything;
	private import ScalarValues::String;
	private import ControlFunctions::allTrue;
	private import Constraints::constraintChecks;
	private import Constraints::assertedConstraintChecks;
	private import Constraints::negatedConstraintChecks;
	private import Parts::Part;
	private import Parts::parts;
	private import Actions::Action;
	private import Interfaces::Interface;
	private import Attributes::AttributeValue;
	
	private abstract constraint def RequirementConstraintCheck {
		doc
		/*
		 * RequirementConstraintCheck is the base ConstraintCheck for RequirementCheck, defining the
		 * separate assumptions and required constraints such that, if all the assumptions are true,
		 * then all the required constraints must be true.
		 */
	
		constraint assumptions[0..*] :> constraintChecks, subperformances {
			doc
			/*
			 * Assumptions that must hold for the required constraints to apply.
			 */
		}
		
		constraint constraints[0..*] :> constraintChecks, subperformances {
			doc
			/*
			 * The required constraints that are to be checked.
			 */
		}
		
		return result = allTrue(assumptions()) implies allTrue(constraints()) {
			doc
			/*
			 * If all the assumptions are true, then all the required constraints must hold.
			 */
		}
	}
	
	abstract requirement def RequirementCheck :> RequirementConstraintCheck {
		doc
		/*
		 * RequirementCheck is the most general class for requirements checking. RequirementsCheck is the base
		 * type of all requirement definitions.
		 */
	
		ref requirement :>> self: RequirementCheck;
		
		subject subj : Anything[1] {
			doc
			/*
			 * The entity that is being checked for satisfaction of the required constraints.
			 */
		}
		
		ref part actors : Part[0..*] {
			doc
			/*
			 * The Parts that fill the role of actors for this RequirementCheck.
			 * (Note: This is not itself an actor parameter, because specific actor
			 * parameters will be added for specific RequirementChecks.)
			 */
		}
		
		ref part stakeholders : Part[0..*] {
			doc
			/*
			 * The Parts that represent stakeholders interested in the concern being checked.
			 * (Note: This is not itself a stakeholder parameter, because specific stakeholder
			 * parameters will be added for specific RequirementChecks.)
			 */
		}

		/* 
		 * Note: assumptions and constraints are redefined here solely to simplify the
		 * resolution of their qualified names as library elements.
		 */
		constraint assumptions :>> RequirementConstraintCheck::assumptions;
		constraint constraints :>> RequirementConstraintCheck::constraints;
		
		abstract requirement subrequirements[0..*] :> requirementChecks, constraints {
			doc
			/*
			 * Nested requirements, which are also required constraints.
			 */
		}
		
		abstract concern concerns[0..*] :> concernChecks, subrequirements {
			doc
			/*
			 * The checks of any concerns being addressed (as required constraints).
			 */
		}
		
	}
	
	requirement def FunctionalRequirementCheck :> RequirementCheck {
		doc
		/*
		 * A functional requirement specifies an action that a system, or part of a system, must perform.
		 */
	
		subject: Action;
	}
	
	requirement def InterfaceRequirementCheck :> RequirementCheck {
		doc
		/*
		 * An interface requirement specifies an interface for connecting systems and system parts, which
		 * optionally may include item flows across the interface and/or interface constraints.
		 */
	
		subject: Interface;
	}
	
	requirement def PerformanceRequirementCheck :> RequirementCheck {
		doc
		/*
		 * A performance requirement quantitavely measures the extent to which a system, or a system part, 
		 * satisfies a required capability or condition.
		 */
	
		subject: AttributeValue;
	}
	
	requirement def PhysicalRequirementCheck :> RequirementCheck {
		doc
		/*
		 * A physical requirement specifies physical characteristics and/or physical constraints of the 
		 * system, or a system part.
		 */
	
		subject: Part;
	}
	
	requirement def DesignConstraintCheck :> RequirementCheck {
		doc
		/*
		 * A design constraint specifies a constraint on the implementation of the system or system part, 
		 * such as the system must use a commercial off the shelf component.
		 */
	
		subject: Part;
	}
	
	concern def ConcernCheck :> RequirementCheck {
		doc
		/*
		 * ConcernCheck is the most general class for concern checking. ConcernCheck is the base type of 
		 * all ConcernDefinitions.
		 */
	
		ref concern :>> self: ConcernCheck;
		
	}
	
	abstract requirement requirementChecks: RequirementCheck[0..*] nonunique :> constraintChecks {
		doc
		/*
		 * requirementChecks is the base feature of all requirement usages.
		 */
	}
	
	abstract requirement satisfiedRequirementChecks :> requirementChecks, assertedConstraintChecks {
		doc
		/*
		 * satisfiedRequirementChecks is the subset of requirementChecks for Requirements asserted to be satisfied.
		 */
	}

	abstract requirement notSatisfiedRequirementChecks: RequirementCheck[0..*] :> requirementChecks, negatedConstraintChecks {
		doc
		/*
		 * notSatisfiedRequirementChecks is the subset of requirementChecks for Requirements asserted to be not satisfied.
		 */
	}
	
	abstract concern concernChecks: ConcernCheck[0..*] nonunique :> requirementChecks {
		doc
		/*
		 * concernChecks is the base feature of all ConcernUsages.
		 */
	}
	
}
```

## Declarations

- `Requirements` — standard library package
- `Requirements::RequirementConstraintCheck` — constraint def
- `Requirements::RequirementConstraintCheck::assumptions` — constraint
- `Requirements::RequirementConstraintCheck::constraints` — constraint
- `Requirements::RequirementCheck` — requirement def
- `Requirements::RequirementCheck::subj` — subject
- `Requirements::RequirementCheck::actors` — part
- `Requirements::RequirementCheck::stakeholders` — part
- `Requirements::RequirementCheck::assumptions` — constraint
- `Requirements::RequirementCheck::constraints` — constraint
- `Requirements::RequirementCheck::subrequirements` — requirement
- `Requirements::RequirementCheck::concerns` — concern
- `Requirements::FunctionalRequirementCheck` — requirement def
- `Requirements::InterfaceRequirementCheck` — requirement def
- `Requirements::PerformanceRequirementCheck` — requirement def
- `Requirements::PhysicalRequirementCheck` — requirement def
- `Requirements::DesignConstraintCheck` — requirement def
- `Requirements::ConcernCheck` — concern def
- `Requirements::requirementChecks` — requirement
- `Requirements::satisfiedRequirementChecks` — requirement
- `Requirements::notSatisfiedRequirementChecks` — requirement
- `Requirements::concernChecks` — concern
