---
name: ConstraintTest
kind: example
language: SysML
source: sysml/src/examples/Simple Tests/ConstraintTest.sysml
elements: [AssertConstraintUsage, AttributeUsage, ConstraintDefinition, ConstraintUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# ConstraintTest

Verbatim SysML model from `sysml/src/examples/Simple Tests/ConstraintTest.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ConstraintTest {
	private import ISQ::MassValue;
	private import SI::kg;
	private import NumericalFunctions::sum;
	
	constraint def MassAnalysis {
		attribute totalMass: MassValue;
		attribute componentMasses: MassValue[0..*];		

		totalMass == sum(componentMasses)
	}
	
	part def Component {
		attribute mass: MassValue;
	}
	
	part vehicle : Component {	
		part engine : Component;
		part frontAxleAssembly : Component;
		part rearAxleAssembly : Component;	
	}
		
	part vehicle1a :> vehicle {
		assert constraint massAnalysis : MassAnalysis {
			attribute redefines totalMass;
			attribute redefines componentMasses;
		}
		
		bind massAnalysis.totalMass = mass;
		bind massAnalysis.componentMasses = engine.mass;
		bind massAnalysis.componentMasses = frontAxleAssembly.mass;
		bind massAnalysis.componentMasses = rearAxleAssembly.mass;
	}
	
	part vehicle1b :> vehicle {		
		assert constraint massAnalysis : MassAnalysis {
			attribute redefines totalMass = mass;
			attribute redefines componentMasses = (engine.mass, frontAxleAssembly.mass, rearAxleAssembly.mass);		
		}	
	}
		
	constraint def MassAnalysis2 { 
		in totalMass : MassValue;
		in componentMasses: MassValue[0..*];
		
		totalMass == sum(componentMasses)
	}
	
	part vehicle2a :> vehicle {
		assert constraint massConstraint : MassAnalysis2;
		
		bind massConstraint.totalMass = mass;
		bind massConstraint.componentMasses = engine.mass;
		bind massConstraint.componentMasses = frontAxleAssembly.mass;
		bind massConstraint.componentMasses = rearAxleAssembly.mass;
	}
		
	part vehicle2b :> vehicle {
		assert constraint massAnalysis2 : MassAnalysis2 {
			in totalMass = mass;
			in componentMasses = (engine.mass, frontAxleAssembly.mass, rearAxleAssembly.mass);
		}
	}
	
	constraint def MassAnalysis3 {
		in totalMass : MassValue;
		in componentMasses: MassValue[0..*];
	}
	
	constraint massAnalysis3 : MassAnalysis3 {
		in totalMass : MassValue;
		in componentMasses: MassValue[0..*];
		
		totalMass == sum(componentMasses)
	}
	
	part vehicle3 :> vehicle {
		assert massAnalysis3 {
			in totalMass = mass;
			in componentMasses = (engine.mass, frontAxleAssembly.mass, rearAxleAssembly.mass);
		}
	}
	
	part vehicle4 :> vehicle {
		assert constraint { mass == engine.mass + frontAxleAssembly.mass + rearAxleAssembly.mass }
	}
	
	constraint massLimitation { mass : MassValue; massLimit : MassValue; mass < massLimit }
	assert not massLimitation { :>> mass = vehicle3.mass; :>> massLimit = vehicle4.mass; }
}
```

## Elements

- [AssertConstraintUsage](../metamodel/elements/AssertConstraintUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ConstraintDefinition](../metamodel/elements/ConstraintDefinition.md)
- [ConstraintUsage](../metamodel/elements/ConstraintUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
