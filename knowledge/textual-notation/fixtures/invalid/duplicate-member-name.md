---
name: Duplicate member name in a namespace
expected: invalid
category: structural
findings:
  - rule: member names must be unique within a namespace
    element: Namespace
---

# Duplicate member name in a namespace

```sysml
package Vehicles {
	part def Engine;
	part def Engine;
}
```

**Expected finding:** `Engine` is declared twice in namespace `Vehicles`; member names must be unique
within their namespace.

**Corrected:**

```sysml
package Vehicles {
	part def Engine;
	part def Transmission;
}
```

**Reference:** [Namespace](../../../metamodel/elements/Namespace.md) – its `ownedMember` / `membership`
features; KerML name *distinguishability* requires distinct member names within a namespace. (No single
`validate…` constraint for this is exposed in the element file.)
