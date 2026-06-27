# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Pipeline composition tests with synthetic characters (no PDF needed)."""

from __future__ import annotations

from pathlib import Path

import pytest

from spec_extract import pipeline
from spec_extract.model import Char
from spec_extract.pipeline import DocMeta, extract_document, write_clauses, write_index


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
