# mycelium-hypha

Hypha is the AI agent of the Mycelium ecosystem, a connecting filament between engineers and the
SysML v2 specification. It is packaged as a Claude plugin with skills and subagents for KerML / SysML
v2 metamodel lookup, standard model library lookup, normative spec citation, and SysML v2
textual-notation validation.

## What's in here

mycelium-hypha is two things in one repository:

1. **The `hypha` Claude plugin** – skills and subagents that give grounded answers about KerML and
   SysML v2, backed by a knowledge base fetched and generated on your own machine, on request.
   Nothing per-release is committed to this repository.
2. **The generation pipelines** (`tools/`) that build that knowledge base from the upstream OMG
   sources. They are not part of the installed plugin – see [Contributing](#contributing) if you
   want to work on them.

```
mycelium-hypha/
├── .claude-plugin/   Plugin + marketplace manifests
├── hooks/            SessionStart hook – checks installed releases against upstream
├── skills/           Conversational workflows (metamodel lookup, spec citation, validation, ...)
├── agents/           Subagents backing those skills
├── knowledge/        Knowledge base – fetched and generated locally, one folder per release tag
├── sources/          Raw OMG inputs – fetched locally, one folder per release tag
└── tools/            Generation pipelines (not part of the installed plugin)
```

## Install

1. In Claude Code, add the marketplace and install the plugin:

   ```
   /plugin marketplace add mycelium-cmbse/mycelium-hypha
   /plugin install hypha@mycelium
   ```

   `/plugin list` confirms it's installed — but installing adds a `SessionStart` hook, and that hook
   only runs at session start, so nothing about SysML v2/KerML data happens yet.

2. **Start a new session** (or restart your current one) so that hook actually runs. It quietly
   compares what's installed locally against what's offerable upstream and feeds the result into
   Claude's context — expect no visible output from this step.

3. Ask it a SysML v2/KerML question. Claude will report that nothing is installed yet, name the
   releases available upstream, and ask which one to fetch — this is the first visible sign anything
   happened. Confirm a release, and it fetches and generates it for you (see [Releases](#releases)).

4. Ask your real question. Metamodel lookup and validation now work from what's installed;
   **spec citation additionally needs the specification text generated locally** (see
   [The knowledge base](#the-knowledge-base)).

### Recommended: `jq`

Install [`jq`](https://jqlang.github.io/jq/) – `brew install jq`, `sudo apt install jq`, or
`winget install jqlang.jq`.

The metamodel ships as a structural graph (`knowledge/<tag>/metamodel/metamodel.json`, ~8 MB) with the
inheritance closures precomputed. Set-shaped and cross-cutting questions – *"which metaclasses have a
feature typed by `Expression`"*, *"every concrete subclass of `Usage`"* – are one query against it,
and `jq` is how the skills run that query. It is a small standalone binary with no runtime behind it.

Hypha still works without `jq`: the skills fall back to reading the per-element markdown, which is
slower, pulls far more into context, and cannot distinguish a field match from a mention in prose. If
you use metamodel lookup or validation regularly, install it.

Example prompts for step 4 above:

- *Metamodel lookup* – "What features does `PartUsage` own and inherit?", "How does `ConnectionUsage`
  relate to `ConnectionDefinition`?", or "Which metaclasses specialize `Feature`?"
- *Spec citation* – "What does the SysML v2 spec say about conformance?" or "Quote the normative rule
  for redefinition."
- *Validation* – "Is this valid SysML v2? `part def Vehicle { attribute mass : Real[2..1]; }`"

## Releases

KerML and SysML v2 are released on a rolling tag cadence (`YYYY-MM`), and hypha generates its
knowledge base **per release tag**. Nothing is pre-installed – the first thing a fresh install does
is ask which release to fetch.

Manage this conversationally, in plain language – "what releases are available", "fetch the latest",
"switch to 2026-04", "remove 2026-03" – and Claude drives the underlying `hypha` CLI for you (`check`
/ `fetch` / `generate` / `use` / `remove` – see [tools/hypha-cli](tools/hypha-cli/README.md) if you
want to run it yourself). It always says what the current default release is, and asks before
fetching (real time, real bandwidth) or removing (deletes that release's files for good).

The skills state which release an answer came from, and answer from the default unless you name one.

## The knowledge base

Once a release is fetched and generated, hypha answers from:

- **the metamodel** – every KerML / SysML v2 metaclass, its features, generalizations and constraints
- **the textual notation** – the full keyword reference, plus every example model the release ships
- **the model library** – the standard libraries (ISQ, ScalarValues, SysML.sysml, ...)
- **cross-references** – links from each metamodel element to the specification clause, grammar
  production and worked example that treat it, so an answer can point back to *why*

**Spec citation needs one more step.** Quoting the normative specification text verbatim needs the
three OMG PDFs, plus a full source checkout to regenerate them into text – not something an installed
plugin has. Without it, metamodel lookup and validation still work fully, and spec citation can still
name the governing clause, just not quote the text itself.

### Provenance

Facts carry one of three tiers, so a reader can tell what was read from what was inferred:
`NORMATIVE` (verbatim clause-anchored spec text), `MODEL` (read from the metamodel XMI) and
`DERIVED` (computed here – closures, name-matched cross-references). The tier definitions travel
with the data, in the `provenanceTiers` block of `cross-references.json`.

## Sources

The knowledge base comes from two upstream OMG repositories, one release tag at a time:

- [Systems-Modeling/SysML-v2-Pilot-Implementation](https://github.com/Systems-Modeling/SysML-v2-Pilot-Implementation) – the metamodel XMI.
- [Systems-Modeling/SysML-v2-Release](https://github.com/Systems-Modeling/SysML-v2-Release) – the specification PDFs, the textual-notation grammar, and the example models.

Exact upstream commits and versions are recorded in [sources/README.md](sources/README.md).

## License

This repository's own code and content are licensed under **Apache-2.0** – see [LICENSE](LICENSE) and
[NOTICE](NOTICE).

### Specifications & licensing

The knowledge base is built from third-party, separately-licensed inputs:

- The generated knowledge (`knowledge/<tag>/metamodel/`, `knowledge/<tag>/textual-notation/`) is a
  derivative "special purpose specification … based upon" the OMG specifications, used for
  informational purposes as permitted by the OMG specification license, and ships under this
  repository's Apache-2.0 license once generated. Nothing per-release is committed to this repository
  any more (see #106) – it is fetched and generated on your own machine instead – but the upstream OMG
  attributions and the OMG license text still apply, and are reproduced in [NOTICE](NOTICE).
- **OMG specification PDFs are copyrighted and never committed** – the OMG license forbids posting the
  specifications on a network, so they stay git-ignored (`sources/<tag>/specs/`), and the spec-derived
  `knowledge/<tag>/spec/` is git-ignored too. Obtain the PDFs from OMG:
  [KerML 1.0](https://www.omg.org/spec/KerML/1.0) (`formal/26-03-01`),
  [SysML 2.0](https://www.omg.org/spec/SysML/2.0) (`formal/26-03-02`),
  [Systems Modeling API & Services 1.0](https://www.omg.org/spec/SystemsModelingAPI/1.0) (`formal/26-03-04`).
- The **metamodel XMI** (`sources/<tag>/xmi/`) and the **textual-notation sources**
  (`sources/<tag>/textual/`) – both git-ignored, fetched per release – come from the SysML v2
  submission team's repositories under the **Eclipse Public License 2.0** (see [NOTICE](NOTICE) and
  [sources/README.md](sources/README.md)). The one exception is `sources/PrimitiveTypes.xmi` (the OMG
  UML primitives library, shared and tag-independent), which is committed.

## Contributing

See [CONTRIBUTING](.github/CONTRIBUTING.md). `dotnet test mycelium-hypha.sln` and `pytest` in
`tools/spec-extract` exercise the generators; CI runs both with SonarQube analysis. The .NET tests
**verify** the knowledge base rather than writing it – `KnowledgeRegenerationTests` fetches a release
fresh and asserts two independent regenerations are byte-identical to *each other*, proving the
generators are deterministic without a committed baseline to diff against (nothing per-release is
committed any more – see #106). Regenerating for real is `hypha generate`'s job (see
[tools/hypha-cli](tools/hypha-cli/README.md)), so a test run leaves the working tree clean.

## Code quality

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=mycelium-cmbse_mycelium-hypha&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=mycelium-cmbse_mycelium-hypha)

