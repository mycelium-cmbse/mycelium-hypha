---
name: ProductSelection_N_ary
kind: example
language: SysML
source: sysml/src/examples/Association Examples/ProductSelection_N_ary.sysml
elements: [ConnectionDefinition, ConnectionUsage, ItemDefinition, ItemUsage]
license: EPL-2.0
---

# ProductSelection_N_ary

Verbatim SysML model from `sysml/src/examples/Association Examples/ProductSelection_N_ary.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ProductSelection_N_ary_SysML {
	
	item def ShoppingCart;
	item def Product;
	item def Account;
	
	// User-specified connection defiation definition
	connection def ProductSelection {
		end [0..1] item cart: ShoppingCart[1];
		end [0..*] item selectedProduct: Product[1];
		end [1..1] item account : Account[1];
	}
	
	// Equivalent connection defiation definition with named end items.
	connection def ProductSelection1 {
		end inCart[0..1] item cart: ShoppingCart[1];
		end selectedProducts[0..*] item selectedProduct: Product[1];
		end withAccount[1..1] item account : Account[1];
	}
	
}
```

## Elements

- [ConnectionDefinition](../metamodel/elements/ConnectionDefinition.md)
- [ConnectionUsage](../metamodel/elements/ConnectionUsage.md)
- [ItemDefinition](../metamodel/elements/ItemDefinition.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
