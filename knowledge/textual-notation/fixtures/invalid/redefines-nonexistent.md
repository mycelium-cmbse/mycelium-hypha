---
name: Redefines a non-inherited feature
expected: invalid
category: structural
findings:
  - rule: a 'redefines' target must be a feature inherited from a specialized type
    element: Redefinition
    constraint: validateRedefinitionFeaturingTypes (redefinedFeature [1..1])
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

**Reference:** [Redefinition](../../../metamodel/elements/Redefinition.md) – `redefinedFeature` is `[1..1]`
(the redefinition must resolve to an actual Feature), and `validateRedefinitionFeaturingTypes` requires
the redefining feature to sit on a featuringType the redefined one does not (i.e. there must be a
supertype to redefine through).

**How it is decided:** reachability, resolved against `knowledge/metamodel/metamodel.json` – a feature is
redefinable from a type when it appears in that type's `ownedAttributes` or `inheritedAttributes`, and
`inheritedFrom` names the declaring supertype to cite. Here the declared `Vehicle` specializes nothing, so
the reachable set is empty whatever the metamodel says about `PartDefinition`.
