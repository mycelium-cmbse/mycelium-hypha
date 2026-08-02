---
name: Scoping
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Scoping.kerml
elements: []
license: EPL-2.0
---

# Scoping

Verbatim KerML model from `kerml/src/examples/Simple Tests/Scoping.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Scoping {
    package P1 {
        class A {
            feature f;
        }
        package P2 {
            class A {
                feature g;
            }
            package P3 {
                class B :> A {
                    feature :>> g;
                }
            }
        }
        package Objects {
            class Object {
                feature test1;
            }
        }
        package '$' {
            class Objects {
                class Object {
                    feature test2;
                }
            }
        }
        package P4 {
            class C :> Objects::Object {
                feature :>> test1;
            }
            class D :> '$'::Objects::Object {
                feature :>> test2;
            }
            class E :> $::Objects::Object {
                feature :>> subobjects;
            }
        }
    }
}
```
