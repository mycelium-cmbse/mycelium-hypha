---
name: ServerSequenceRealization-2
kind: example
language: SysML
source: sysml/src/examples/Interaction Sequencing Examples/ServerSequenceRealization-2.sysml
elements: [ActionUsage, AttributeUsage, ExhibitStateUsage, FlowUsage, InterfaceUsage, ItemUsage, PartUsage, PerformActionUsage, PortDefinition, PortUsage, StateUsage, TransitionUsage]
license: EPL-2.0
---

# ServerSequenceRealization-2

Verbatim SysML model from `sysml/src/examples/Interaction Sequencing Examples/ServerSequenceRealization-2.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
package ServerSequenceRealization_2 {
	private import ScalarValues::String;
	private import ServerSequenceModel::*;
	private import Configuration::*;
	
	package Configuration {
		
		port def PublicationPort;
		
		port def SubscriptionPort;
		
		part producer_2[1] {
			attribute someTopic : String;
			private item somePublication;
			
			port publicationPort : ~PublicationPort;
			
			perform action producerBehavior {	
				action publish send new Publish(someTopic, somePublication) via publicationPort;
			}
		}
		
		interface producer_2.publicationPort to server_2.publicationPort;
		
		part server_2[1] {
			port publicationPort : PublicationPort;
			port subscriptionPort : SubscriptionPort;
			
			exhibit state serverBehavior {
				entry; then waitForSubscription;
				
				state waitForSubscription;
				transition subscribing
					first waitForSubscription
					accept sub : Subscribe via subscriptionPort
					then waitForPublication;
					
				state waitForPublication;
				transition delivering
					first waitForPublication
					accept pub : Publish via publicationPort
					if pub.topic == subscribing.sub.topic
					do send new Deliver(pub.publication) to subscribing.sub.subscriber
					then waitForPublication;
			}
		}
		
		interface consumer_2.subscriptionPort to server_2.subscriptionPort;
		
		part consumer_2[1] {
			attribute myTopic : String;
			
			port subscriptionPort : ~SubscriptionPort;
			
			perform action consumerBehavior {
				action subscribe send new Subscribe(myTopic, consumer_2) to server_2;
				then action delivery accept Deliver via consumer_2;
			}
		}
		
	}
	
	part realization_2 : PubSubSequence {
		part :>> producer :> producer_2 {
			event producerBehavior.publish[1] :>> publish_source_event;
		}

		part :>> server :> server_2 {
			event serverBehavior.subscribing.accepter[1] :>> subscribe_target_event;
			event serverBehavior.delivering.accepter[1] :>> publish_target_event;
			event serverBehavior.delivering.effect[1] :>> deliver_source_event;
		}
		
		part :>> consumer :> consumer_2 {
			event consumerBehavior.subscribe[1] :>> subscribe_source_event;
			event consumerBehavior.delivery[1] :>> deliver_target_event;
		}

		flow :>> publish_message: Transfers::MessageTransfer {
 			end :>> source ::> producer.publicationPort;
 			end :>> target ::> server.publicationPort;
 		}
		flow :>> subscribe_message: Transfers::MessageTransfer {
 			end :>> source ::> consumer.subscriptionPort;
 			end :>> target ::> server.subscriptionPort;
 		}
		flow :>> deliver_message: Transfers::MessageTransfer {
 			end :>> source ::> server;
 			end :>> target ::> consumer;
 		}
 		
 		/* Binding sent/accept messages to specification model messages. */
		  /* Sends */
 		bind producer_2.producerBehavior.publish.sentMessage = publish_message;
 		bind consumer_2.consumerBehavior.subscribe.sentMessage = subscribe_message;
 		bind server_2.serverBehavior.delivering.effect.sentMessage = deliver_message;
 		  /* Accepts */
 		bind consumer_2.consumerBehavior.delivery.acceptedMessage = subscribe_message;
 		bind server_2.serverBehavior.subscribing.accepter.acceptedMessage = subscribe_message;
 		bind server_2.serverBehavior.delivering.accepter.acceptedMessage = publish_message;
	}
}
```

## Elements

- [ActionUsage](../metamodel/elements/ActionUsage.md)
- [AttributeUsage](../metamodel/elements/AttributeUsage.md)
- [ExhibitStateUsage](../metamodel/elements/ExhibitStateUsage.md)
- [FlowUsage](../metamodel/elements/FlowUsage.md)
- [InterfaceUsage](../metamodel/elements/InterfaceUsage.md)
- [ItemUsage](../metamodel/elements/ItemUsage.md)
- [PartUsage](../metamodel/elements/PartUsage.md)
- [PerformActionUsage](../metamodel/elements/PerformActionUsage.md)
- [PortDefinition](../metamodel/elements/PortDefinition.md)
- [PortUsage](../metamodel/elements/PortUsage.md)
- [StateUsage](../metamodel/elements/StateUsage.md)
- [TransitionUsage](../metamodel/elements/TransitionUsage.md)
