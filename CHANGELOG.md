# Changelog

All notable changes to mycelium-hypha are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **`hypha use`, `hypha remove` and `hypha check`** (`refs #106`) replace `hypha sync`: switching the
  default release, removing one no longer wanted, and comparing installed releases against what is
  offerable upstream are now separate, always-safe-to-run verbs instead of one verb that silently
  fetched and generated the newest release automatically.
  - The `version-management` skill gives Claude the vocabulary to list local vs. online releases,
    fetch a chosen tag with visible progress, switch the default, or remove one - always confirming
    with the user first, since fetching costs real time and removing is destructive.
  - `hypha fetch` downloads the OMG specification PDFs **by default** now (`--no-specs` to opt out) -
    a fully local install needs everything a plain HTTP call can get it.

### Changed
- **The plugin's `SessionStart` hook only ever compares now - it never fetches or generates anything
  itself** (`refs #106`). `Hypha.Tools.Hook` runs `hypha check --json` synchronously (cheap enough - a
  couple of GitHub API calls - that no background process is needed) and reports what it finds:
  nothing installed yet, a newer release available, or nothing to report - leaving the decision to
  fetch entirely to the user. The same hook also folds in what `hooks/check-spec-pdfs.py` used to do
  separately (naming missing OMG PDFs for the default release), so the plugin needs one
  `SessionStart` hook instead of two, and no longer depends on Python being present just to run it.

### Removed
- **`hypha sync` and the background/lock/status-file machinery it needed** (`refs #106`). Detached
  launching, a file lock, and a polled status file existed only because a hook-triggered fetch had to
  return immediately and report progress later; once fetching is something the user asks for and
  Claude runs in the foreground, none of that indirection is needed. `hooks/check-spec-pdfs.py` is
  retired, folded into `Hypha.Tools.Hook`.

### Added
- **`hypha`, a distributable command-line tool, is now what generates the knowledge base**
  (`fixes #81`). It ships as a `dotnet tool` for anyone with the .NET 10 SDK and as a
  **self-contained** executable per platform (`win-x64`, `linux-x64`, `osx-x64`, `osx-arm64`) for
  anyone with no toolchain at all – which is the answer #77 needed to the question of what happens
  when the toolchains are absent. NativeAOT is deliberately out of scope: Handlebars.Net compiles
  templates at runtime through expression trees, and uml4net is reflection-heavy over XMI.
  - Four verbs: `discover` (what the upstreams offer), `fetch` (download a release into `sources/`
    and record it), `generate` (the knowledge base) and `list` (what this checkout carries).
  - `generate` takes an optional artifact name, and the names are not a list it holds: they are the
    `Artifact` of whatever `IKnowledgeGenerator` implementations are registered, so adding a
    generator adds its verb and `generate nonsense` reports the ones that exist.
  - `ReleaseInstaller` gives fetching the orchestration generation already had: it downloads a
    release's inputs, resolves both upstream commits and merges the result into `versions.json`. The
    manifest is written only once the downloads finish, so an interrupted fetch leaves a release
    unrecorded rather than recorded and half-present.
  - A **Release** workflow packs the tool, publishes the four self-contained builds, generates
    `THIRD-PARTY-NOTICES.txt` for the bundled closure and drafts a `tools-vX.Y.Z` release. The tool
    and the plugin version independently.

