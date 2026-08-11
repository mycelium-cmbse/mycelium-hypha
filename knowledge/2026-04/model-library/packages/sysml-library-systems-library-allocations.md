---
name: Allocations
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/Allocations.sysml
declares: [Allocations, Allocations::Allocation, Allocations::allocations]
license: EPL-2.0
---

# Allocations

Verbatim SysML standard-library source from `sysml.library/Systems Library/Allocations.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package Allocations {
	doc
	/*
	 * This package defines the base types for allocations and related structural elements
	 * in the SysML language.
	 */

	private import Base::Anything;
	private import Connections::*;

	allocation def Allocation :> BinaryConnection {
		doc
		/*
		 * Allocation is the most general class of allocation, represented as a connection 
		 * between the source of the allocation and the target. Allocation is the base type 
		 * of all AllocationDefinitions.
		 */
	
		end source: Anything :>> BinaryConnection::source;
		end target: Anything :>> BinaryConnection::target;
	}
	
	abstract allocation allocations: Allocation[0..*] nonunique :> binaryConnections {
		doc
		/*
		 * allocations is the base feature of all AllocationUsages.
		 */
	}
}
```

## Declarations

- `Allocations` — standard library package
- `Allocations::Allocation` — allocation def
- `Allocations::allocations` — allocation
