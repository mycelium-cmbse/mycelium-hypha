---
name: Behaviors
kind: example
language: KerML
source: kerml/src/examples/Simple Tests/Behaviors.kerml
elements: [FlowUsage]
license: EPL-2.0
---

# Behaviors

Verbatim KerML model from `kerml/src/examples/Simple Tests/Behaviors.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
package Behaviors {
    behavior A {
        in x;
        out y = b.y1;
        composite step b : B {
            in x1 = A::x;
        }
    }
    behavior B specializes A {
        in x1;
        out var y1;
    }
    class C {
        var z = A().y;
        step a : A;
        step b : B;
        binding z = a.y;
        flow a.y to b.x1;
    }
    abstract flow msg of C;
}
```

## Elements

- [FlowUsage](../metamodel/elements/FlowUsage.md)
