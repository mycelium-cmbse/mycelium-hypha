---
name: PacketUsage
kind: example
language: SysML
source: sysml/src/examples/Packet Example/PacketUsage.sysml
elements: [AttributeUsage, PartUsage]
license: EPL-2.0
---

# PacketUsage

Verbatim SysML model from `sysml/src/examples/Packet Example/PacketUsage.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Packet Usage' {
	public import Packets::*;
	private import ScalarValues::Real;
	
	part packet1: 'Thermal Data Packet';
	part packet2: 'Thermal Data Packet';
	part packet3: 'Thermal Data Packet' {
		attribute 'special data field' redefines 'packet data field'{
			attribute redefines 'user data field' {
				attribute 'special data': Real;
			}
		}
	}
	
}
	
```

## Elements

- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
