---
name: Items
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/Items.sysml
declares: [Items, Items::Item, Items::Item::start, Items::Item::done, Items::Item::shape, Items::Item::envelopingShapes, Items::Item::envelopingShapes::envelopedItem, Items::Item::envelopingShapes::envelopingItem, Items::Item::boundingShapes, Items::Item::boundingShapes::boundingShape, Items::Item::boundingShapes::face, Items::Item::boundingShapes::inter, Items::Item::boundingShapes::edge, Items::Item::boundingShapes::inter, Items::Item::voids, Items::Item::isSolid, Items::Item::subitems, Items::Item::subparts, Items::Item::checkedConstraints, Items::Touches, Items::Touches::touchedItemToo, Items::Touches::touchedItem, Items::items]
license: EPL-2.0
---

# Items

Verbatim SysML standard-library source from `sysml.library/Systems Library/Items.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package Items {
	doc
	/*
	 * This package defines the base types for items and related structural elements in the
	 * SysML language.
	 */

	private import Objects::Object;
	private import Objects::objects;
	private import Parts::Part;
	private import Parts::parts;
	private import Occurrences::HappensWhile;
	private import Occurrences::JustOutsideOf;
	private import Objects::StructuredSpaceObject;
	private import Constraints::ConstraintCheck;
	private import Constraints::constraintChecks;
	private import CollectionFunctions::contains;
	private import SequenceFunctions::isEmpty;
	private import SequenceFunctions::notEmpty;
	private import SequenceFunctions::includes;
	private import SequenceFunctions::union;
	private import ControlFunctions::forAll;
	
	abstract item def Item :> Object {
		doc
		/*
		 * Item is the most general class of objects that are part of, exist in or flow through a system. 
		 * Item is the base type of all ItemDefinitions.
		 */
	
		ref self: Item :>> Object::self;
		
		item start: Item :>> startShot;
		item done: Item :>> endShot;
		
		item shape : Item :>> spaceBoundary {
		doc
			/*
			 * The shape of an Item is its spatial boundary.
			 */
		}
		
		item envelopingShapes : Item[0..*] {
            doc
			/*
			 * Each enveloping shape is the shape of an Item that spacially overlaps this Item for its
			 * entire lifetime.
			 */
			 
			ref item envelopedItem :>> that;	

			assert constraint { 
                doc
                /* 
                 * Enables two dimensional items to be enveloped by two or three dimensional shapes.
                 */             
			    innerSpaceDimension == 
    				(if envelopedItem.innerSpaceDimension == 3  | envelopedItem.outerSpaceDimension == 3? 2 
    				else envelopedItem.outerSpaceDimension - 1)
			}
			assert constraint { (that as Item).innerSpaceDimension < 3 implies notEmpty(outerSpaceDimension) }

			item envelopingItem [1];

			assert constraint {
				doc
				/* 
				 * This constraint prevents an envelopingShape from being a portion.
				 */
				 
				envelopingItem.shape.spaceTimeCoincidentOccurrences->includes(that) and
				envelopingItem.spaceTimeEnclosedOccurrences->includes(that.that) 
			}
		}
		
		item boundingShapes : StructuredSpaceObject [0..*] :> envelopingShapes {
            doc
			/*
			 * envelopingShapes that are structured space objects with every face or every edge
			 * intersecting this Item.
			 */		
            
			ref item boundingShape: Item :>> self;

			private item :>> faces {
				ref item face :>> self;
				item inter [1];
				assert constraint { contains(inter.intersectionsOf, union(face, boundingShape)) }
			}
			private item :>> edges {
				ref item edge :>> self;
				item inter [1];
				assert constraint { isEmpty(faces) implies
							contains(inter.intersectionsOf, union(edge, boundingShape)) }
			}
		}

		item voids :>> innerSpaceOccurrences [0..*] {
			doc
			/*
			 * Voids are inner space occurrences of this Item.
			 */
		}

		attribute isSolid = isEmpty(voids) {
			doc
			/*
			 * An Item is solid if it has no voids.
			 */
		}
		
		abstract item subitems: Item[0..*] :> items, subobjects {
			doc
			/*
			 * The Items that are composite subitems of this Item.
			 */
			 
			private ref redefines Item::incomingTransferSort, subobjects::incomingTransferSort;
		}
		
		abstract part subparts: Part[0..*] :> subitems, parts {
			doc
			/*
			 * The subitems of this Item that are Parts.
			 */
		}
		
		abstract constraint checkedConstraints: ConstraintCheck[0..*] :> constraintChecks, ownedPerformances {
			doc
			/*
			 * Constraints that have been checked by this Item.
			 */
		}
	}
	
	connection def Touches :> JustOutsideOf, HappensWhile {
		doc
		/*
		 * Touching items are just outside each other and happen at the same time.
		 */
	
		end touchesToo [0..*] item touchedItemToo :>> separateSpaceToo, thisOccurrence;
		end touches [0..*] item touchedItem :>> separateSpace, thatOccurrence;
	}

	abstract item items : Item[0..*] nonunique :> objects {
		doc
		/*
		 * items is the base feature of all ItemUsages.
		 */
	}
	
}
```

## Declarations

- `Items` — standard library package
- `Items::Item` — item def
- `Items::Item::start` — item
- `Items::Item::done` — item
- `Items::Item::shape` — item
- `Items::Item::envelopingShapes` — item
- `Items::Item::envelopingShapes::envelopedItem` — item
- `Items::Item::envelopingShapes::envelopingItem` — item
- `Items::Item::boundingShapes` — item
- `Items::Item::boundingShapes::boundingShape` — item
- `Items::Item::boundingShapes::face` — item
- `Items::Item::boundingShapes::inter` — item
- `Items::Item::boundingShapes::edge` — item
- `Items::Item::boundingShapes::inter` — item
- `Items::Item::voids` — item
- `Items::Item::isSolid` — attribute
- `Items::Item::subitems` — item
- `Items::Item::subparts` — part
- `Items::Item::checkedConstraints` — constraint
- `Items::Touches` — connection def
- `Items::Touches::touchedItemToo` — item
- `Items::Touches::touchedItem` — item
- `Items::items` — item
