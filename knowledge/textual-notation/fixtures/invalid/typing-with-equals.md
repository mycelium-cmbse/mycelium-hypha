---
name: Typing with '=' instead of ':'
expected: invalid
category: syntax
findings:
  - rule: a feature is typed with ':' (defined by); '=' binds a value, not a type
    element: FeatureValue
---

# Typing with '=' instead of ':'

```sysml
part def Vehicle {
	part eng = Engine;
}
```

**Expected finding:** on `part eng = Engine` – a part is typed with `:` (`part eng : Engine`). The `=`
operator binds a feature *value*, not a type, so `Engine` is being read as a value expression here.

**Corrected:**

```sysml
part def Vehicle {
	part eng : Engine;
}
```

**Reference:** [relationship operators](../../index.md#relationship-operators);
[FeatureValue](../../../sysml2/elements/FeatureValue.md) (`=` binds a value);
[PartUsage](../../../sysml2/elements/PartUsage.md) – its `partDefinition` is the part's definition, of
which `= Engine` provides none.
