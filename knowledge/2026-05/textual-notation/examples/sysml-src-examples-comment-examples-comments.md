---
name: Comments
kind: example
language: SysML
source: sysml/src/examples/Comment Examples/Comments.sysml
elements: [PartDefinition, PartUsage]
license: EPL-2.0
---

# Comments

Verbatim SysML model from `sysml/src/examples/Comment Examples/Comments.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package Comments {
	doc /* Documentation Comment */

	doc /* Documentation about Package */

	comment cmt /* Named Comment */	
	comment cmt_cmt about cmt /* Comment about Comment */
	
	comment about C /* Documention Comment on Part Def */
	part def C {
		doc /* Documentation in Part Def */
		comment /* Comment in Part Def */
		comment about Comments /* Comment about Package */
	}
}
```

## Elements

- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
