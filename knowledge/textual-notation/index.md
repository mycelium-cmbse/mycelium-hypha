# SysML v2 textual notation

> Entry point for the textual-notation reference used by the `sysml-validation` skill and the
> `sysml-validator` subagent. Partly curated, partly generated from the grammar (`bnf/`) and
> example sources in `sources/textual/`.

## Worked examples

Each example pairs a small, valid SysML v2 notation snippet with the metamodel elements it exercises
(linked to `knowledge/metamodel/elements/`). Curated from the SysML v2 release training set (EPL-2.0;
see [NOTICE](../../NOTICE)).

| Example | Demonstrates |
| --- | --- |
| [Packages, imports and aliases](examples/packages.md) | `package`, `import` (visibility), `alias` |
| [Part and attribute definitions](examples/part-definitions.md) | `part def`, `attribute def`, `part`, `attribute`, `ref part` |
| [Generalization and specialization](examples/generalization.md) | `abstract`, `specializes` / `:>`, multiple specialization |
| [Connections](examples/connections.md) | `connection def`, `end`, `connect … to`, multiplicities |
| [Ports](examples/ports.md) | `port def`, `port`, directed `in`/`out item` |
| [Action definitions](examples/actions.md) | `action def`, `action`, `in`/`out`, `flow`, `bind` |
| [State definitions and transitions](examples/states.md) | `state def`, `state`, `transition`, `accept` |
| [Constraints](examples/constraints.md) | `constraint def`, `constraint`, parameters, Boolean expression |
| [Requirements](examples/requirements.md) | `requirement def`, `subject`, `require`/`assume constraint`, `doc` |

## More constructs – reference models

Concepts without a dedicated worked example above are covered by these existing, committed model
files under `sources/textual/` (real, valid notation from the SysML v2 release set, EPL-2.0). Each is
paired with its metamodel element.

| Construct | Element | Reference model |
| --- | --- | --- |
| Item | [ItemDefinition](../metamodel/elements/ItemDefinition.md) | [Items Example](<../../sources/textual/sysml/training/08. Items/Items Example.sysml>) |
| Flow | [FlowUsage](../metamodel/elements/FlowUsage.md) | [Flow Definition Example](<../../sources/textual/sysml/training/13. Flows/Flow Definition Example.sysml>) |
| Enumeration | [EnumerationDefinition](../metamodel/elements/EnumerationDefinition.md) | [Enumeration Definitions-1](<../../sources/textual/sysml/training/06. Enumeration Definitions/Enumeration Definitions-1.sysml>) |
| Interface | [InterfaceDefinition](../metamodel/elements/InterfaceDefinition.md) | [Interface Example](<../../sources/textual/sysml/training/11. Interfaces/Interface Example.sysml>) |
| Occurrence | [OccurrenceDefinition](../metamodel/elements/OccurrenceDefinition.md) | [Event Occurrence Example](<../../sources/textual/sysml/training/27. Occurrences/Event Occurrence Example.sysml>) |
| Use case | [UseCaseDefinition](../metamodel/elements/UseCaseDefinition.md) | [Use Case Definition Example](<../../sources/textual/sysml/training/35. Use Cases/Use Case Definition Example.sysml>) |
| Analysis case | [AnalysisCaseDefinition](../metamodel/elements/AnalysisCaseDefinition.md) | [Analysis Case Definition Example](<../../sources/textual/sysml/training/33. Analysis/Analysis Case Definition Example.sysml>) |
| Verification case | [VerificationCaseDefinition](../metamodel/elements/VerificationCaseDefinition.md) | [Verification Case Definition Example](<../../sources/textual/sysml/training/34. Verification/Verification Case Definition Example.sysml>) |
| Calculation | [CalculationDefinition](../metamodel/elements/CalculationDefinition.md) | [Calculation Definitions](<../../sources/textual/sysml/training/30. Calculations/Calculation Definitions.sysml>) |
| Allocation | [AllocationDefinition](../metamodel/elements/AllocationDefinition.md) | [Allocation Definition Example](<../../sources/textual/sysml/training/38. Allocation/Allocation Definition Example.sysml>) |
| Metadata | [MetadataDefinition](../metamodel/elements/MetadataDefinition.md) | [Metadata Example-1](<../../sources/textual/sysml/training/39. Metadata/Metadata Example-1.sysml>) |
| View | [ViewDefinition](../metamodel/elements/ViewDefinition.md) | [Views Example](<../../sources/textual/sysml/training/42. Views/Views Example.sysml>) |
| Viewpoint | [ViewpointDefinition](../metamodel/elements/ViewpointDefinition.md) | [Viewpoint Example](<../../sources/textual/sysml/training/42. Views/Viewpoint Example.sysml>) |
| Rendering | [RenderingDefinition](../metamodel/elements/RenderingDefinition.md) | [Views Example](<../../sources/textual/sysml/training/42. Views/Views Example.sysml>) |
| Concern | [ConcernDefinition](../metamodel/elements/ConcernDefinition.md) | [ViewTest](<../../sources/textual/sysml/examples/Simple Tests/ViewTest.sysml>) |

## Grammar summary

