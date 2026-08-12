---
name: Transfers
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Semantic Library/Transfers.kerml
declares: [Transfers, Transfers::Transfer, Transfers::Transfer::source, Transfers::Transfer::source::sourceOutput, Transfers::Transfer::target, Transfers::Transfer::target::targetInput, Transfers::Transfer::isInstant, Transfers::Transfer::payload, Transfers::Transfer::payloadNum, Transfers::Transfer::instant, Transfers::MessageTransfer, Transfers::FlowTransfer, Transfers::FlowTransfer::isMove, Transfers::FlowTransfer::isPush, Transfers::FlowTransfer::sourceOutputLink, Transfers::FlowTransfer::sourceOutputLink::transferSource, Transfers::FlowTransfer::sourceOutputLink::transferPayload, Transfers::FlowTransfer::targetInputLink, Transfers::FlowTransfer::targetInputLink::transferTarget, Transfers::FlowTransfer::targetInputLink::transferPayload, Transfers::FlowTransfer::sending, Transfers::FlowTransfer::moving, Transfers::FlowTransfer::pushing, Transfers::FlowTransfer::delivering, Transfers::TransferBefore, Transfers::TransferBefore::source, Transfers::TransferBefore::target, Transfers::TransferBefore::self, Transfers::FlowTransferBefore, Transfers::FlowTransferBefore::source, Transfers::FlowTransferBefore::target, Transfers::transfers, Transfers::transfers::source, Transfers::transfers::target, Transfers::messageTransfers, Transfers::messageTransfers::source, Transfers::messageTransfers::target, Transfers::flowTransfers, Transfers::flowTransfers::source, Transfers::flowTransfers::target, Transfers::transfersBefore, Transfers::transfersBefore::source, Transfers::transfersBefore::target, Transfers::flowTransfersBefore, Transfers::flowTransfersBefore::source, Transfers::flowTransfersBefore::target, Transfers::SendPerformance, Transfers::SendPerformance::payload, Transfers::SendPerformance::sender, Transfers::SendPerformance::receiver, Transfers::SendPerformance::sentTransfer, Transfers::AcceptPerformance, Transfers::AcceptPerformance::payload, Transfers::AcceptPerformance::receiver, Transfers::AcceptPerformance::acceptedTransfer, Transfers::sendPerformances, Transfers::acceptPerformances]
license: EPL-2.0
---