### Changed
- **The tests no longer write the knowledge base; they verify it** (`fixes #81`). Running
  `dotnet test` used to rewrite `knowledge/`, which made every test run a write to the repository and
  made "is the working tree clean afterwards?" a check you had to remember to perform. Generation is
  `hypha generate`'s job now, and a test run leaves the working tree clean.
  - `KnowledgeRegenerationTests` regenerates the whole knowledge base into a scratch folder and
    compares it **byte for byte** against the committed files – the same determinism proof, without
    touching a tracked file. All 1,090 files match.
  - The seam is `HyphaKnowledgeOptions.OutputRoot` (the CLI's `--output`): inputs are still read from
    the repository, only the output moves. `knowledge/<tag>/spec/` stays on the input side, because
    the Python PDF chain writes it and the cross-references read it.
  - `Hypha.Knowledge.Tests` went from 30s to 3s, having stopped doing the generation work twice.
- **Generation orchestration moved out of the test fixtures and into the library** (`refs #92`). Each
  artifact is now an `IKnowledgeGenerator` – `Artifact`, `Order`, `GenerateAsync(tag)` – registered as
  a collection, so regenerating a release is a loop over `Order` rather than a list of calls each
  caller had to keep in step. The knowledge base comes out byte-identical.
  - `KnowledgeLayout` replaced three separate copies of the repository layout that had grown up in the
    test projects, one per project plus a private one inside a fixture.
  - Two fixtures now write the committed knowledge base instead of seven; the rest assert on scratch
    output, so changing one of them can no longer rewrite what ships.
  - Three invariants moved from assertions in tests into the generators, where they hold for every
    caller: a clause title must never reach the cross-references, two models must never slugify to the
    same example page, and the textual notation refuses to run before the metamodel index exists.
  - `HyphaKnowledgeOptions` replaces the optional parameters that were accumulating on
    `AddHyphaKnowledge` – token, API host, raw-content host, repository root, download concurrency –
    and is validated at registration rather than at first use. It is where the CLI's flags will land.
  - Every generator takes an `ILogger` and reports the same three messages, so a host can show
    progress without knowing which generator it is watching. That is the seam #77 needs.
  - `ReleaseDiscovery`, `CommitResolver` and `ReleaseFetcher` are resolved by interface like
    everything else.

### Fixed
- **The cross-references no longer degrade silently on a new release tag** (`fixes #96`). The PDF-free
  rebuild added in #88 carries title-matched clause edges forward from the committed document, which
  is lossless – except for a tag that has neither a clause catalog nor a previous document, where it
  wrote a file with roughly a third of its clause edges missing and nothing failing. `Build` now
  refuses that combination and names both ways out; `AllowGrammarOnly` states that a grammar-only
  document is intended.

### Changed
- **The cross-references moved to .NET, and no longer need the OMG PDFs** (`fixes #88`).
  `Hypha.Knowledge.CrossReferences` writes `knowledge/<tag>/cross-references.json` byte-identically to
  the Python it replaces, and this **completes the port** (`fixes #82`): .NET now produces every
  committed artifact.
  - **`cross-references.json` can be rebuilt from committed sources alone.** 351 of its 546 clause
    edges are stated outright by the grammar's own `// Clause` comments in the committed `.kebnf`;
    the remaining title-matched ones are carried forward from the committed document, which already
    holds them. A contributor without the specifications regenerates the file and gets the same
    bytes back – previously they could not regenerate it at all.
  - The licensing guarantee is unchanged and still tested: edges record clause **identifiers**,
    never clause text, which is what lets this file be committed while `knowledge/<tag>/spec/`
    cannot be.
  - `tools/spec-extract` is now the PDF chain and nothing else, and its README says so.
- **The textual-notation knowledge base moved to .NET** (`fixes #87`).
  `Hypha.Knowledge.TextualNotation` now writes `knowledge/<tag>/textual-notation/` – 309 example
  pages per release plus the index. All 618 committed example pages come out **byte-identical**; the
  only change is the line naming the generator in `index.md`.
  - Surface forms are still derived from the metamodel's own naming convention rather than curated
    (`PartDefinition` → `part def`, `PartUsage` → `part`), and still bounded by that release's
    metamodel index, so the mapping cannot invent an element.
  - The declaration match still refuses to fire inside an identifier or a quoted name –
    `counterpart` is not a `part`. Unlike the grammar patterns this one is deliberately left
    Unicode-aware, matching the Python it replaces: model text is user-written.
  - Example pages are ordered segment by segment and case-insensitively, which is what the committed
    index was generated with; an ordinal sort of the whole path would interleave folders with files.
- **BNF parsing and the grammar references moved to .NET** (`fixes #86`). `Hypha.Knowledge.Grammar`
  now parses both the textual `.kebnf` and the graphical `.kgbnf` and writes
  `knowledge/<tag>/textual-notation/grammar-{kerml,sysml,graphical}.md`. The generated files are
  **byte-identical** to what the Python emitted, apart from the line naming the generator.
  - The porting hazards are guarded by tests rather than left to be rediscovered: `\d` and `\w` are
    written out as explicit ASCII classes, because Python compiled these patterns with `re.ASCII`
    while .NET's shorthands are Unicode-aware; and the possessive quantifier on the clause number
    becomes an atomic group, which .NET does have.
  - The exclusivity guard is unchanged and still the point: a helper production shared by two
    elements contributes to neither. 96 of the 190 referenced helpers are shared, so relaxing it
    would report more feature assignments and mean less.
- **Release handling moved from Python to .NET** (`fixes #83`, `fixes #84`, `fixes #85`), into the new
  `tools/knowledge-gen/Hypha.Knowledge` library. `spec_extract.versions` and `spec_extract.fetch` are
  gone; `spec-extract` now does what it is best at – PDF extraction – and nothing else.
  - `ReleaseTag`, `ReleaseCatalog`, `GitHubToken` and `ReleaseDiscovery` offer the same 56 releases in
    the same order as the Python they replace; `VersionManifest` rewrites the committed
    `knowledge/versions.json` byte for byte.
  - `ReleaseInputs` decides *what* a release consists of as a pure function of a repository listing,
    so the selection rules are testable without the network; `ReleaseFetcher` downloads it.
  - Fetching is concurrent (6 at a time) over a pooled `HttpClient`. The Python fetcher opened a
    connection per file and was reset by the host after roughly 190 of a release's 314 files; a
    release now fetches whole in about 15 seconds.
- Well-established NuGet packages are now welcome rather than avoided, and services are composed
  through `Microsoft.Extensions.DependencyInjection`. `AddHyphaKnowledge()` registers one resilient
  named client – `Microsoft.Extensions.Http.Resilience`, so retries are **jittered**, which matters
  once downloads run concurrently – and the services that share it. This is groundwork for the CLI
  (#81).

### Added
- **Version awareness (`fixes #67`, `fixes #74`).** The knowledge base is now generated per upstream
  release tag (`YYYY-MM`) instead of from a single snapshot, with a committed rolling window of the
  two most recent releases – currently `2026-05` (the default) and `2026-04`.
  - `knowledge/versions.json` records the installed tags, the default, and the upstream commit each
    tag resolved to (traceability only – **the tag is the identifier**).
  - Release discovery offers a version only when the tag resolves in **both** upstreams: they are not
    in lockstep (`2023-07.1` is Release-only; `2024-08`, `2023-01`, `2026-05-pre` are Pilot-only).
    Tags are `YYYY-MM` with an optional point release; pre-releases, internal drops and letter
    revisions are excluded. 56 releases are currently offerable.
  - Fetching pulls a release's inputs from both upstreams, retrying transient connection resets with
    backoff and resuming a partial run.
  - Skills and subagents resolve the release from the manifest, answer from the default unless a tag
    is named, and **state which release an answer came from**.
  - `hooks/check-spec-pdfs.py` checks the default release and gives tagged download URLs.

### Fixed
- The previous inputs were not a coherent version: the PDFs resolved to Release tag `2026-03`, the
  textual sources to `2026-04`, and the metamodel XMI to an *untagged* `master` commit. Each release
  now comes from a single upstream point.
- `XmiModelReaderTests` was silently skipping on a stale `sources/xmi` path; it runs again.
- `.gitignore` gained a global `*.pdf` backstop. Moving the specs rule to the per-tag form stopped it
  matching a stale copy of the old layout, and three copyrighted OMG PDFs were very nearly committed.

### Changed
- `sources/` and `knowledge/` are laid out per tag: `sources/<tag>/{xmi,textual,specs}` and
  `knowledge/<tag>/{metamodel,spec,cross-references.json}`. `knowledge/textual-notation/` stays
  shared (hand-curated), and `sources/PrimitiveTypes.xmi` is shared – it is the OMG UML primitives
  library, published by neither upstream and identical for every release.
- The model URI inside the XMI (`…/SysML/20250201`) is no longer used anywhere as a version
  identifier. It tracks neither release nor content: the 2026-05 metamodel still declares a 2025 URI.
  The same model may appear under several tags, which is accepted rather than deduplicated.

### Added
- `knowledge/cross-references.json` (+ schema): edges from every metamodel element to the
  specification clauses that treat it, its BNF grammar production and any worked example. It records
  clause **identifiers only, never clause text**, so it ships even though `knowledge/spec/` cannot –
  185 of 187 elements resolve to at least one clause. Generated by the new `spec_extract.crossrefs`
  layer (`fixes #68`).
- Provenance tiers (`NORMATIVE` / `MODEL` / `DERIVED`) documented in the skills and the README, and
  carried in the data itself via the `provenanceTiers` block, so every cross-reference edge says how
  it was obtained.
- `knowledge/metamodel/diagrams/<Package>.md`: a Mermaid class diagram per package (41 files),
  generated from the XMI by the new `PackageDiagramGenerator` and linked from each package section of
  `knowledge/metamodel/index.md`. Each diagram carries the package's metaclasses (abstract ones
  marked), their generalizations, one-hop boundary nodes for supertypes owned elsewhere, and the
  structural owned features (`fixes #69`).
- Two rules keep those diagrams legible: derived features are omitted (computed views rather than
  structure, and they outnumber the structural features roughly three to one), and only
  metaclass-typed features become edges – primitive- and enumeration-typed ones are drawn inside the
  class box, so `String` and `Boolean` never become hub nodes.

### Fixed
- Packages that share a name across the two metamodels (`KerML::Kernel::Metadata` and
  `SysML::Systems::Metadata`) are merged into one diagram. Treating them separately wrote two files
  to the same path, silently discarding one of them.

### Changed
- `metamodel-lookup` and the `metamodel-navigator` subagent now query
  `knowledge/metamodel/metamodel.json` for set, closure and fan-out questions instead of grepping the
  per-element markdown, which stays the citable surface for single-element answers. Matching a JSON
  field is also exact, where grepping markdown matches documentation prose too.
- `spec-citation` degrades gracefully: without the OMG PDFs it now names the governing clause from
  the cross-references instead of refusing to answer, and says the reference is `DERIVED`.
- `sysml-validation` grounds its reachability and multiplicity-bound checks in `metamodel.json`
  (`inheritedAttributes` / `inheritedFrom`, and the typed `lower`/`upper` integers) rather than
  reasoning from prose; the `bad-multiplicity` and `redefines-nonexistent` fixtures record how each
  is decided.
- `jq` is now described as **recommended** rather than merely an accelerator, and the recommendation
  moved into the plugin-facing README – the previous note lived in `tools/metamodel-gen/README.md`,
  which is not shipped with the plugin.

## [1.1.0] - 2026-07-01

### Changed
- Bumped `uml4net` (`uml4net.xmi`, `uml4net.Extensions`, `uml4net.Reporting`) `8.1.2` → `8.2.1`,
  which reads the previously-missing subsetting information from the XMI. Regenerated the metamodel
  knowledge base accordingly (`knowledge/metamodel/`): several element files and `metamodel.json`
  now carry the complete `Subsets` sets (`fixes #62`).
- Interesting-metaclass selection (used for the generator golden fixtures) now folds in operation
  argument/return-type variations, broadening test coverage (`fixes #63`).

## [1.0.0] - 2026-06-29

### Added
- Initial Claude plugin scaffolding (`.claude-plugin/plugin.json`, marketplace manifest).
- Skills: `metamodel-lookup`, `spec-citation`, `sysml-validation`.
- Subagents: `metamodel-navigator`, `spec-citation`, `sysml-validator`.
- Knowledge-base layout (`knowledge/`) for KerML/SysML v2 metamodel elements,
  normative spec excerpts, and textual-notation examples.
- Source-document layout (`sources/`) for OMG XMI and PDF specifications.
- Generation pipelines layout (`tools/`): C#/uml4net metamodel generator and
  Python PDF spec extractor.

### Changed
- Renamed the metamodel knowledge folder `knowledge/sysml2/` → `knowledge/metamodel/`
  (consistent content-type naming alongside `spec/` and `textual-notation/`, and no longer
  overloaded with the `knowledge/spec/sysml2/` clause folder).
