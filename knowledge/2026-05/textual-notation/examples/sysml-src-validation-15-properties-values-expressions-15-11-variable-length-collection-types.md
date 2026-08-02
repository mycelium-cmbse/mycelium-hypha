---
name: 15_11-Variable Length Collection Types
kind: example
language: SysML
source: sysml/src/validation/15-Properties-Values-Expressions/15_11-Variable Length Collection Types.sysml
elements: [AttributeDefinition, AttributeUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# 15_11-Variable Length Collection Types

Verbatim SysML model from `sysml/src/validation/15-Properties-Values-Expressions/15_11-Variable Length Collection Types.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package '15_11-Variable Length Collection Types' {
	private import ScalarValues::*;
	private import Collections::*;
	
	part def SparePart;
	part def Person;
	
	/* Examples of declaring syntactic sugar-like names for instantiating collection types. */
	
	attribute def 'Bag<SparePart>' :> Bag {
		ref part :>> elements: SparePart;
	}
	
	attribute def 'List<Integer>' :> List {
		value :>> elements: Integer;
	}
	
	attribute def 'Set<String>' :> Set {
		attribute :>> elements: String;
	}
	
	attribute def 'OrderedSet<Person>' :> OrderedSet {
		ref part :>> elements: Person;
	}
	
	attribute def 'List<Set<Person>>' :> List {
		attribute :>> elements: Set {
			ref part :>> elements: Person;
		}
	}
	
	attribute def 'Array<Real>[4]' :> Array {
		attribute :>> elements: Real;
		attribute :>> dimensions = 4;
	}
}
```

## Elements

- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
