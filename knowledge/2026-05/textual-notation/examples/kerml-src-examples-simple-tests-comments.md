---
name: Comments
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Comments.kerml
elements: [PartUsage]
license: EPL-2.0
---

# Comments

Verbatim KerML model from `kerml/src/examples/Simple Tests/Comments.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
/* AAA */
//a lexical comment ("note") is not a part of model
package Comments {
	// inside package
	/*
*AAA
 * BBB*/	
 /*
    *
    *
    * AAA  ***   
    *BBB
    								*/

   /*
 *       AAAA
 *       BBBB           */	
 /* AAAA
 
 
  * BBBB
 *
 * CCCC
 */
 locale "en_US" /*
 * AAAA
 * BBBB
 *    CCC DDD    
 */
	
	/* comment inside a package */
	comment cmt /* Named Comment */	
	comment cmt_cmt about cmt /* Other Comment about Comment */
	
	class C {
		doc locale "en_US"/* Documentation on Class C */
		comment /* Comment in Class C */
		comment about Comments /* Comment about Package */
		
	}
	/* abc */
	class A {
		doc <a> /* Documentation comment on A*/
		comment about a locale "en_US" /* Comment about documenation with ID 'a' */		
	}
}
```

## Elements

- [PartUsage](../metamodel/elements/PartUsage.md)
