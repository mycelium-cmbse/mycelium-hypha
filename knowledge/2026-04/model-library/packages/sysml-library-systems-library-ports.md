---
name: Ports
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/Ports.sysml
declares: [Ports, Ports::Port, Ports::Port::subports, Ports::Port::interfacingPorts, Ports::ports]
license: EPL-2.0
---

# Ports

Verbatim SysML standard-library source from `sysml.library/Systems Library/Ports.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package Ports {
    doc
    /*
     * This package defines the base types for ports and related structural elements 
     * in the SysML language.
     */

    private import Objects::Object;
    private import Objects::objects;
    
    abstract port def Port :> Object {
        doc
        /*
         * Port is the most general class of objects that represent connection points
         * for interacting with a Part. Port is the base type of all PortDefinitions.
         * 
         * Transfers outgoing from a Port are always targeted to a Port connected to
         * the original Port by an Interface.
         */
    
        ref self: Port :>> Object::self;
        
        port subports: Port [0..*] :> ports, timeEnclosedOccurrences {
            doc
            /*
             * The Ports that are subports of this Port.
             */
        }
        
        abstract ref port interfacingPorts : Port[0..*] nonunique :> ports {
            doc
            /*
             * Ports that are connected to this Port by an Interface.
             */
        }
        
        ref :>> outgoingTransfersFromSelf :> interfacingPorts.incomingTransfersToSelf {
            doc
            /* 
             * The target of each of the outgoingTransfersFromSelf of a Port must be an interfacingPort.
             */
             
             end ref source;
             end ref target;
        }
    }
    
    abstract port ports : Port[0..*] nonunique :> objects {
        doc
        /*
         * ports is the base feature of all PortUsages.
         */
    }
}
```

## Declarations

- `Ports` — standard library package
- `Ports::Port` — port def
- `Ports::Port::subports` — port
- `Ports::Port::interfacingPorts` — port
- `Ports::ports` — port
