# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Clause boundary detection: successor numbering, TOC rejection, stray-number rejection."""

from __future__ import annotations

from spec_extract.clauses import detect_clauses
from spec_extract.model import Line


def _lines(*entries: tuple[int, str]) -> list[Line]:
    return [Line(page=page, text=text) for page, text in entries]


def test_detects_clauses_and_groups_body() -> None:
    lines = _lines(
        (1, "1 Scope"),
        (1, "This document defines the scope."),
        (2, "1.1 Purpose"),
        (2, "The purpose is stated here."),
        (3, "2 Conformance"),
        (3, "A tool shall conform when it satisfies 2.5 of the rules."),
    )

    clauses = detect_clauses(lines, document="KerML", version="1.0")

    assert [(c.number, c.title) for c in clauses] == [
        ("1", "Scope"),
        ("1.1", "Purpose"),
        ("2", "Conformance"),
    ]
    assert clauses[0].page_start == 1
    assert clauses[2].page_start == 3
    # "2.5" inside the body is not mistaken for a heading.
    assert "2.5 of the rules" in clauses[2].text


def test_table_of_contents_entries_are_ignored() -> None:
    lines = _lines(
        (2, "1 Scope .......... 5"),
        (2, "2 Conformance .......... 9"),
        (5, "1 Scope"),
        (5, "Real scope body."),
    )

    clauses = detect_clauses(lines, document="KerML", version="1.0")

    assert [c.number for c in clauses] == ["1"]
    assert clauses[0].page_start == 5
    assert "Real scope body." in clauses[0].text
