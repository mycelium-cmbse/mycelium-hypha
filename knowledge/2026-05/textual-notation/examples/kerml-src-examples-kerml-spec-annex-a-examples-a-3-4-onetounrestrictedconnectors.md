---
name: A-3-4-OneToUnrestrictedConnectors
kind: example
language: KerML
source: kerml/src/examples/KerML Spec Annex A Examples/A-3-4-OneToUnrestrictedConnectors.kerml
elements: []
license: EPL-2.0
---

# A-3-4-OneToUnrestrictedConnectors

Verbatim KerML model from `kerml/src/examples/KerML Spec Annex A Examples/A-3-4-OneToUnrestrictedConnectors.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml

package OneToUnrestrictedConnectorsModelToBeExecuted {
	doc
	/* 
	 */

	private import WithoutConnectorsModelToBeExecuted::BikeFork;

	classifier Bicycle {
		feature carrier : BikeBasket [*];
		feature holdsWheel : BikeFork [*];
		connector carrierFixed : BikeBasketFixed from [*] carrier to [1] holdsWheel;
	}
	classifier BikeBasket;

	assoc BikeBasketFixed {
		end feature basket : BikeBasket;
		end feature fixedTo : BikeFork;
	}
}

package OneToUnrestrictedConnectorsExecution {
	doc
	/* 
	 */

	private import Atoms::*;
	private import OneToUnrestrictedConnectorsModelToBeExecuted::*;
	private import OneToOneConnectorsExecution::MyBikeFork1;
	private import OneToOneConnectorsExecution::MyBikeFork2;
	private import OneToOneConnectorsExecution::MyBikeFork;

	#atom
	classifier MyBikeBasket1 specializes BikeBasket;
	#atom
	classifier MyBikeBasket2 specializes BikeBasket;

	classifier MyBikeBasket unions MyBikeBasket1, MyBikeBasket2;

	#atom
	assoc MyBikeBasket1_Fork1_BBF_Link specializes BikeBasketFixed {
		end feature redefines basket : MyBikeBasket1;
		end feature redefines fixedTo : MyBikeFork1;
	}
	#atom
	assoc MyBikeBasket2_Fork1_BBF_Link specializes BikeBasketFixed {
		end feature redefines basket : MyBikeBasket2;
		end feature redefines fixedTo : MyBikeFork1;
	}

	classifier MyBikeBasket_Fork_BBF_Link unions MyBikeBasket1_Fork1_BBF_Link, MyBikeBasket2_Fork1_BBF_Link;

	#atom
	classifier MyBike specializes Bicycle {
		feature redefines carrier : MyBikeBasket [2];
		feature redefines holdsWheel : MyBikeFork [2];
		connector redefines carrierFixed : MyBikeBasket_Fork_BBF_Link [2] from [*] carrier to [1] holdsWheel;
	}
}
```
