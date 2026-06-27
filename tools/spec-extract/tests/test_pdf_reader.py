# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Tests for the pdfplumber wrapper.

Two layers: PDF-free unit tests that fake ``pdfplumber`` (so the mapping logic is covered in CI,
where the real PDFs are absent), plus a skip-if-absent proof against the real KerML PDF.
"""

from __future__ import annotations

from pathlib import Path

import pytest

from spec_extract import pdf_reader


class _FakePage:
    def __init__(
        self,
        *,
        chars: list[dict] | None = None,
        words: list[dict] | None = None,
        text: str | None = "",
    ) -> None:
        self.chars = chars or []
        self._words = words or []
        self._text = text

    def extract_words(self, **_kwargs: object) -> list[dict]:
        return self._words

    def extract_text(self) -> str | None:
        return self._text


class _FakePdf:
    def __init__(self, pages: list[_FakePage]) -> None:
        self.pages = pages

    def __enter__(self) -> _FakePdf:
        return self

    def __exit__(self, *_exc: object) -> bool:
        return False


class _FakePdfplumber:
    def __init__(self, pages: list[_FakePage]) -> None:
        self._pages = pages

    def open(self, _path: object) -> _FakePdf:
        return _FakePdf(self._pages)


# --- PDF-free unit tests (run everywhere, including CI) -------------------------------------------


def test_page_count_counts_pages(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(pdf_reader, "pdfplumber", _FakePdfplumber([_FakePage(), _FakePage()]))
    assert pdf_reader.page_count("x.pdf") == 2


def test_extract_pages_uses_text_or_empty_string(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(pdf_reader, "pdfplumber", _FakePdfplumber([_FakePage(text="hello"), _FakePage(text=None)]))
    assert pdf_reader.extract_pages("x.pdf") == ["hello", ""]


def test_extract_text_joins_pages_with_newline(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(pdf_reader, "pdfplumber", _FakePdfplumber([_FakePage(text="a"), _FakePage(text="b")]))
    assert pdf_reader.extract_text("x.pdf") == "a\nb"


def test_extract_words_maps_fields_and_page_index(monkeypatch: pytest.MonkeyPatch) -> None:
    words = [{"text": "Foo", "x0": 1.0, "x1": 5.0, "top": 2.0}]
    monkeypatch.setattr(pdf_reader, "pdfplumber", _FakePdfplumber([_FakePage(), _FakePage(words=words)]))

    result = pdf_reader.extract_words("x.pdf")

    assert result[0] == []
    word = result[1][0]
    assert (word.text, word.x0, word.x1, word.top, word.page) == ("Foo", 1.0, 5.0, 2.0, 2)


def test_extract_chars_maps_fields_and_page_index(monkeypatch: pytest.MonkeyPatch) -> None:
    chars = [{"text": "A", "x0": 1.0, "x1": 2.0, "top": 3.0, "size": 10.0, "fontname": "TimesNewRomanPSMT"}]
    monkeypatch.setattr(pdf_reader, "pdfplumber", _FakePdfplumber([_FakePage(chars=chars)]))

    char = pdf_reader.extract_chars("x.pdf")[0][0]

    assert (char.text, char.x0, char.x1, char.top, char.size, char.fontname, char.page) == (
        "A",
        1.0,
        2.0,
        3.0,
        10.0,
        "TimesNewRomanPSMT",
        1,
    )


# --- Proof against the real PDF (skips when the git-ignored PDF is absent) ------------------------


def test_page_count_is_positive(kerml_pdf: Path) -> None:
    assert pdf_reader.page_count(kerml_pdf) > 0


def test_extracts_readable_text(kerml_pdf: Path) -> None:
    pages = pdf_reader.extract_pages(kerml_pdf)

    assert len(pages) == pdf_reader.page_count(kerml_pdf)

    text = "\n".join(pages)
    assert len(text) > 10_000
    assert "Kernel Modeling Language" in text
