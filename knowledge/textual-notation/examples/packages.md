---
name: Packages, imports and aliases
kind: example
language: SysML
constructs: [package, import, alias]
elements: [Package, MembershipImport, NamespaceImport, PartDefinition, Membership]
source: "sources/textual/sysml/training/01. Packages/Package Example.sysml"
license: EPL-2.0
---

# Packages, imports and aliases

A worked SysML v2 textual-notation example: grouping members in a package, importing from other
namespaces with visibility, and defining aliases.

## Notation

```sysml
package 'Package Example' {
	public import ISQ::TorqueValue;
	private import ScalarValues::*;
	 
	private part def Automobile;
	
	public alias Car for Automobile;	                         
	alias Torque for ISQ::TorqueValue;
}
```

## Constructs & elements

- `package … { }` – [Package](../../sysml2/elements/Package.md): a namespace that owns its members.
- `public import X::Y` – [MembershipImport](../../sysml2/elements/MembershipImport.md): import a single named member.
- `private import X::*` – [NamespaceImport](../../sysml2/elements/NamespaceImport.md): import all members of a namespace; `public`/`private` set the import's visibility.
- `part def` – [PartDefinition](../../sysml2/elements/PartDefinition.md).
- `alias C for Automobile` – an aliasing [Membership](../../sysml2/elements/Membership.md): a second name for an existing member.

## Source

`sources/textual/sysml/training/01. Packages/Package Example.sysml` – from
Systems-Modeling/SysML-v2-Release (EPL-2.0); see [NOTICE](../../../NOTICE).
