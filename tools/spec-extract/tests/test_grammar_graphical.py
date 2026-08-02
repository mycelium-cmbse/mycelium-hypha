# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Unit tests for the graphical (.kgbnf) grammar parser, on text in the shape upstream really uses."""

from __future__ import annotations

from spec_extract.grammar import parse_graphical, render_graphical

GRAPHICAL = """
// Part 2 - Systems Modeling Language (SysML)

// Clause 8.2.3.2 Elements and Relationships Graphical Notation

element =
     dependencies-and-annotations-element
   | general-element

compartment =| general-compartment

general-compartment =
      <img src="images/general-compartment.svg" width="408.0">
      general-view

ellipsis-at-lower-left-corner = '...'

// Note. An element inside a textual compartment is selected by graying out a substring.

// Clause 8.2.3.3 Dependencies Graphical Notation

binary-dependency =
      <img src="images/binary-dependency.svg" width="200.0">
"""


def test_parses_kebab_case_production_names() -> None:
    names = [production.name for production in parse_graphical(GRAPHICAL)]

    assert names == [
        "element",
        "compartment",
        "general-compartment",
        "ellipsis-at-lower-left-corner",
        "binary-dependency",
    ]


def test_tolerates_the_leading_alternation_bar() -> None:
    # "compartment =| general-compartment" occurs upstream and must not be dropped.
    by_name = {production.name: production for production in parse_graphical(GRAPHICAL)}

    assert "compartment" in by_name
    assert "general-compartment" in by_name["compartment"].body


def test_captures_image_references() -> None:
    by_name = {production.name: production for production in parse_graphical(GRAPHICAL)}

    assert by_name["general-compartment"].images == ("images/general-compartment.svg",)
    assert by_name["element"].images == ()


def test_attributes_productions_to_their_clause() -> None:
    by_name = {production.name: production for production in parse_graphical(GRAPHICAL)}

    assert by_name["element"].clause == "8.2.3.2"
    assert by_name["binary-dependency"].clause == "8.2.3.3"


def test_note_comments_do_not_start_a_production_or_leak_into_bodies() -> None:
    by_name = {production.name: production for production in parse_graphical(GRAPHICAL)}

    assert "Note." not in by_name["ellipsis-at-lower-left-corner"].body
    assert not [name for name in by_name if name.startswith("note")]


def test_graphical_productions_declare_no_metaclass() -> None:
    # The graphical grammar has no ": Metaclass" form; nothing should claim otherwise.
    assert all(production.produces is None for production in parse_graphical(GRAPHICAL))


def test_render_links_images_at_the_release_tag() -> None:
    text = render_graphical("2026-05", "Systems-Modeling/SysML-v2-Release", parse_graphical(GRAPHICAL))

    assert (
        "![general-compartment](https://raw.githubusercontent.com/Systems-Modeling/"
        "SysML-v2-Release/2026-05/bnf/images/general-compartment.svg)" in text
    )


def test_render_groups_by_clause_and_counts_images() -> None:
    text = render_graphical("2026-05", "owner/repo", parse_graphical(GRAPHICAL))

    assert "## 8.2.3.2 Elements and Relationships Graphical Notation" in text
    assert "## 8.2.3.3 Dependencies Graphical Notation" in text
    assert "withImages: 2" in text
    assert "productions: 5" in text
