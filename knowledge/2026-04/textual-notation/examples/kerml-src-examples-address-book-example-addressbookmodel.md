---
name: AddressBookModel
kind: example
language: KerML
source: kerml/src/examples/Address Book Example/AddressBookModel.kerml
elements: []
license: EPL-2.0
---

# AddressBookModel

Verbatim KerML model from `kerml/src/examples/Address Book Example/AddressBookModel.kerml` (EPL-2.0; see [NOTICE](../../../NOTICE)).

```kerml
private import ScalarValues::*;
package AddressBookModel {
	
	class Entry {
		name: String;
		address: String;
	}
	
	class AddressBook {
		entries: Entry[*];
	}
	
}
```
