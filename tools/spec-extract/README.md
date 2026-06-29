# spec-extract (Python)

Extracts the **full verbatim text** of the OMG PDF specifications into per-clause markdown, so the
Hypha plugin can quote the normative wording exactly. The output is **never committed or
redistributed** (see below).

- **Input:** `sources/specs/*.pdf` (KerML 1.0, SysML v2) – OMG-copyrighted, git-ignored.
- **Output:** `knowledge/spec/kerml/` and `knowledge/spec/sysml2/` – one file per clause/section
  carrying its full text plus clause number, title and source page metadata.

Like `tools/metamodel-gen`, this tool is **driven by tests, not a distributed CLI**.

## Licensing & OMG terms

The OMG specifications are copyrighted – see the [NOTICE](../../NOTICE) for the full attribution and
the OMG specification license. Two constraints shape this tool:

- The **PDFs are never redistributed**. They stay git-ignored in `sources/specs/` (the OMG license
  forbids posting the specifications on a network); provenance is recorded in
  [`sources/README.md`](../../sources/README.md#provenance) instead.
- The **generated `knowledge/spec/` is the full verbatim specification text**, so it is treated exactly
  like the PDFs: **git-ignored and regenerated locally** from the user's own copies, never committed or
  posted. Because it is not redistributed, holding the verbatim text locally for informational use
  stays within the OMG terms.

## Setup

Requires **Python ≥ 3.12**. From this directory (`tools/spec-extract/`):

```sh
python -m venv .venv
# Windows:        .venv\Scripts\Activate.ps1
# macOS / Linux:  source .venv/bin/activate
pip install -e .[dev]
```

## Lint & test

```sh
ruff check .
pytest
```

The spec PDFs are git-ignored (see **Licensing & OMG terms** above). When they are present in
`sources/specs/`, the extraction-proof test runs against the real KerML PDF; when they are absent
(e.g. in CI), that test **skips** rather than fails – the same convention `metamodel-gen` uses for
its XMI inputs.

## PDF library

**pdfplumber** (MIT) is the pinned PDF reader (`pyproject.toml`):

- **License** – MIT, a clean fit for this Apache-2.0 repository.
- **Capability** – exposes per-word/character positions and layout, which the later clause/section
  segmentation step needs to detect headings and structure (not just a flat text dump).

PyMuPDF was considered (faster, excellent raw extraction) but rejected: it is AGPL-3.0 (or paid
commercial), an awkward licensing fit even for a non-distributed dev tool.

## Pipeline

The extractor is a small chain of pure, independently-testable layers (`spec_extract.*`):

| Module | Responsibility |
| --- | --- |
| `pdf_reader` | `pdfplumber` access: `page_count`, `extract_pages`, `extract_text`, `extract_words` (positioned words). |
| `layout` | Assemble positioned words into reading-ordered lines: multi-column ordering and running header/footer/page-number stripping. |
| `clauses` | Detect clause/section headings (successor-numbering rule; skips TOC and stray in-text numbers) and group body text, with page ranges. |
| `normative` | Split a clause into normative prose vs informative `NOTE`/`EXAMPLE` blocks; flag clauses containing `shall`/`must`. Lossless. |
| `markdown` | Render a clause to deterministic markdown (front matter + body), e.g. `07.04.02-concrete-syntax.md`. |
| `pipeline` | `extract_document(pdf, DocMeta)` → clauses; `write_clauses(clauses, out_dir)` → files. |

### Output

One markdown file per clause under `knowledge/spec/kerml/` and `knowledge/spec/sysml2/`, with YAML
front matter (`clause`, `title`, `document`, `version`, `pages`, `normative`) and a verbatim body;
informative `NOTE`/`EXAMPLE` runs are wrapped in `<!-- informative:note -->` markers (original text
untouched, but greppable). Each document also gets an `index.md` table of contents (clause → title →
pages → file, in document order) and a machine-readable `index.json` catalog (same metadata, keyed by
clause number for O(1) lookup – metadata only, no clause text). This tree is **git-ignored** (see
*Licensing & OMG terms*).

### Regenerating

Running `pytest` with the PDFs present in `sources/specs/` runs `test_generate.py`, which writes the
(git-ignored) `knowledge/spec/` tree from the real specs – the same "tests write the knowledge base"
convention `metamodel-gen` uses. Without the PDFs that test (and the real-PDF smoke test) **skip**;
the assertion-based unit tests always run.

## Status

- [x] Project scaffold + PDF tooling (issue #6).
- [x] Per-clause extraction pipeline + tests (issue #7).

Input provenance (upstream source + version/commit) is recorded in
[`sources/README.md`](../../sources/README.md#provenance).
