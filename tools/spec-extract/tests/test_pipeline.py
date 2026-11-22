# SPDX-FileCopyrightText: 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Pipeline composition tests with synthetic characters (no PDF needed)."""

from __future__ import annotations

from pathlib import Path

import pytest

from spec_extract import pipeline
from spec_extract.model import Char, Line
from spec_extract.pipeline import DocMeta, cover_version, extract_document, write_clauses, write_index


def _row(
    tokens: list[str],
    top: float,
    *,
    x0: float = 50.0,
    size: float = 10.0,
    char_w: float = 6.0,
    gap: float = 4.0,
) -> list[Char]:
    chars: list[Char] = []
    x = x0
    for index, token in enumerate(tokens):
        if index > 0:
            x += gap
        for glyph in token:
            chars.append(
                Char(text=glyph, x0=x, x1=x + char_w, top=top, size=size, fontname="TimesNewRomanPSMT", page=1)
            )
            x += char_w
    return chars


def test_extract_document_composes_layers(monkeypatch: pytest.MonkeyPatch) -> None:
    chars = _row(["1", "Scope"], top=100) + _row(["Body", "text."], top=120)
    monkeypatch.setattr(pipeline.pdf_reader, "extract_chars", lambda _path: [chars])

    clauses = extract_document("x.pdf", DocMeta("KerML", "1.0", "kerml"))

    assert len(clauses) == 1
    assert (clauses[0].number, clauses[0].title) == ("1", "Scope")
    assert "Body text." in clauses[0].text


def test_write_clauses_and_index(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    chars = _row(["1", "Scope"], top=100) + _row(["Body", "text."], top=120)
    monkeypatch.setattr(pipeline.pdf_reader, "extract_chars", lambda _path: [chars])
    clauses = extract_document("x.pdf", DocMeta("KerML", "1.0", "kerml"))

    paths = write_clauses(clauses, tmp_path)
    index = write_index(clauses, tmp_path)

    assert [path.name for path in paths] == ["01-scope.md"]
    assert paths[0].read_text(encoding="utf-8").startswith("---")
    assert index.name == "index.md"
    assert index.read_text(encoding="utf-8").startswith("---")


def test_the_version_is_read_from_the_cover(monkeypatch: pytest.MonkeyPatch) -> None:
    chars = _row(["Version", "1.1", "Beta", "1"], top=60) + _row(["1", "Scope"], top=100)
    monkeypatch.setattr(pipeline.pdf_reader, "extract_chars", lambda _path: [chars])

    clauses = extract_document("x.pdf", DocMeta("KerML", "1.0", "kerml"))

    assert [clause.version for clause in clauses] == ["1.1 Beta 1"]


def test_without_a_cover_version_the_given_one_is_kept(monkeypatch: pytest.MonkeyPatch) -> None:
    chars = _row(["1", "Scope"], top=100)
    monkeypatch.setattr(pipeline.pdf_reader, "extract_chars", lambda _path: [chars])

    clauses = extract_document("x.pdf", DocMeta("KerML", "1.0", "kerml"))

    assert [clause.version for clause in clauses] == ["1.0"]


def test_only_the_cover_page_names_the_version() -> None:
    lines = [Line(page=1, text="Kernel Modeling Language"), Line(page=2, text="Version 9.9")]

    assert cover_version(lines) is None