# Transfers

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Semantic Library/Transfers.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package Transfers {
    doc
    /*
     * This package defines the transfer interactions used to type flows.
     */

    private import Base::Anything;
    private import Occurrences::*;
    private import Links::*;
    private import Objects::BinaryLinkObject;
    private import Performances::Performance;
    private import Performances::performances;
    private import ScalarValues::Boolean;
    private import ScalarValues::Natural;
    private import SequenceFunctions::*;
    
    interaction Transfer specializes Performance, BinaryLink {
        doc
        /*
         * A Transfer represents the transfer of a payload from the source of the interaction 
         * to the target of the interaction.
         */
    
        end feature source: Occurrence redefines BinaryLink::source {
            doc
            /*
             * The entity whose output is the source of the payload to be transferred.
             */
        
            feature sourceOutput: Anything[0..*];
        }
        
        end feature target: Occurrence redefines BinaryLink::target {
            doc
            /*
             * The entity whose input is the target of the payload to be transferred.
             */
        
            feature targetInput: Anything[0..*];
        }
        
        feature isInstant: Boolean[1] {
            doc
            /*
             * If isInstant is true, then the transfer is instantaneous.
             */
        }
        
        feature payload: Anything[1..*] {
            doc
            /*
             * The things that are to be transferred.
             */
        }
        
        feature payloadNum: Natural [1] = size(payload);
        
        private instantNum: Natural[1] = if isInstant? 1 else 0;
        private binding instant[instantNum] of [0..1] startShot = [0..1] endShot {
            doc
            /*
             * If isInstant is true, then the start and end of the transfer happen at the same time.
             */
        }
    }
    
    interaction MessageTransfer specializes Transfer {
        doc
        /*
         * A MessageTransfer is a Transfer that does not specify where the payload is picked
         * up and dropped off (see FlowTransfer). They are sent by SendPerformances and
         * accepted by AcceptPerformances.
         */
    }
     
    interaction FlowTransfer specializes Transfer disjoint from MessageTransfer {
        doc
        /*
         * A FlowTransfer is a Transfer identifying an output feature of the source from which
         * to pick up a payload and an input feature of the target to which to drop it off. They can
         * start when the payload is available at the source and move or copy it to the target.
         */
         
        feature isMove: Boolean[1] default true {
            doc
            /*
             * If isMove is true, then the entire payload leaves the source at the start
             * of the transfer.
             */
        }
        
        feature isPush: Boolean[1] default true {
            doc
            /*
             * If isPush is true, then the transfer begins when the payload is available
             * at the source.
             */
        }
        
        connector sourceOutputLink: BinaryLinkObject[payloadNum] {
            doc
            /*
             * The output of the payloads from the sourceOutput.
             */
        
            end [1] feature transferSource references source;
            end [payloadNum] feature transferPayload references payload subsets transferSource.sourceOutput;
        }
        
        connector targetInputLink: BinaryLinkObject[payloadNum] {
            doc
            /*
             * The input of the payload to the targetInput.
             */
        
            end [1] feature transferTarget references target;
            end [payloadNum] feature transferPayload references payload subsets transferTarget.targetInput;
        }
        
        private connector sending: HappensDuring[payloadNum] from [1] startShot to [payloadNum] sourceOutputLink {
          doc
            /*
             * The start of the transfer happens during the output of each of the payloads from the
             * source. 
             */
        }
        
        private connector moving: HappensWhile[0..*] from [0..*] sourceOutputLink.endShot to [0..1] startShot {
            doc
            /*
             * If isMove is true, then all payloads leave the source at the start
             * of the transfer.
             */
        }
        private inv { isMove implies size(moving) == size(sourceOutputLink) }
        
        private connector pushing: HappensWhile[0..*] from [0..*] sourceOutputLink.startShot to [0..1] startShot {
            doc
            /*
             * If isPush is true, then the transfer begins when the payloads are available
             * at the source.
             */
        }
        private inv { isPush implies size(pushing) == size(sourceOutputLink) }
        
        private connector delivering: HappensWhile[payloadNum] from [payloadNum] targetInputLink.startShot to [1] endShot {
            doc
            /*
             * The input of each of the payloads to the target starts at the end of the transfer.
             */
        }
    }
    
    interaction TransferBefore specializes Transfer, HappensBefore intersects Transfer, HappensBefore {
        doc
        /*
         * TransferBefore is a specialization of Transfer in which the source happens before
         * the transfer, which happens before the target.
         */
    
        end feature source: Occurrence redefines Transfer::source, HappensBefore::earlierOccurrence;
        end feature target: Occurrence redefines Transfer::target, HappensBefore::laterOccurrence;
        
        feature self: TransferBefore redefines Performance::self;
        
        private succession source then self;
        private succession self then target;
    }
    
    interaction FlowTransferBefore specializes TransferBefore, FlowTransfer intersects FlowTransfer, TransferBefore {
        doc
        /*
         * FlowTransferBefore is a FlowTransfer that is also a TransferBefore. 
         */
         
        end feature source: Occurrence redefines Transfer::source, TransferBefore::source;
        end feature target: Occurrence redefines Transfer::target, TransferBefore::target;         
    }
    
    abstract step transfers: Transfer[0..*] nonunique subsets performances, binaryLinks {
        doc
        /*
         * transfers is a specialization of performances and binaryLinks restricted to type 
         * Transfer.
         */
    
        end feature source: Occurrence redefines Transfer::source, binaryLinks::source;
        end feature target: Occurrence redefines Transfer::target, binaryLinks::target;
    }
    
    abstract step messageTransfers: MessageTransfer[0..*] nonunique subsets transfers {
        doc
        /*
         * messageTransfers is a specialization of transfers restricted to type MessageTransfers.
         */
        
        end feature source: Occurrence redefines MessageTransfer::source, transfers::source;
        end feature target: Occurrence redefines MessageTransfer::target, transfers::target;      
    }
    
    abstract flow flowTransfers: FlowTransfer[0..*] nonunique subsets transfers {
        doc
        /*
         * flowTransfers is a specialization of transfers restricted to type FlowTransfers.
         * It is the default subsetting for non-succession flows.
         */
         
        end feature source: Occurrence redefines FlowTransfer::source, transfers::source;
        end feature target: Occurrence redefines FlowTransfer::target, transfers::target;
    }
      
    abstract flow transfersBefore: TransferBefore[0..*] nonunique subsets transfers, happensBeforeLinks
        intersects transfers, happensBeforeLinks {
        doc
        /*
         * transfersBefore is a specialization of transfers and happensBeforeLinks restricted to
         * type TransferBefore.
         */
    
        end feature source: Occurrence redefines TransferBefore::source, transfers::source, happensBeforeLinks::earlierOccurrence;
        end feature target: Occurrence redefines TransferBefore::target, transfers::target, happensBeforeLinks::laterOccurrence;
    }
    
    abstract flow flowTransfersBefore: FlowTransferBefore[0..*] nonunique subsets flowTransfers, transfersBefore
        intersects flowTransfers, transfersBefore {
        doc
        /*
         * flowTransfersBefore is a specialization of flowTransfers and transfersBefore that is restricted
         * to type FlowTransferBefore. IT is the default subsetting for succession flows.
         */
    
        end feature source: Occurrence redefines FlowTransferBefore::source, flowTransfers::source, transfersBefore::source;
        end feature target: Occurrence redefines FlowTransferBefore::target, flowTransfers::target, transfersBefore::target;
    }

    behavior SendPerformance specializes Performance  {
        doc
        /*
         * SendPerformances are Performance that require an outgoingTransferFromSelf 
         * from a designated sender Occurrence carrying a given payload, optionally to a designated receiver.
         */
    
        in feature payload [0..*];
        in feature sender: Occurrence[1] default this;
        in feature receiver: Occurrence[0..1];
        feature sentTransfer: MessageTransfer [1] subsets sender.outgoingTransfersFromSelf {
            feature redefines payload = SendPerformance::payload;
        }
        binding [0..1] receiver.incomingTransfersToSelf = [1] sentTransfer;

        succession self then sentTransfer;
    }
    
    behavior AcceptPerformance specializes Performance {
        doc
        /*
         * AcceptPerformance is a performance that requires an incomingTransferToSelf
         * of a desigated receiver Occurrence, providing its payload as output.
         */
        inout feature payload[0..*];
        in feature receiver: Occurrence[1] default this;
        feature acceptedTransfer: MessageTransfer[1] subsets receiver.incomingTransfersToSelf;
        succession acceptedTransfer then self.endShot;
        
        binding payload = acceptedTransfer.payload;
    }

    abstract step sendPerformances: SendPerformance[0..*] nonunique subsets performances {
        doc
        /*
         * sendPerformances is a specialization of performances for SendPerformances.
         */
    }
        
    abstract step acceptPerformances: AcceptPerformance[0..*] nonunique subsets performances {
        doc
        /*
         * acceptPerformances is a specialization of performances for AcceptPerformances.
         */
    }
}
```

## Declarations

- `Transfers` — standard library package
- `Transfers::Transfer` — interaction
- `Transfers::Transfer::source` — feature
- `Transfers::Transfer::source::sourceOutput` — feature
- `Transfers::Transfer::target` — feature
- `Transfers::Transfer::target::targetInput` — feature
- `Transfers::Transfer::isInstant` — feature
- `Transfers::Transfer::payload` — feature
- `Transfers::Transfer::payloadNum` — feature
- `Transfers::Transfer::instant` — binding
- `Transfers::MessageTransfer` — interaction
- `Transfers::FlowTransfer` — interaction
- `Transfers::FlowTransfer::isMove` — feature
- `Transfers::FlowTransfer::isPush` — feature
- `Transfers::FlowTransfer::sourceOutputLink` — connector
- `Transfers::FlowTransfer::sourceOutputLink::transferSource` — feature
- `Transfers::FlowTransfer::sourceOutputLink::transferPayload` — feature
- `Transfers::FlowTransfer::targetInputLink` — connector
- `Transfers::FlowTransfer::targetInputLink::transferTarget` — feature
- `Transfers::FlowTransfer::targetInputLink::transferPayload` — feature
- `Transfers::FlowTransfer::sending` — connector
- `Transfers::FlowTransfer::moving` — connector
- `Transfers::FlowTransfer::pushing` — connector
- `Transfers::FlowTransfer::delivering` — connector
- `Transfers::TransferBefore` — interaction
- `Transfers::TransferBefore::source` — feature
- `Transfers::TransferBefore::target` — feature
- `Transfers::TransferBefore::self` — feature
- `Transfers::FlowTransferBefore` — interaction
- `Transfers::FlowTransferBefore::source` — feature
- `Transfers::FlowTransferBefore::target` — feature
- `Transfers::transfers` — step
- `Transfers::transfers::source` — feature
- `Transfers::transfers::target` — feature
- `Transfers::messageTransfers` — step
- `Transfers::messageTransfers::source` — feature
- `Transfers::messageTransfers::target` — feature
- `Transfers::flowTransfers` — flow
- `Transfers::flowTransfers::source` — feature
- `Transfers::flowTransfers::target` — feature
- `Transfers::transfersBefore` — flow
- `Transfers::transfersBefore::source` — feature
- `Transfers::transfersBefore::target` — feature
- `Transfers::flowTransfersBefore` — flow
- `Transfers::flowTransfersBefore::source` — feature
- `Transfers::flowTransfersBefore::target` — feature
- `Transfers::SendPerformance` — behavior
- `Transfers::SendPerformance::payload` — feature
- `Transfers::SendPerformance::sender` — feature
- `Transfers::SendPerformance::receiver` — feature
- `Transfers::SendPerformance::sentTransfer` — feature
- `Transfers::AcceptPerformance` — behavior
- `Transfers::AcceptPerformance::payload` — feature
- `Transfers::AcceptPerformance::receiver` — feature
- `Transfers::AcceptPerformance::acceptedTransfer` — feature
- `Transfers::sendPerformances` — step
- `Transfers::acceptPerformances` — step
