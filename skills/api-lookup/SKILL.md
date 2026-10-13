---
name: api-lookup
description: Look up the OMG Systems Modeling API & Services (the SysML v2 API, not the language metamodel) – a PIM type's attributes, multiplicities, derivation, subsetting and redefinition (Project, Commit, Branch, Tag, DataVersion, DataIdentity, Query, PrimitiveConstraint, ...), a PIM service operation, the REST/HTTP PSM (OpenAPI) schema with its required and nullable members, an endpoint, or how a PIM type maps to its PSM schema. Use when the user asks how the SysML v2 API models something, whether a PIM member is in a request or response model, or which endpoint a service operation maps to.
---

# Systems Modeling API lookup

Answer questions about the OMG **Systems Modeling API and Services** specification from three
generated views of it: the Platform Independent Model (PIM), the REST/HTTP Platform Specific Model
(PSM, the OpenAPI document), and the cross-reference that lines the two up with the specification
clauses. Be exact and cite the file you read.

This is the API layer. For the language metamodel (`PartUsage`, `Feature`, ...) use
`metamodel-lookup` instead.

## API versions

The API is versioned **apart from the language releases**: it has its own manifest and its own
default. Read `knowledge/api/versions.json` first – it lists the installed API versions newest first
and names the `default` one.

```sh
jq -r '.default, (.versions[].tag)' knowledge/api/versions.json
```

Then substitute that version for `<version>` in every path below.

- Answer from the **default** API version unless the user names one, and **say which one you
  answered from** – e.g. "in API version 2026-04, `Commit` ...".
- An API version is a tag of the upstream API-Services repository, `2025-02` or later. It is not the language release
  tag in `knowledge/versions.json`, and the two do not have to match.
- If `knowledge/api/versions.json` does not exist, nothing is installed. Say so and offer to install
  one through the `version-management` skill (`hypha api discover`, then `hypha api fetch` and
  `hypha api generate`) – ask first, it needs the network.

## Knowledge base

```
knowledge/api/<version>/
├── pim/                    the Platform Independent Model
│   ├── index.md, index.json    every class, interface, data type and enumeration → its page
│   ├── elements/<Name>.md      one page per type
│   ├── metamodel.json          the same model as a structural graph
│   └── diagrams/               one class diagram per package
├── openapi/                the REST/HTTP PSM
│   ├── index.md, index.json    every schema and endpoint
│   ├── schemas/<Name>.md       one page per API schema
│   └── endpoints.md            every operation, grouped as the document groups them
├── spec/                   verbatim clause text of the API specification (see spec-citation)
└── cross-references.json   PIM type ↔ clause ↔ PSM schema, and PIM operation ↔ endpoint
```

### PIM pages (`pim/elements/<Name>.md`)

The same anatomy as a metamodel element page, because the PIM is a UML model too: front matter
(`name`, `package`, `isAbstract`, `generalizes`, `specializedBy`), then **Generalizations** /
**Specializations**, **Owned features** (visibility, type, multiplicity, `derived` / `composite` /
`ordered`, and any `Redefines ...` / `Subsets ...` line), an **Inherited features** table giving the
*full effective* attribute set – read it directly, do not re-walk the generalization chain –
**Operations** and **Constraints**.

Three things the metamodel pages do not have:

- The six **services** (`ProjectService`, `ProjectDataVersioningService`, `QueryService`, ...) are
  classes whose content is their **Operations**: one signature per operation, with parameter and
  return multiplicities and the operation's documentation.
- `Data` is an **interface** (`kind: interface`), with a **Realized by** list.
- `UUID`, `IRI` and `ISO8601DateTime` are **data types** (`kind: datatype`).

`pim/metamodel.json` holds the same content with the inheritance closures computed
(`.classes[] | {name, ownedAttributes, inheritedAttributes, ownedOperations}`); an attribute carries
`lower`, `upper` (`-1` is unbounded), `isDerived`, `redefines` and `subsets`.

```sh
jq '.classes[] | select(.name=="Project") | .ownedAttributes[] | {name, lower, upper, isDerived, subsets}' \
  knowledge/api/<version>/pim/metamodel.json
```

