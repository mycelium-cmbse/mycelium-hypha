---
name: Invalid multiplicity range
expected: invalid
category: semantic
findings:
  - rule: multiplicity lower bound must not exceed the upper bound
    element: MultiplicityRange
---

# Invalid multiplicity range

```sysml
part def Vehicle {
	attribute mass : Real[2..1];
}
```

**Expected finding:** on `attribute mass : Real[2..1]` – the multiplicity range `[2..1]` is invalid
because the lower bound (2) is greater than the upper bound (1).

**Corrected:**

```sysml
part def Vehicle {
	attribute mass : Real[1..2];
}
```

**Reference:** [MultiplicityRange](../../../metamodel/elements/MultiplicityRange.md) – its `lowerBound` /
`upperBound` features; a range with `lowerBound > upperBound` is empty/ill-formed. (The metamodel has no
dedicated `validate…` constraint for this; it is inherent range semantics.)
