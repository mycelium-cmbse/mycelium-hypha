---
name: Unbalanced braces
expected: invalid
category: syntax
findings:
  - rule: every '{' must be matched by a closing '}'
    element: grammar
---

# Unbalanced braces

```sysml
package Vehicles {
	part def Vehicle {
}
```

**Expected finding:** the body of `part def Vehicle {` is never closed – there is one fewer `}` than
`{`, so `package Vehicles` is left open at end of input.

**Corrected:**

```sysml
package Vehicles {
	part def Vehicle {
	}
}
```

**Reference:** declaration forms in the [grammar summary](../../index.md#grammar-summary).
