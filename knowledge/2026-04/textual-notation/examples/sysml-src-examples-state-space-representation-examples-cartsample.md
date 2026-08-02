---
name: CartSample
kind: example
language: SysML
source: sysml/src/examples/State Space Representation Examples/CartSample.sysml
elements: [ActionUsage, AttributeDefinition, AttributeUsage, FlowUsage, PartDefinition, PartUsage]
license: EPL-2.0
---

# CartSample

Verbatim SysML model from `sysml/src/examples/State Space Representation Examples/CartSample.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
// State Space Representation cart example

package CartSample {
    private import StateSpaceRepresentation::*;
    part def Cart {
        attribute mass :> ISQ::mass;

        attribute def CartInput :> Input {
            attribute force :> ISQ::force;
        }

        attribute def CartOutput :> Output {
            attribute velocity :> ISQ::speed;
        }

        attribute def CartState :> StateSpace {
            attribute velocity :> ISQ::speed;
        }

        attribute def CartStateDerivative :> StateDerivative {
            ref :>> stateSpace : CartState;
            attribute accel :> ISQ::acceleration;
        }
    }

    part def Pusher {
        attribute def PusherOutput :> Output {
            attribute force :> ISQ::force;
        }
    }

    part context {
        part cart : Cart {
            action cartBehavior : ContinuousStateSpaceDynamics {
                in input : CartInput;
                out output : CartOutput;
                :>> stateSpace : CartState;

                calc :>> getDerivative { 
                	in input: CartInput; 
                	in stateSpace: CartState;
                    new CartStateDerivative(input.force / mass)
                }
                calc :>> getOutput {
                	in :>> stateSpace : CartState;
                    new CartOutput(stateSpace.velocity)
                }
            }
        }
        part pusher : Pusher {
            attribute pusherForce :> ISQ::force;

            action pusherBehavior : ContinuousStateSpaceDynamics {
                in input;
                out output : PusherOutput;
                calc :>> getOutput {
                    new PusherOutput(pusherForce)
                }
            }
        }

        flow pusher.pusherBehavior.output to cart.cartBehavior.input;
    }
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeDefinition](../metamodel/elements/AttributeDefinition.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [PartDefinition](../metamodel/elements/PartDefinition.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
