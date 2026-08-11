---
name: Metadata
kind: model-library-file
language: SysML
source: sysml.library/Systems Library/Metadata.sysml
declares: [Metadata, Metadata::MetadataItem, Metadata::metadataItems]
license: EPL-2.0
---

# Metadata

Verbatim SysML standard-library source from `sysml.library/Systems Library/Metadata.sysml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```sysml
standard library package Metadata {
doc
/*
 * This package defines the base types for metadata definitions and related 
 * metadata annotations in the SysML language.
 */

	private import Metaobjects::Metaobject;
	private import Metaobjects::metaobjects;
	private import Items::Item;
	private import Items::items;
	
	abstract metadata def MetadataItem :> Metaobject, Item {
		doc
		/*
		 * MetadataItem is the most general class of Items that represent Metaobjects. 
		 * MetadataItem is the base type of all MetadataDefinitions.
		 */
		 
		 ref self : MetadataItem redefines Metaobject::self, Item::self;
	}
	
	abstract item metadataItems : MetadataItem[0..*] :> metaobjects, items {
		doc
		/*
		 * metadataItems is the base feature of all MetadataUsages.
		 * 
		 * Note: It is not itself a MetadataUsage, because it is not being used as an
		 * AnnotatingElement here.
		 */
	}
}
```

## Declarations

- `Metadata` — standard library package
- `Metadata::MetadataItem` — metadata def
- `Metadata::metadataItems` — item
