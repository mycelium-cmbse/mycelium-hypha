---
name: Parts
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/Parts.sysml
declares: [Parts, Parts::Part, Parts::Part::start, Parts::Part::done, Parts::Part::ownedPorts, Parts::Part::performedActions, Parts::Part::ownedActions, Parts::Part::ownedActions::this, Parts::Part::exhibitedStates, Parts::Part::ownedStates, Parts::parts]
license: EPL-2.0
---

# Parts

Verbatim SysML standard-library source from `sysml.library/Systems Library/Parts.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package Parts {
doc
/*
 * This package defines the base types for parts and related structural elements in the
 * SysML language.
 */

	private import Objects::Object;
	private import Objects::objects;
	private import Items::Item;
	private import Items::items;
	private import Ports::Port;
	private import Ports::ports;
	private import Actions::Action;
	private import Actions::actions;
	private import States::StateAction;
	private import States::stateActions;
	
	abstract part def Part :> Item {
		doc
		/*
		 * Part is the most general class of objects that represent all or a part of a system.
		 * Part is the base type of all PartDefinitions.
		 */
	
		ref self: Part :>> Item::self;
		
		part start: Part :>> Item::start;
		part done: Part :>> Item::done;
		
		abstract port ownedPorts: Port[0..*] :> ports, timeEnclosedOccurrences {
			doc
			/*
			 * Ports that are owned by this Part.
			 */
		}
		
		abstract ref action performedActions: Action[0..*] :> actions, enactedPerformances {
			doc
			/*
			 * Actions that are performed by this Part.
			 */
		}
		
		abstract action ownedActions: Action[0..*] :> actions, ownedPerformances {
			doc
			/*
			 * Actions that are owned by this Part.
			 */
		
		 	ref part this : Part :>> Action::this, ownedPerformances::this = that as Part {
				doc
				/*
				 * The "this" reference of an ownedAction is always its owning Part.
				 */
			}
		}
		
		abstract ref state exhibitedStates: StateAction[0..*] :> stateActions, performedActions {
			doc
			/*
			 * StateActions that are exhibited by this Part.
			 */
		}
		
		abstract state ownedStates: StateAction[0..*] :> stateActions, ownedActions {
			doc
			/*
			 * StateActions that are owned by this Part.
			 */
		}
	}
	
	abstract part parts: Part[0..*] nonunique :> items {
		doc
		/*
		 * parts is the base feature of all part properties.
		 */
	}
	
}
```

## Declarations

- `Parts` — standard library package
- `Parts::Part` — part def
- `Parts::Part::start` — part
- `Parts::Part::done` — part
- `Parts::Part::ownedPorts` — port
- `Parts::Part::performedActions` — action
- `Parts::Part::ownedActions` — action
- `Parts::Part::ownedActions::this` — part
- `Parts::Part::exhibitedStates` — state
- `Parts::Part::ownedStates` — state
- `Parts::parts` — part
