---
name: FeatureChains
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/FeatureChains.kerml
elements: []
license: EPL-2.0
---

# FeatureChains

Verbatim KerML model from `kerml/src/examples/Simple Tests/FeatureChains.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package FeatureChains {
	classifier F {
		feature a : A;
	}
	  
	feature f : F;
	  
	classifier A {
		feature g = f.a;
	}
	  
	classifier B {
	  	feature f : F;
	  	feature a : A;
	}
	  
	feature b : B {
	  	connector f.a to a.g;
	  	binding f.a = a.g;
	}
	  
	feature g subsets f.a;
	subset g.g subsets b.f.a;
	redefinition b.f redefines b.a;
	  
	subtype g.g specializes b.f.a;
	
	disjoint b.f.a from b.a;
	
	feature h1 unions f, b.f, b.a;
	feature h2 differences b.f, b.a intersects f.a, g disjoint from h1;
	
	feature b_f_a chains b chains f.a;
}
```
