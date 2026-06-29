---
name: Redefines a non-inherited feature
expected: invalid
category: structural
findings:
  - rule: a 'redefines' target must be a feature inherited from a specialized type
    element: Redefinition
---

# Redefines a non-inherited feature

```sysml
part def Vehicle {
	attribute redefines mass = 1500;
}
```

**Expected finding:** on `attribute redefines mass` – `Vehicle` specializes nothing, so there is no
inherited feature `mass` to redefine.

**Corrected (declare it, or redefine an actually-inherited feature):**

```sysml
part def Vehicle {
	attribute mass = 1500;
}
```

**Reference:** [Redefinition](../../../sysml2/elements/Redefinition.md).
