---
name: PacketUsage
kind: example
language: KerML
source: kerml/src/examples/Packet Example/PacketUsage.kerml
elements: []
license: EPL-2.0
---

# PacketUsage

Verbatim KerML model from `kerml/src/examples/Packet Example/PacketUsage.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
private import Packets::*;
private import ScalarValues::Real;
package 'Packet Usage' {
	
	feature packet1: 'Thermal Data Packet';
	feature packet2: 'Thermal Data Packet';
	feature packet3: 'Thermal Data Packet' {
		feature 'special data field' redefines 'packet data field'{
			feature :>> 'user data field' {
				feature 'special data': Real;
			}
		}
	}
	
}
	
```