### PSM pages (`openapi/schemas/<Name>.md`)

One page per schema **of the API itself**. The front matter gives its `kind` (`object`, `union` or
`enumeration`; otherwise a plain JSON type) and its `role` – whether the endpoints use it in request bodies, in responses, or
both. The role is computed from the endpoints, not from a `Request` suffix in the name. Then a
**Properties** table: `Property | Type | Required | Nullable | Refers to`.

- **Required** and **Nullable** are independent: a member can be required and nullable.
- A member typed `Identified` carries only an `@id`. **Refers to** names the type that identifier
  points at, as the document states it.
- `@id` and `@type` are PSM members with no PIM counterpart of the same name.

The document also carries a schema, and a request twin, for every KerML / SysML metamodel element.
Those are listed by name in `openapi/index.json` (`namespace: "metamodel"`) and are **not** rendered
as pages – look the element itself up with `metamodel-lookup`.

`openapi/endpoints.md` lists every operation: method and path, `operationId`, parameters, request
body and each response with its body type.

## Cross-references

`cross-references.json` is what answers "how do these line up". Never guess a mapping from a name
when this file has it. Its shape is described by `knowledge/api-cross-references.schema.json`.

```sh
jq '.types["Commit"]' knowledge/api/<version>/cross-references.json
jq '.operations["ProjectService.createProject"]' knowledge/api/<version>/cross-references.json
```

- `.types["<Name>"].clauses` – the specification clauses that cover the type.
- `.types["<Name>"].schema` – its PSM `response` and `request` schemas.
- `.types["<Name>"].members` – every effective PIM attribute with its `multiplicity` and `derived`
  flag, and whether the **response** and the **request** model have a member of the same name.
  A row with `response: false` is a PIM member the response model leaves out.
- `.types["<Name>"].psmOnly` – members of the response model that are not PIM attributes of the type.
- `.operations["<Service>.<operation>"]` – the clause listing the operation, and the `endpoints` the
  specification maps it to (with the OpenAPI `operationId`).
- `.modelMapping` – the rows of the specification's PIM-to-PSM model mapping table, as written.

Read a name mismatch from `members` and `psmOnly` together: a PIM attribute with `response: false`
next to a similarly named `psmOnly` entry (PIM `previousCommits`, PSM `previousCommit`) is one
concept under two spellings. Report both spellings – do not pick one.

### How far to trust an edge

Every edge names the `method` that produced it. Say which kind you are relying on:

| `method` | Meaning |
| --- | --- |
| `mapping-table` | Stated by one of the specification's PIM-to-PSM mapping tables. |
| `type-constant` | Stated by the OpenAPI document: a request schema whose `@type` may only be that type. |
| `clause-title`, `definition-paragraph`, `operation-signature` | The clause is about the type or lists the operation. |
| `same-name` | Inferred: a PSM schema carries the PIM type's name. Not stated anywhere. |
| `bold-mention` | Weak: the name is set in bold in the clause, typically in a figure. |

`.sources` records which inputs were present when the file was generated. With
`specification: false` there are no clause edges and no operation-to-endpoint mapping; with
`openApi: false` there are no schemas. Say so rather than concluding a mapping does not exist.

An operation with `mappingClause` set and an empty `endpoints` list **is** mapped by the
specification, but in prose that spans several table rows. Read that clause in
`knowledge/api/<version>/spec/` – or, if the text is not generated, name the clause and say the
mapping has to be read there.

## Procedure

1. Read `knowledge/api/versions.json` and resolve the API version.
2. A question about **one PIM type** – its attributes, multiplicities, derivation, subsetting: read
   `pim/elements/<Name>.md`.
3. A question about **the wire format** – required, nullable, request versus response: read
   `openapi/schemas/<Name>.md` (and `<Name>Request.md` when the cross-reference names one).
4. A question that **compares** the two, or asks for an endpoint or a clause: read the entry in
   `cross-references.json` first, then the pages it points at.
5. State the API version, cite the files, and say which `method` backs any mapping you report.
6. Quote specification wording only from `spec/`, following the `spec-citation` skill. The
   cross-reference holds clause numbers, never wording.
