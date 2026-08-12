---
name: ParametersOfInterestMetadata
kind: model-library-file
language: SysML
source: sysml.library/Domain Libraries/Metadata/ParametersOfInterestMetadata.sysml
declares: [ParametersOfInterestMetadata, ParametersOfInterestMetadata::measuresOfEffectiveness, ParametersOfInterestMetadata::measuresOfPerformance]
license: EPL-2.0
---

# ParametersOfInterestMetadata

Verbatim SysML standard-library source from `sysml.library/Domain Libraries/Metadata/ParametersOfInterestMetadata.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package ParametersOfInterestMetadata {
	doc
	/*
	 * This package contains definitions of metadata to identify key parameters of interest,
	 * including measures of effectiveness (MOE) and other key measures of performance (MOP).
	 */
	 
	 private import Metaobjects::SemanticMetadata;
	 
	 attribute measuresOfEffectiveness[*] nonunique {
	 	doc /* Base feature for attributes that are measures of effectiveness. */
	 }
	 
	 attribute measuresOfPerformance[*] nonunique {
	 	doc /* Base feature for attributes that are measures of performance. */
	 }
	 
	 metadata def <moe> MeasureOfEffectiveness :> SemanticMetadata {
	 	doc 
	 	/*
	 	 * MeasureOfEffectiveness is semantic metadata for identifying an attribute as a
	 	 * measure of effectiveness.
	 	 */
	 	
	 	:>> annotatedElement : SysML::Usage;
	 	:>> baseType = measuresOfEffectiveness meta SysML::Usage;
	 }
	 
	 metadata def <mop> MeasureOfPerformance :> SemanticMetadata {
	 	doc 
	 	/*
	 	 * MeasureOfPerformance is semantic metadata for identifying an attribute as a
	 	 * measure of performance.
	 	 */
	 	
	 	:>> annotatedElement : SysML::Usage;
	 	:>> baseType = measuresOfPerformance meta SysML::Usage;
	 }
}
```

## Declarations

- `ParametersOfInterestMetadata` — standard library package
- `ParametersOfInterestMetadata::measuresOfEffectiveness` — attribute
- `ParametersOfInterestMetadata::measuresOfPerformance` — attribute
