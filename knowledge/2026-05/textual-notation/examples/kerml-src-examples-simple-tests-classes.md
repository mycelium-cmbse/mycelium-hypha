---
name: Classes
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Classes.kerml
elements: []
license: EPL-2.0
---

# Classes

Verbatim KerML model from `kerml/src/examples/Simple Tests/Classes.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Classes {
	
	feature f: A;

	public class <'1'> A { 
		feature b: B;
		protected in c: C;
		portion feature p : A;
	}
	
	abstract class <'2'> B {
		public abstract feature a: A {
			composite feature aa: A;
		}
		public composite feature a1: A;
		feature x {
			composite feature a: A {
			    portion feature q : A;
			}
			portion feature q : A;
		}
		package P { }
	}
	
	private struct C specializes Classes::'2' {
		private y: A, '2'[0..*];
		alias z for y;
		composite feature c : C {
			composite feature cc : C;
		}
	}
	
}
```
