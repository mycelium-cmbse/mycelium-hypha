---
name: api-lookup
description: "Look up the OMG Systems Modeling API & Services (the SysML v2 API, not the language metamodel): a PIM type's attributes, multiplicities, derivation, subsetting and redefinition (Project, Commit, Branch, Tag, DataVersion, DataIdentity, Query, PrimitiveConstraint, ...), a PIM service operation, which PSM (OpenAPI) schema a PIM type maps to and which of its members the request and response models carry, an endpoint, or a clause of the API specification. Use when the user asks how the SysML v2 API models something, whether a PIM member is in a request or response model, or which endpoint a service operation maps to."
---

# Systems Modeling API lookup

Answer questions about the OMG **Systems Modeling API and Services** specification from the **hypha
MCP server's tools**: its Platform Independent Model (PIM), its REST/HTTP Platform Specific Model
(PSM, the OpenAPI document), and the cross-reference that lines the two up with the specification
clauses. The tools are named here without a client's prefix: a client may show `hypha_api` as, for
example, `mcp__hypha__hypha_api`. Be exact, and say where each fact comes from.

This is the API layer. For the language metamodel (`PartUsage`, `Feature`, ...) use
`metamodel-lookup` instead.

## API versions

The API is versioned **apart from the language releases**: it has its own versions and its own
default. `hypha_release_status` lists the installed API versions (`apiVersions`) and the default one
(`apiDefault`).

- `hypha_api` answers from the **default** API version unless it is given a `version`. **Say which
  one answered**, as every result's grounding names it: "in API version 2026-04, `Commit` ...".
- An API version is a tag of the upstream API-Services repository, `2025-02` or later. It is not a
  language release tag, and the two do not have to match.
- If no API version is installed, say so, and offer to install one through the `version-management`
  skill. Ask first, as it needs the network.

## Tools

`hypha_api` with the `question`:

| Question | `question` | Arguments |
| --- | --- | --- |
| A PIM class, including the six services: its attributes (owned and inherited), operations, supertypes and documentation | `pim-class` | `name`, such as `Project` or `ProjectService` |
| How a PIM type lines up with the PSM and the specification: its clauses, its PSM schemas, and which of its members each model carries | `type` | `name`, such as `Commit` |
| A PIM service operation: the clauses that list it and the endpoints it maps to | `operation` | `name`, qualified by its service, such as `ProjectService.createProject` |
| A clause of the API specification, with its verbatim text | `clause` | `clause`, such as `7.1.4` |

Every one takes `version` for another installed API version.

## Reading an answer

Every result carries a `value`, a `grounding` (tier, API version and the generated file it was read
from) and, when part of the knowledge was never generated, `unavailable` saying why and how to
generate it. A `null` value means the API version has no such class, type, operation or clause: say
so, and never invent one.

### A PIM class (`pim-class`)

The same shape as a metaclass of the language metamodel, because the PIM is a UML model too: `name`,
`package`, `isAbstract`, `extends`, `allAncestors`, `ownedAttributes` and `inheritedAttributes` (the
*full effective* set, with `inheritedFrom`: never re-walk the supertypes), the operations and the
`documentation`. An attribute carries `lower`, `upper` (`-1` is unbounded: write it `*`),
`isDerived`, `redefines` and `subsets`, each reference naming the feature and its owner. The six
**services** (`ProjectService`, `ProjectDataVersioningService`, `QueryService`, ...) are classes whose
content is their operations.

### How a type lines up (`type`)

- `clauses`: the specification clauses that cover the type, each with the `method` that found it.
- `schema`: its PSM `response` and `request` schemas, each with the method that links it.
- `members`: every effective PIM attribute with its `multiplicity` and `derived` flag, and whether the
  **response** and the **request** model have a member of the same name. A member with
  `response: false` is a PIM attribute the response model leaves out.
- `psmOnly`: members of the response model that are not PIM attributes of the type.

Read a name mismatch from `members` and `psmOnly` together: a PIM attribute with `response: false`
next to a similarly named `psmOnly` entry (PIM `previousCommits`, PSM `previousCommit`) is one concept
under two spellings. Report both spellings; do not pick one.

### An operation (`operation`)

`clauses` lists where the specification lists the operation, and `endpoints` the endpoints the
specification maps it to: each with its HTTP `method`, its `path` and its OpenAPI `operationId`. An
operation with `mappingClause` set and no `endpoints` **is** mapped, but in prose that spans several
table rows: read that clause with `question: "clause"`, or name it when its text is not generated.

### How far to trust a link

Every link between the PIM, the PSM and the specification names the method that produced it. Say
which kind you are relying on:

| Method | Meaning |
| --- | --- |
| `mapping-table` | Stated by one of the specification's PIM-to-PSM mapping tables. |
| `type-constant` | Stated by the OpenAPI document: a request schema whose `@type` may only be that type. |
| `clause-title`, `definition-paragraph`, `operation-signature` | The clause is about the type or lists the operation. |
| `same-name` | Inferred: a PSM schema carries the PIM type's name. Not stated anywhere. |
| `bold-mention` | Weak: the name is set in bold in the clause, typically in a figure. |

A type or operation with no clauses, or no schema, can mean the specification or the OpenAPI document
was not present when the knowledge was generated: when `unavailable` says so, report that rather than
concluding the link does not exist.

## What the tools do not answer yet

The **properties of a PSM schema** (each member's type, whether it is required, whether it is
nullable, and what an `Identified` member refers to) are not answered by any tool yet. `type` says
which PIM members the request and response models carry, but not those flags. When the user asks for
them, say plainly that this version of hypha cannot answer them, rather than guessing from the PIM
multiplicities: a PIM attribute's multiplicity does not say whether the wire format requires or
nulls it.

## Procedure

1. Pick the `question` from the table above; pass `version` only when the user names one.
2. A question about **one PIM type** (its attributes, multiplicities, derivation, subsetting):
   `pim-class`.
3. A question that **compares** the PIM with the PSM (which members a request or response carries),
   or asks for a schema: `type`. For an endpoint: `operation`.
4. State the API version, and say which method backs any link you report.
5. Quote specification wording only from `question: "clause"`, following the `spec-citation` skill:
   the cross-reference holds clause numbers, never wording.
