# spec-extract (Python)

Extracts normative text from the OMG PDF specifications into clause-segmented markdown for
citation.

- **Input:** `sources/specs/*.pdf` (KerML 1.0, SysML v2 Parts 1–2)
- **Output:** `knowledge/spec/kerml/` and `knowledge/spec/sysml2/` — segmented by clause/section,
  each file carrying clause number, title, and source page metadata.

## Status

Not yet implemented. Planned as a Python tool that:

1. Reads each PDF (text + structure) and detects clause/section boundaries.
2. Emits one markdown file per clause, preserving normative wording verbatim and tagging
   normative vs. informative content.
3. Records source document, version, clause number/title, and page range as front matter.

> Choose the PDF library during implementation (e.g. `pymupdf`/`pdfplumber`) and pin it in
> `pyproject.toml`.
