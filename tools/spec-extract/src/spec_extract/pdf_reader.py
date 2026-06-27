# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Thin, testable wrapper over ``pdfplumber`` for reading the OMG specification PDFs.

This is the lowest layer of the extraction pipeline: it only turns a PDF into text. Clause/section
segmentation and markdown generation are built on top of this in later work.
"""

from __future__ import annotations

from pathlib import Path

import pdfplumber

from spec_extract.model import Word


def page_count(pdf_path: str | Path) -> int:
    """Return the number of pages in the PDF at ``pdf_path``."""
    with pdfplumber.open(pdf_path) as pdf:
        return len(pdf.pages)


def extract_pages(pdf_path: str | Path) -> list[str]:
    """Return the extracted text of each page, in order.

    Pages that contain no extractable text (e.g. image-only pages) yield an empty string rather than
    being dropped, so page indices stay aligned with the source document.
    """
    with pdfplumber.open(pdf_path) as pdf:
        return [page.extract_text() or "" for page in pdf.pages]


def extract_text(pdf_path: str | Path) -> str:
    """Return the whole document's extracted text, pages joined by newlines."""
    return "\n".join(extract_pages(pdf_path))


def extract_words(pdf_path: str | Path) -> list[list[Word]]:
    """Return the positioned words of each page (1-based ``page`` index).

    Keeping word positions lets the layout layer reconstruct reading order across columns and drop
    running headers/footers — things a flat ``extract_text`` cannot do.
    """
    pages: list[list[Word]] = []
    with pdfplumber.open(pdf_path) as pdf:
        for index, page in enumerate(pdf.pages, start=1):
            words = [
                Word(
                    text=word["text"],
                    x0=float(word["x0"]),
                    x1=float(word["x1"]),
                    top=float(word["top"]),
                    page=index,
                )
                for word in page.extract_words(use_text_flow=False)
            ]
            pages.append(words)
    return pages
