---
name: Missing statement terminator
expected: invalid
category: syntax
findings:
  - rule: a feature declaration must be terminated by ';' (or a '{ }' body)
    element: grammar
---

# Missing statement terminator

```sysml
part def Vehicle {
	attribute mass : Real
	attribute power : Real;
}
```

**Expected finding:** on `attribute mass : Real` – the declaration is missing its terminating `;`, so
the parser runs it into the next line.

**Corrected:**

```sysml
part def Vehicle {
	attribute mass : Real;
	attribute power : Real;
}
```

**Reference:** declaration forms in the [grammar summary](../../index.md#grammar-summary).
