# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Proof that the pinned PDF library extracts readable text from a real OMG specification.

The OMG PDFs are git-ignored, so these tests skip (rather than fail) when the PDF is absent — the
same convention the C# metamodel-gen tests use for the XMI inputs, keeping CI green without the
copyrighted sources.
"""

from __future__ import annotations

from pathlib import Path

from spec_extract import pdf_reader


def test_page_count_is_positive(kerml_pdf: Path) -> None:
    assert pdf_reader.page_count(kerml_pdf) > 0


def test_extracts_readable_text(kerml_pdf: Path) -> None:
    pages = pdf_reader.extract_pages(kerml_pdf)

    assert len(pages) == pdf_reader.page_count(kerml_pdf)

    text = "\n".join(pages)
    # A substantial amount of real text, not a few stray glyphs.
    assert len(text) > 10_000
    # A stable phrase from the specification proves the text is readable, not mojibake.
    assert "Kernel Modeling Language" in text
