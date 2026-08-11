---
name: MeasurementRefCalculations
kind: model-library-file
language: SysML
source: sysml.library/Domain Libraries/Quantities and Units/MeasurementRefCalculations.sysml
declares: [MeasurementRefCalculations, MeasurementRefCalculations::*, MeasurementRefCalculations::/, MeasurementRefCalculations::**, MeasurementRefCalculations::^, MeasurementRefCalculations::CoordinateFrame*, MeasurementRefCalculations::CoordinateFrame/, MeasurementRefCalculations::ToString]
license: EPL-2.0
---

# MeasurementRefCalculations

Verbatim SysML standard-library source from `sysml.library/Domain Libraries/Quantities and Units/MeasurementRefCalculations.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package MeasurementRefCalculations {
	doc
	/*
	 * This package package defines calculations on MeasurementUnits and CoordinateFrames.
	 */
	 
    private import ScalarValues::String;
    private import ScalarValues::Real;
    private import MeasurementReferences::MeasurementUnit;
    private import MeasurementReferences::ScalarMeasurementReference;
    private import MeasurementReferences::CoordinateFrame;
        
    /* MeasurementUnit operations */
    calc def '*' specializes DataFunctions::'*' { in x: MeasurementUnit[1]; in y: MeasurementUnit[1]; return : MeasurementUnit[1]; }
    calc def '/' specializes DataFunctions::'/' { in x: MeasurementUnit[1]; in y: MeasurementUnit[1]; return : MeasurementUnit[1]; }
    calc def '**' specializes DataFunctions::'**' { in x: MeasurementUnit[1]; in y: Real[1]; return : MeasurementUnit[1]; }
    calc def '^' specializes DataFunctions::'^' { in x: MeasurementUnit[1]; in y: Real[1]; return : MeasurementUnit[1]; }

    /* CoordinateFrame and MeasurementUnit operations */
    calc def 'CoordinateFrame*' specializes DataFunctions::'*' { in x: CoordinateFrame[1]; in y: MeasurementUnit[1]; return : CoordinateFrame[1]; }
    calc def 'CoordinateFrame/' specializes DataFunctions::'/' { in x: CoordinateFrame[1]; in y: MeasurementUnit[1]; return : CoordinateFrame[1]; }

    calc def ToString specializes BaseFunctions::ToString { 
        doc 
        /*
         * Returns the Unicode string symbol representing a scalar measurement reference.
         */
        in x: ScalarMeasurementReference[1]; return : String[1];
    }
}
```

## Declarations

- `MeasurementRefCalculations` — standard library package
- `MeasurementRefCalculations::*` — calc def
- `MeasurementRefCalculations::/` — calc def
- `MeasurementRefCalculations::**` — calc def
- `MeasurementRefCalculations::^` — calc def
- `MeasurementRefCalculations::CoordinateFrame*` — calc def
- `MeasurementRefCalculations::CoordinateFrame/` — calc def
- `MeasurementRefCalculations::ToString` — calc def
