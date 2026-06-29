---
name: Reserved keyword used as a name
expected: invalid
category: lexical
findings:
  - rule: a reserved keyword cannot be an unquoted name
    element: grammar
---

# Reserved keyword used as a name

```sysml
part def part;
```

**Expected finding:** the name `part` is a reserved keyword and cannot be used as an unquoted
identifier. To use it as a name it must be a quoted (escaped) name, `'part'`.

**Corrected:**

```sysml
part def Part;
```

**Reference:** reserved words in the [keyword reference](../../index.md#keyword-reference).
