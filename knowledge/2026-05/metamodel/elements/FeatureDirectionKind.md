---
name: FeatureDirectionKind
package: Types
fully qualified name: KerML::Core::Types::FeatureDirectionKind
visibility: public
kind: enumeration
---

# FeatureDirectionKind

`Types` package · enumeration

FeatureDirectionKind enumerates the possible kinds of direction that a Feature may be given as a member of a Type.

## Literals

- `in` — Values of the Feature on each instance of its domain are determined externally to that instance and used internally.
- `inout` — Values of the Feature on each instance are determined either as in or out directions, or both.
- `out` — Values of the Feature on each instance of its domain are determined internally to that instance and used externally.
