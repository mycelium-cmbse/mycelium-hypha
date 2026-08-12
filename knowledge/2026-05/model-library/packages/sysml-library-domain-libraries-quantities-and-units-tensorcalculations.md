---
name: TensorCalculations
kind: model-library-file
language: SysML
source: sysml.library/Domain Libraries/Quantities and Units/TensorCalculations.sysml
declares: [TensorCalculations, TensorCalculations::[, TensorCalculations::[::n, TensorCalculations::isZeroTensorQuantity, TensorCalculations::isUnitTensorQuantity, TensorCalculations::+, TensorCalculations::-, TensorCalculations::scalarTensorMult, TensorCalculations::TensorScalarMult, TensorCalculations::scalarQuantityTensorMult, TensorCalculations::TensorScalarQuantityMult, TensorCalculations::tensorVectorMult, TensorCalculations::vectorTensorMult, TensorCalculations::tensorTensorMult, TensorCalculations::transform]
license: EPL-2.0
---

# TensorCalculations

Verbatim SysML standard-library source from `sysml.library/Domain Libraries/Quantities and Units/TensorCalculations.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package TensorCalculations {
	doc
	/*
	 * This package package defines calculations for the construction of and computations on TensorQuantityValues.
	 */
	 
    private import ScalarValues::Boolean;
    private import ScalarValues::Number;
    private import Quantities::ScalarQuantityValue;
    private import Quantities::VectorQuantityValue;
    private import Quantities::TensorQuantityValue;
    private import MeasurementReferences::TensorMeasurementReference;
    private import MeasurementReferences::CoordinateTransformation;
    
    calc def '[' specializes BaseFunctions::'[' { 
    	in elements: Number[1..n] ordered nonunique; 
    	in mRef: TensorMeasurementReference[1]; 
    	return quantity: TensorQuantityValue[1];
    	private attribute n = mRef.flattenedSize;
    }

    calc def isZeroTensorQuantity { 
    	in x : TensorQuantityValue[1]; 
    	return : Boolean[1];
    }
    calc def isUnitTensorQuantity { 
    	in x : TensorQuantityValue[1]; 
    	return : Boolean[1];
    }

    /* Addition and subtraction */
    calc def '+' :> DataFunctions::'+' { in : TensorQuantityValue[1]; in : TensorQuantityValue[1]; return : TensorQuantityValue[1]; }
    calc def '-' :> DataFunctions::'-' { in : TensorQuantityValue[1]; in : TensorQuantityValue[1]; return : TensorQuantityValue[1]; }

    /* Multiplication and division */
    calc def scalarTensorMult { in : Number[1]; in : TensorQuantityValue[1]; return : TensorQuantityValue[1]; }
    calc def TensorScalarMult { in : TensorQuantityValue[1]; in : Number[1]; return : TensorQuantityValue[1]; }
    calc def scalarQuantityTensorMult { in : ScalarQuantityValue[1]; in : TensorQuantityValue[1]; return : TensorQuantityValue[1]; }
    calc def TensorScalarQuantityMult { in : TensorQuantityValue[1]; in : ScalarQuantityValue[1]; return : TensorQuantityValue[1]; }
    calc def tensorVectorMult { in : TensorQuantityValue[1]; in : VectorQuantityValue[1]; return : VectorQuantityValue[1]; }
    calc def vectorTensorMult { in : VectorQuantityValue[1]; in : TensorQuantityValue[1]; return : VectorQuantityValue[1]; }
    calc def tensorTensorMult { in : TensorQuantityValue[1]; in : TensorQuantityValue[1]; return : TensorQuantityValue[1]; }
    
    /* Tensor transformation */
    calc def transform {
        in transformation : CoordinateTransformation;
        in sourceTensor : TensorQuantityValue;
        return targetTensor : TensorQuantityValue;
    }
}
```

## Declarations

- `TensorCalculations` — standard library package
- `TensorCalculations::[` — calc def
- `TensorCalculations::[::n` — attribute
- `TensorCalculations::isZeroTensorQuantity` — calc def
- `TensorCalculations::isUnitTensorQuantity` — calc def
- `TensorCalculations::+` — calc def
- `TensorCalculations::-` — calc def
- `TensorCalculations::scalarTensorMult` — calc def
- `TensorCalculations::TensorScalarMult` — calc def
- `TensorCalculations::scalarQuantityTensorMult` — calc def
- `TensorCalculations::TensorScalarQuantityMult` — calc def
- `TensorCalculations::tensorVectorMult` — calc def
- `TensorCalculations::vectorTensorMult` — calc def
- `TensorCalculations::tensorTensorMult` — calc def
- `TensorCalculations::transform` — calc def
