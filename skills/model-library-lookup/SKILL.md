---
name: model-library-lookup
description: Look up what a SysML v2 / KerML standard model library element means or where it is declared (e.g. ISQBase::mass, ScalarValues::Real, SysML.sysml's Parts::Part) – the normative library packages a user's own model specializes with `:>`, not the metamodel itself. Use when the user references a qualified name from ISQ, ScalarValues, SysML.sysml, or asks what a standard-library type/attribute/part is.
---

# Standard model library lookup

Answer questions about the SysML v2 / KerML **standard model libraries** – the normative library
packages (`ISQBase::mass`, `ScalarValues::Real`, `Parts::Part`, ...) a user's own model specializes,
not the metamodel that describes what a metaclass *is*. Be exact and cite the source file.

## Releases

The knowledge base is generated **per upstream release tag** (`YYYY-MM`, e.g. `2026-05`). Read
`knowledge/versions.json` first: it lists the installed tags newest first and names the `default`
one to answer from.

```sh
jq -r '.default, (.versions[].tag)' knowledge/versions.json
```

Then substitute that tag for `<tag>` in every path below.

- Answer from the **default** release unless the user names one.
- **Say which release you answered from.** The standard libraries change between releases like
  everything else in the knowledge base.
- If the user asks about a release that is not installed, say so and list the ones that are.

## What this is – and is not

This is the **declaration surface**: a qualified name, the literal keyword that introduced it (e.g.
`attribute def`, `datatype`, `part`), and where it is declared. It is built by a lightweight scanner
over the verbatim library source, not a real parser.

It does **not** resolve `:>`/`:>>` specialization or redefinition chains, does not know a library
element's supertypes or effective feature set, and cannot tell a public member from a private one.
If asked "what does `ISQBase::LengthValue` inherit" or "what is `mass`'s full feature set", say
plainly that this index does not compute that – read the page's verbatim source and follow its own
`:>`/`:>>` references by hand, or use `metamodel-lookup` for the *metaclass* (e.g. `AttributeUsage`)
the declaration is written as. Do not fabricate a resolved structure to fill the gap.

## Knowledge base

- `knowledge/<tag>/model-library/index.json` — `declarations`, keyed by qualified name (e.g.
  `"ISQBase::mass"`), each `{kind, file, source}`: `kind` is the literal introducing keyword(s),
  `file` is the package page relative to `model-library/`, `source` is the upstream path under
  `sysml.library/`. `counts` gives the number of files and distinct qualified names.
- `knowledge/<tag>/model-library/packages/<slug>.md` — one page per standard-library file: front
  matter (`name`, `language`, `source`, `declares`, `license`), the **verbatim** KerML/SysML source in
  a fenced block, and a `## Declarations` list of every qualified name that page declares.
- `knowledge/<tag>/model-library/index.md` — human-readable index, grouped by upstream folder
  (`Domain Libraries`, `Kernel Libraries`, `Systems Library`).

**Same-file collisions keep the first declaration** (rare – two anonymous nested redefinition blocks
in one file both introducing a child with the same name); the page itself still lists every
declaration, so read the page's `## Declarations` section, not just the index, when a name might be
ambiguous within one file.

## Procedure

1. Resolve the qualified name against the index:
   ```sh
   jq '.declarations["ISQBase::mass"]' knowledge/<tag>/model-library/index.json
   ```
   Without `jq`, grep `knowledge/<tag>/model-library/index.md` for the file, or grep
   `knowledge/<tag>/model-library/packages/` for the qualified name directly.
2. Read the linked page (`file` in the index entry). Locate the declaration in the verbatim source –
   its surrounding `doc /* ... */` block, if any, is the closest thing to prose documentation this
   index has, and its own `:>`/`:>>` clause names what it specializes or redefines.
3. Quote the relevant source lines as the answer, not a paraphrase – there is no synthesized prose
   here, only the source itself. Always include the source file path and the release tag.
4. If the qualified name is not in the index, say so – never invent a library element. Check whether
   the user meant a metaclass instead (`metamodel-lookup`) or a different release.

## Provenance

Everything here is `MODEL` tier: read directly from the verbatim standard-library source, the same
way an element file's own features are. Nothing is `DERIVED` – the scanner does not compute a
specialization graph – and nothing is `NORMATIVE` in the `spec-citation` sense, since the library
source itself, not the specification text, is what is being quoted.

## Related skills

- **`metamodel-lookup`** — "what *kind* of thing is `AttributeUsage`", or any question about the
  metamodel structure a library declaration is written against.
- **`spec-citation`** — the normative specification wording behind a library concept (e.g. why ISQ
  defines quantities the way it does), rather than the library's own source text.
