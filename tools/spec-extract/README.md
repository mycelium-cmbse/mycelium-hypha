# spec-extract (Python)

Extracts **clause-referenced citations** from the OMG PDF specifications into segmented markdown, so
the Hypha plugin can quote and point at the normative text without redistributing the specifications
themselves.

- **Input:** `sources/specs/*.pdf` (KerML 1.0, SysML v2 Parts 1–2) — OMG-copyrighted, git-ignored.
- **Output:** `knowledge/spec/kerml/` and `knowledge/spec/sysml2/` — excerpts segmented by
  clause/section, each carrying clause number, title, and source page metadata for citation.

Like `tools/metamodel-gen`, this tool is **driven by tests, not a distributed CLI**.

## Licensing & OMG terms

The OMG specifications are copyrighted — see the [NOTICE](../../NOTICE) for the full attribution and
the OMG specification license. Two constraints shape this tool:

- The **PDFs are never redistributed**. They stay git-ignored in `sources/specs/` (the OMG license
  forbids posting the specifications on a network); provenance is recorded in
  [`sources/README.md`](../../sources/README.md#provenance) instead.
- The **output is a derivative "special purpose specification … based upon"** the specs, for
  **informational citation only** — clause-referenced excerpts with provenance, *not* a wholesale
  verbatim re-publication of the specification text. Extraction is scoped to what citation needs so
  `knowledge/spec/` stays within the OMG terms.

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
(e.g. in CI), that test **skips** rather than fails — the same convention `metamodel-gen` uses for
its XMI inputs.

## PDF library

**pdfplumber** (MIT) is the pinned PDF reader (`pyproject.toml`):

- **License** — MIT, a clean fit for this Apache-2.0 repository.
- **Capability** — exposes per-word/character positions and layout, which the later clause/section
  segmentation step needs to detect headings and structure (not just a flat text dump).

PyMuPDF was considered (faster, excellent raw extraction) but rejected: it is AGPL-3.0 (or paid
commercial), an awkward licensing fit even for a non-distributed dev tool.

The current code surface is intentionally minimal — `spec_extract.pdf_reader` (`page_count`,
`extract_pages`, `extract_text`) — proving the library reads the specs cleanly. Clause segmentation
and markdown emission are built on top of it next.

## Status

Project scaffold + PDF tooling in place (issue #6). Still to come, as a Python tool that:

1. Reads each PDF (text + structure) and detects clause/section boundaries.
2. Emits clause-referenced excerpts (one markdown file per clause) for citation, tagging normative
   vs. informative content — within the OMG terms above.
3. Records source document, version, clause number/title, and page range as front matter.

Input provenance (upstream source + version/commit) is recorded in
[`sources/README.md`](../../sources/README.md#provenance).
