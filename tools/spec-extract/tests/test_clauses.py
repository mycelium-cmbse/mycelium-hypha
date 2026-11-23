# SPDX-FileCopyrightText: 2026 Starion Group S.A.
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


def test_annexes_follow_the_numbered_clauses() -> None:
    lines = _lines(
        (1, "1 Scope"),
        (2, "2 Conformance"),
        (2, "2.1 Levels"),
        (9, "A Annex: Conformance Test Suite"),
        (9, "A.1 ProjectService Conformance Test Cases"),
        (10, "A.2 QueryService Conformance Test Cases"),
        (10, "A.2.1 Overview"),
        (11, "A.10 Cross-Cutting Conformance Test Cases"),
        (12, "A.3 Cross-Cutting Conformance Test Cases"),
        (13, "B Annex: Examples and Cookbook"),
        (13, "B.1 Examples"),
    )

    clauses = detect_clauses(lines, document="SystemsModelingAPI", version="1.0")

    # A.10 does not follow A.2.1, so it stays in the body of A.2.1.
    assert [c.number for c in clauses] == ["1", "2", "2.1", "A", "A.1", "A.2", "A.2.1", "A.3", "B", "B.1"]
    assert clauses[3].title == "Annex: Conformance Test Suite"
    assert clauses[3].page_start == 9


def test_a_sentence_starting_with_a_capital_letter_is_not_an_annex() -> None:
    lines = _lines(
        (1, "1 Scope"),
        (1, "A tool shall conform to this specification."),
        (2, "B Annex: Examples"),
        (3, "2 Conformance"),
    )

    clauses = detect_clauses(lines, document="KerML", version="1.0")

    assert [c.number for c in clauses] == ["1", "2"]
    assert "A tool shall conform" in clauses[0].text


def test_no_numbered_clause_follows_an_annex() -> None:
    lines = _lines(
        (1, "1 Scope"),
        (5, "A Annex: Model Execution"),
        (5, "A.1 Overview"),
        (5, "2 instances are created for each part."),
        (6, "A.2 Instantiation Procedure"),
    )

    clauses = detect_clauses(lines, document="KerML", version="1.0")

    assert [c.number for c in clauses] == ["1", "A", "A.1", "A.2"]
    assert "2 instances are created" in clauses[2].text


def test_annex_contents_entries_are_ignored() -> None:
    lines = _lines(
        (2, "A Annex: Model Execution .......... 443"),
        (2, "A.1 Overview .......... 443"),
        (5, "1 Scope"),
        (9, "A Annex: Model Execution"),
    )

    clauses = detect_clauses(lines, document="KerML", version="1.0")

    assert [(c.number, c.page_start) for c in clauses] == [("1", 5), ("A", 9)]


def test_a_wrapped_annex_heading_keeps_its_whole_title() -> None:
    lines = _lines(
        (1, "1 Scope"),
        (60, "A Annex: Conformance Test Suite"),
        (60, "(Normative)"),
        (95, "B Annex: API and Services Examples and"),
        (95, "Cookbook"),
        (95, "(Informative)"),
        (95, "B.1 Examples"),
    )

    clauses = detect_clauses(lines, document="SystemsModelingAPI", version="1.0")

    assert [(c.number, c.title) for c in clauses] == [
        ("1", "Scope"),
        ("A", "Annex: Conformance Test Suite"),
        ("B", "Annex: API and Services Examples and Cookbook"),
        ("B.1", "Examples"),
    ]
    assert clauses[2].text == "(Informative)"


def test_an_annex_status_applies_to_its_subclauses() -> None:
    lines = _lines(
        (1, "1 Scope"),
        (5, "A Annex: Model Execution"),
        (5, "(Informative)"),
        (5, "A.1 Overview"),
        (6, "A.1.1 Details"),
        (7, "B Annex: Conformance Test Suite"),
        (7, "(Normative)"),
        (7, "B.1 Cases"),
    )

    clauses = detect_clauses(lines, document="KerML", version="1.0")

    assert [(c.number, c.declared_normative) for c in clauses] == [
        ("1", None),
        ("A", False),
        ("A.1", False),
        ("A.1.1", False),
        ("B", True),
        ("B.1", True),
    ]


def test_an_annex_without_a_status_keeps_a_one_line_title() -> None:
    lines = _lines(
        (1, "1 Scope"),
        (5, "A Annex: Model Execution"),
        (5, "Its text starts here."),
        (5, "A.1 Overview"),
    )

    clauses = detect_clauses(lines, document="KerML", version="1.0")

    assert [(c.number, c.title, c.declared_normative) for c in clauses] == [
        ("1", "Scope", None),
        ("A", "Annex: Model Execution", None),
        ("A.1", "Overview", None),
    ]
    assert clauses[1].text == "Its text starts here."


def test_an_annex_status_on_the_next_page_does_not_wrap_the_title() -> None:
    lines = _lines(
        (1, "1 Scope"),
        (5, "A Annex: Model Execution"),
        (6, "(Informative)"),
    )

    clauses = detect_clauses(lines, document="KerML", version="1.0")

    assert (clauses[1].title, clauses[1].declared_normative) == ("Annex: Model Execution", None)
