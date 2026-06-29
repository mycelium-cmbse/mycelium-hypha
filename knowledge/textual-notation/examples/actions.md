---
name: Action definitions
kind: example
language: SysML
constructs: ["action def", action, in, out, flow, bind]
elements: [ActionDefinition, ActionUsage, ItemDefinition, ItemUsage, FlowUsage]
source: "sources/textual/sysml/training/14. Action Definitions/Action Definition Example.sysml"
license: EPL-2.0
---

# Action definitions

A worked SysML v2 textual-notation example: action definitions with input/output parameters,
nested actions, and item flows between them.

## Notation

```sysml
package 'Action Definition Example' {
	item def Scene;
	item def Image;
	item def Picture;

	action def Focus { in scene : Scene; out image : Image; }
	action def Shoot { in image: Image; out picture : Picture; }

	action def TakePicture { in scene : Scene; out picture : Picture;
		bind focus.scene = scene;

		action focus: Focus { in scene; out image; }

		flow from focus.image to shoot.image;

		action shoot: Shoot { in image; out picture; }

		bind shoot.picture = picture;
	}
}
```

## Constructs & elements

- `action def { in …; out … }` — [ActionDefinition](../../sysml2/elements/ActionDefinition.md); `in`/`out` are directed parameter features.
- `action focus : Focus` — [ActionUsage](../../sysml2/elements/ActionUsage.md): a step typed by an action definition.
- `item def` — [ItemDefinition](../../sysml2/elements/ItemDefinition.md); the parameters are [ItemUsage](../../sysml2/elements/ItemUsage.md) features.
- `flow from a to b` — an item flow between features (a [FlowUsage](../../sysml2/elements/FlowUsage.md) / flow connection).
- `bind x = y` — a binding connector equating two features.

## Source

`sources/textual/sysml/training/14. Action Definitions/Action Definition Example.sysml` — from
Systems-Modeling/SysML-v2-Release (EPL-2.0); see [NOTICE](../../../NOTICE).
