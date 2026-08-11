---
name: Links
kind: model-library-file
language: KerML
source: sysml.library/Kernel Libraries/Kernel Semantic Library/Links.kerml
declares: [Links, Links::Link, Links::Link::participant, Links::BinaryLink, Links::BinaryLink::participant, Links::BinaryLink::source, Links::BinaryLink::target, Links::SelfLink, Links::SelfLink::thisThing, Links::SelfLink::sameThing, Links::links, Links::binaryLinks, Links::selfLinks, Links::selfLinks::thisThing, Links::selfLinks::sameThing]
license: EPL-2.0
---

# Links

Verbatim KerML standard-library source from `sysml.library/Kernel Libraries/Kernel Semantic Library/Links.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
standard library package Links {
    doc
    /*
     * This package defines associations and features that are related to the typing of links.
     */

    private import Base::Anything;
    private import Base::things;
    
    abstract assoc Link specializes Anything {
        doc
        /*
         * Link is the most general association between two or more things.
         */

        feature participant: Anything[2..*] nonunique ordered;
    }
    
    assoc all BinaryLink specializes Link {
        doc
        /*
         * BinaryLink is the most general binary association between exactly two things, 
         * nominally directed from source to target.
         */
         
        feature participant: Anything[2] nonunique ordered redefines Link::participant;
        
        end feature source: Anything[1] subsets participant;
        end feature target: Anything[1] subsets participant;
    }
    
    assoc all SelfLink specializes BinaryLink {
        doc
        /*
         * SelfLink is a binary association in which the things at the two ends are asserted
         * to be the same.
         */
        
        end feature thisThing: Anything redefines source subsets sameThing crosses sameThing.self;
        end self2 [1] feature sameThing: Anything redefines target subsets thisThing;
    }
        
    abstract feature links: Link[0..*] nonunique subsets things {
        doc
        /*
         * links is the most general feature of links between individuals.
         */
    }
    
    abstract feature binaryLinks: BinaryLink[0..*] nonunique subsets links {
        doc
        /*
         * binaryLinks is a specialization of links restricted to type BinaryLink.
         */
    }
    
    abstract feature selfLinks: SelfLink[0..*] nonunique subsets binaryLinks {
        doc
        /*
         * selfLinks is a specialization of binaryLinks restricted to type SelfLink.
         */

        end feature thisThing: Anything redefines SelfLink::thisThing, binaryLinks::source;
        end feature sameThing: Anything redefines SelfLink::sameThing, binaryLinks::target;
    }

}
```

## Declarations

- `Links` — standard library package
- `Links::Link` — assoc
- `Links::Link::participant` — feature
- `Links::BinaryLink` — assoc
- `Links::BinaryLink::participant` — feature
- `Links::BinaryLink::source` — feature
- `Links::BinaryLink::target` — feature
- `Links::SelfLink` — assoc
- `Links::SelfLink::thisThing` — feature
- `Links::SelfLink::sameThing` — feature
- `Links::links` — feature
- `Links::binaryLinks` — feature
- `Links::selfLinks` — feature
- `Links::selfLinks::thisThing` — feature
- `Links::selfLinks::sameThing` — feature