Summarized from the committed grammar `sources/textual/bnf/SysML-textual-bnf.kebnf` (kernel layer in
`KerML-textual-bnf.kebnf`; EPL-2.0, see [NOTICE](../../NOTICE)). Read those files for the full BNF; the
[worked examples](#worked-examples) above show the forms in use.

### Declaration forms

Two shapes underlie almost every construct:

- **Definition** – `«keyword» def Name [specialization] { … }` (e.g. `part def`, `attribute def`,
  `action def`, `state def`, `connection def`, `port def`, `constraint def`, `requirement def`).
- **Usage** – `[«modifier»] «keyword» name [: Type] [multiplicity] [specialization] [= value] { … }`
  (e.g. `part eng : Engine[1]`, `attribute mass : Real`, `ref part driver : Person`).

Common **modifiers**: `abstract`, `ref`, `variation` / `variant`, `derived`, `ordered`, `nonunique`,
`constant`, and feature direction `in` / `out` / `inout`.

### Membership and visibility

- **Import** – `import Namespace::*;` (all members) or `import Namespace::Member;` (one); with a
  `public` / `private` / `protected` visibility prefix.
- **Alias** – `alias NewName for QualifiedName;`.
- **Documentation** – `doc /* … */`; comments via `comment` or `/* … */`.

### Relationship operators

| Symbol | Keyword | Meaning |
| --- | --- | --- |
| `:` | `defined by` | typing: a usage is defined by a definition |
| `:>` | `specializes` | specialization (between types) |
| `:>` | `subsets` | subsetting (between features) |
| `:>>` | `redefines` | redefinition |
| `::>` | `references` | referencing |
| `=>` | `crosses` | crossing |

Feature values bind with `=`; connections use `connect … to …`; flows use `flow … from … to …`.

## Keyword reference

Reserved keywords grouped by purpose; the main definitional keywords link to their metamodel element.

### Packaging & members
`package` ([Package](../metamodel/elements/Package.md)), `import`, `alias`, `library`, `standard`,
`public`, `private`, `protected`, `all`, `as`, `filter`.

### Structure
`attribute` ([AttributeDefinition](../metamodel/elements/AttributeDefinition.md)),
`item` ([ItemDefinition](../metamodel/elements/ItemDefinition.md)),
`part` ([PartDefinition](../metamodel/elements/PartDefinition.md)),
`port` ([PortDefinition](../metamodel/elements/PortDefinition.md)),
`connection` / `connect` ([ConnectionDefinition](../metamodel/elements/ConnectionDefinition.md)),
`interface` ([InterfaceDefinition](../metamodel/elements/InterfaceDefinition.md)),
`flow` ([FlowUsage](../metamodel/elements/FlowUsage.md)), `binding` / `bind`, `end`,
`occurrence` ([OccurrenceDefinition](../metamodel/elements/OccurrenceDefinition.md)), `individual`,
`snapshot`, `timeslice`, `event`, `message`.

### Values & types
`enum` ([EnumerationDefinition](../metamodel/elements/EnumerationDefinition.md)), `def`, `abstract`,
`ref`, `variation` / `variant`, `derived`, `ordered`, `nonunique`, `constant`, `true`, `false`, `null`.

### Behavior & actions
`action` ([ActionDefinition](../metamodel/elements/ActionDefinition.md)), `perform`, `do`, `send`,
`accept`, `after`, `at`, `via`, `fork`, `join`, `merge`, `decide`, `terminate`, `assign`,
`if` / `then` / `else`, `while` / `loop` / `until` / `for`.

### States
`state` ([StateDefinition](../metamodel/elements/StateDefinition.md)), `transition`, `entry`, `exit`,
`first` / `then`, `exhibit`, `when`, `accept`.

### Calculations & expressions
`calc` ([CalculationDefinition](../metamodel/elements/CalculationDefinition.md)), `return`, `and`, `or`,
`not`, `xor`, `implies`, `istype`, `hastype`, `meta`.

### Requirements, cases & views
`requirement` ([RequirementDefinition](../metamodel/elements/RequirementDefinition.md)),
`constraint` ([ConstraintDefinition](../metamodel/elements/ConstraintDefinition.md)), `require`,
`assume`, `assert`, `satisfy`, `subject`, `stakeholder`,
`concern` ([ConcernDefinition](../metamodel/elements/ConcernDefinition.md)), `actor`, `objective`,
`case` / `use` / `include` ([UseCaseDefinition](../metamodel/elements/UseCaseDefinition.md)),
`analysis` ([AnalysisCaseDefinition](../metamodel/elements/AnalysisCaseDefinition.md)),
`verification` / `verify` ([VerificationCaseDefinition](../metamodel/elements/VerificationCaseDefinition.md)),
`frame`, `view` ([ViewDefinition](../metamodel/elements/ViewDefinition.md)),
`viewpoint` ([ViewpointDefinition](../metamodel/elements/ViewpointDefinition.md)),
`render` / `rendering` ([RenderingDefinition](../metamodel/elements/RenderingDefinition.md)), `expose`,
`dependency`, `allocate` / `allocation` ([AllocationDefinition](../metamodel/elements/AllocationDefinition.md)),
`metadata` ([MetadataDefinition](../metamodel/elements/MetadataDefinition.md)), `language`, `rep`.

### Relationships & navigation
`specializes`, `subsets`, `redefines`, `references`, `crosses`, `defined by`, `from`, `to`, `of`,
`by`, `about`, `via`.
