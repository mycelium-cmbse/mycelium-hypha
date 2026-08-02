---
name: Interface Decomposition Example
kind: example
language: SysML
source: sysml/src/training/11. Interfaces/Interface Decomposition Example.sysml
elements: [InterfaceDefinition, InterfaceUsage, PortDefinition, PortUsage]
license: EPL-2.0
---

# Interface Decomposition Example

Verbatim SysML model from `sysml/src/training/11. Interfaces/Interface Decomposition Example.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package 'Interface Decomposition Example' {
	
	port def SpigotBank;
	port def Spigot;
	
	port def Faucet;
	port def FaucetInlet;
	
	interface def WaterDelivery {
		end [1] port suppliedBy : SpigotBank {
			port hot : Spigot;
			port cold : Spigot;
		}
		end [1..*] port deliveredTo : Faucet {
			port hot : FaucetInlet;
			port cold : FaucetInlet;
		}
		
		connect suppliedBy.hot to deliveredTo.hot;
		connect suppliedBy.cold to deliveredTo.cold;
	}
	
}
```

## Elements

- [InterfaceDefinition](../metamodel/elements/InterfaceDefinition.md)
- [InterfaceUsage](../metamodel/elements/InterfaceUsage.md)
- [PortDefinition](../metamodel/elements/PortDefinition.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
