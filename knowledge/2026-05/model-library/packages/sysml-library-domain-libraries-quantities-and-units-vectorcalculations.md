---
name: VectorCalculations
kind: model-library-file
language: SysML
source: sysml.library/Domain Libraries/Quantities and Units/VectorCalculations.sysml
declares: [VectorCalculations, VectorCalculations::[, VectorCalculations::[::n, VectorCalculations::isZeroVectorQuantity, VectorCalculations::isUnitVectorQuantity, VectorCalculations::+, VectorCalculations::-, VectorCalculations::scalarVectorMult, VectorCalculations::vectorScalarMult, VectorCalculations::scalarQuantityVectorMult, VectorCalculations::vectorScalarQuantityMult, VectorCalculations::vectorScalarDiv, VectorCalculations::vectorScalarQuantityDiv, VectorCalculations::inner, VectorCalculations::outer, VectorCalculations::norm, VectorCalculations::angle, VectorCalculations::transform]
license: EPL-2.0
---

# VectorCalculations

Verbatim SysML standard-library source from `sysml.library/Domain Libraries/Quantities and Units/VectorCalculations.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package VectorCalculations {
	doc
	/*
	 * This package package defines calculations for the construction of and computations on VectorQuantityValues.
	 */
	 
	private import ScalarValues::Boolean;
	private import ScalarValues::Number;
    private import Quantities::ScalarQuantityValue;
    private import Quantities::VectorQuantityValue;
    private import MeasurementReferences::VectorMeasurementReference;
    private import MeasurementReferences::CoordinateTransformation;
    
    calc def '[' :> BaseFunctions::'[' { 
    	in elements: Number[1..n] ordered nonunique; 
    	in mRef: VectorMeasurementReference[1]; 
    	return quantity : VectorQuantityValue[1];
    	private attribute n = mRef.flattenedSize;
    }

    calc def isZeroVectorQuantity :> VectorFunctions::isZeroVector { 
    	in : VectorQuantityValue[1]; 
    	return : Boolean[1];
    }
    calc def isUnitVectorQuantity { 
    	in : VectorQuantityValue[1]; 
    	return : Boolean[1];
    }

    /* Addition and subtraction */
	calc def '+' :> VectorFunctions::'+' { in : VectorQuantityValue[1]; in : VectorQuantityValue[1]; return : VectorQuantityValue[1]; }
	calc def '-' :> VectorFunctions::'-' { in : VectorQuantityValue[1]; in : VectorQuantityValue[1]; return : VectorQuantityValue[1]; }

    /* Multiplication and division */
	calc def scalarVectorMult :> VectorFunctions::scalarVectorMult { in : Number[1]; in : VectorQuantityValue[1]; return : VectorQuantityValue[1]; }
	calc def vectorScalarMult :> VectorFunctions::vectorScalarMult { in : VectorQuantityValue[1]; in : Number[1]; return : VectorQuantityValue[1]; }
    calc def scalarQuantityVectorMult { in : ScalarQuantityValue[1]; in : VectorQuantityValue[1]; return : VectorQuantityValue[1]; }
    calc def vectorScalarQuantityMult { in : VectorQuantityValue[1]; in : ScalarQuantityValue[1]; return : VectorQuantityValue[1]; }
	calc def vectorScalarDiv :> VectorFunctions::vectorScalarDiv { in : VectorQuantityValue[1]; in : Number[1]; return : VectorQuantityValue[1]; }
    calc def vectorScalarQuantityDiv { in : VectorQuantityValue[1]; in : ScalarQuantityValue[1]; return : VectorQuantityValue[1]; }
	calc def inner :> VectorFunctions::inner { in : VectorQuantityValue[1]; in : VectorQuantityValue[1]; return : Number[1]; }
    calc def outer { in : VectorQuantityValue[1]; in : VectorQuantityValue[1]; return : VectorQuantityValue[1]; }
	
    alias '*' for scalarVectorMult;
    
    /* Norm and angle */
	calc def norm :> VectorFunctions::norm { in : VectorQuantityValue[1]; return : Number[1]; }
	calc def angle :> VectorFunctions::angle { in : VectorQuantityValue[1]; in : VectorQuantityValue[1]; return : Number[1]; }
	
	/* Coordinate transformation */
	calc def transform {
	    in transformation : CoordinateTransformation;
	    in sourceVector : VectorQuantityValue {
	        :>> mRef = transformation.source;
	    }
	    return targetVector : VectorQuantityValue {
	        :>> mRef = transformation.target {
	            :>> dimensions = sourceVector.mRef.dimensions;
	        }
    	}
	}
}
```

## Declarations

- `VectorCalculations` — standard library package
- `VectorCalculations::[` — calc def
- `VectorCalculations::[::n` — attribute
- `VectorCalculations::isZeroVectorQuantity` — calc def
- `VectorCalculations::isUnitVectorQuantity` — calc def
- `VectorCalculations::+` — calc def
- `VectorCalculations::-` — calc def
- `VectorCalculations::scalarVectorMult` — calc def
- `VectorCalculations::vectorScalarMult` — calc def
- `VectorCalculations::scalarQuantityVectorMult` — calc def
- `VectorCalculations::vectorScalarQuantityMult` — calc def
- `VectorCalculations::vectorScalarDiv` — calc def
- `VectorCalculations::vectorScalarQuantityDiv` — calc def
- `VectorCalculations::inner` — calc def
- `VectorCalculations::outer` — calc def
- `VectorCalculations::norm` — calc def
- `VectorCalculations::angle` — calc def
- `VectorCalculations::transform` — calc def
