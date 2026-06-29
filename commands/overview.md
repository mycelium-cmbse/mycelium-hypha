---
description: Overview of Hypha – its SysML v2 / KerML skills, how to use them, and example prompts
disable-model-invocation: true
---

Show the user the following overview of the Hypha plugin, verbatim.

# Hypha – SysML v2 / KerML assistant

Hypha grounds answers about the OMG **SysML v2 / KerML** metamodel, specification, and textual
notation in a built-in knowledge base. Just ask in natural language – the right skill is
selected automatically. Three capabilities:

- **Metamodel lookup** – how a metaclass is defined: features, supertypes, subtypes, redefinitions,
  multiplicities, constraints (and enumerations / primitive types).
  - *"What features does `PartUsage` own and inherit?"*
  - *"How does `ConnectionUsage` relate to `ConnectionDefinition`?"*
  - *"Which metaclasses specialize `Feature`?"*
- **Spec citation** – the exact normative wording, with a clause reference. (Needs the OMG PDFs locally;
  see below.)
  - *"What does the SysML v2 spec say about conformance?"*
  - *"Quote the normative rule for redefinition."*
- **Validation** – check SysML v2 / KerML textual notation against the grammar and metamodel, and
  explain any issues with fixes.
  - *"Is this valid SysML v2? `part def Vehicle { attribute mass : Real[2..1]; }`"*

## Spec citation needs the OMG PDFs

The specification text is **not shipped** (the OMG license forbids redistributing it). To enable spec
citation, download these three PDFs into `sources/specs/`, then regenerate `knowledge/spec/` by running
`tools/spec-extract` (see its README):

- https://github.com/Systems-Modeling/SysML-v2-Release/blob/master/doc/1-Kernel_Modeling_Language.pdf
- https://github.com/Systems-Modeling/SysML-v2-Release/blob/master/doc/2a-OMG_Systems_Modeling_Language.pdf
- https://github.com/Systems-Modeling/SysML-v2-Release/blob/master/doc/3-Systems_Modeling_API_and_Services.pdf

A SessionStart hook reminds you when they are missing. Metamodel lookup and validation work without them.
