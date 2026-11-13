---
name: model-library-lookup
description: "Look up what a SysML v2 / KerML standard model library element means or where it is declared (e.g. ISQBase::mass, ScalarValues::Real, SysML.sysml's Parts::Part): the normative library packages a user's own model specializes with `:>`, not the metamodel itself. Use when the user references a qualified name from ISQ, ScalarValues, SysML.sysml, or asks what a standard-library type/attribute/part is."
---

# Standard model library lookup

Answer questions about the SysML v2 / KerML **standard model libraries**: the normative library
packages (`ISQBase::mass`, `ScalarValues::Real`, `Parts::Part`, ...) a user's own model specializes,
not the metamodel that describes what a metaclass *is*. Answer from the **hypha MCP server's tools**,
named here without a client's prefix. Be exact, and quote the source.

## Which release

- Every tool answers from the **default** release unless it is given a `tag`; call
  `hypha_release_status` when unsure what is installed.
- **Say which release answered.** The standard libraries change between releases like everything else.
- If the user asks about a release that is not installed, say so and name the installed ones.

## What this is, and what it is not

This is the **declaration surface**: a qualified name, the keyword that introduced it (such as
`attribute def`, `datatype` or `part`), where it is declared, and the verbatim library source around
it. It is built by a lightweight scanner over the library source, not a real parser.

It does **not** resolve `:>`/`:>>` specialization or redefinition chains, does not know a library
element's supertypes or effective feature set, and cannot tell a public member from a private one. If
asked "what does `ISQBase::LengthValue` inherit" or "what is `mass`'s full feature set", say plainly
that this does not compute that. Follow the source's own `:>`/`:>>` references by looking each one up
in turn, or use `metamodel-lookup` for the *metaclass* the declaration is written as (such as
`AttributeUsage`). Never fabricate a resolved structure to fill the gap.

## Tools

- `hypha_lookup` with `kind: "model-library"` and `name`, the qualified name (`ISQBase::mass`): the
  library page from that declaration on, a page at a time. The answer's `status` says what was found:
  - `found`: `content` holds the verbatim source, `path` the page, and `declarationLine` the line the
    declaration is on. The name may have matched loosely (ignoring case, or a trailing part such as
    `mass`), so report the qualified `name` the answer gives, not the one asked. When `nextLine` is
    set, call again with it as `startLine` for the rest.
  - `ambiguous`: several declarations match equally well (the same short name in two packages);
    `matches` lists them. Ask the user, or pick the one the context makes plain, and look that up.
  - `candidates`: nothing matched the name itself; `matches` only contain it. They are suggestions,
    never the answer, even when there is one.
  - `not-found`: the release declares nothing of that name.
- `hypha_search` with `text` and `kind: "model-library"`: find the qualified name of a declaration
  from a partial name, such as `mass`.
- The resource `hypha://<tag>/model-library/<QualifiedName>`, for a client that reads resources: the
  same page for an exact qualified name only, so find the name first.

## Procedure

1. Look the qualified name up with `hypha_lookup` (`kind: "model-library"`). If it is not exact, use
   the `matches` it returns, or `hypha_search`.
2. Read the declaration in the returned source. Its surrounding `doc /* ... */` block, if any, is the
   closest thing to prose documentation, and its own `:>`/`:>>` clause names what it specializes or
   redefines.
3. Quote the relevant source lines as the answer, not a paraphrase: there is no synthesized prose,
   only the source itself. Name the release, and the page the grounding names.
4. If the name is not declared, say so; never invent a library element. Check whether the user meant a
   metaclass instead (`metamodel-lookup`) or a different release.

## Provenance

Everything here is `MODEL` tier: read directly from the verbatim standard-library source, as a
metaclass's own features are. Nothing is `DERIVED` (the scanner computes no specialization graph), and
nothing is `NORMATIVE` in the `spec-citation` sense, since the library source, not the specification
text, is what is quoted.

## Related skills

- **`metamodel-lookup`**: "what *kind* of thing is `AttributeUsage`", or any question about the
  metamodel structure a library declaration is written against.
- **`spec-citation`**: the normative specification wording behind a library concept, rather than the
  library's own source text.
