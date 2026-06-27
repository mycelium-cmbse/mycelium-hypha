# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Markdown rendering: deterministic file naming, front matter, lossless tagged body."""

from __future__ import annotations

from spec_extract.markdown import clause_filename, render, render_index
from spec_extract.model import Block, Clause


def _clause() -> Clause:
    return Clause(
        number="7.4.2",
        title="Concrete Syntax",
        document="KerML",
        version="1.0",
        page_start=45,
        page_end=47,
        blocks=[
            Block("text", "The model shall use the concrete syntax."),
            Block("note", "NOTE This guidance is informative."),
        ],
        is_normative=True,
    )


def test_clause_filename_is_zero_padded_and_slugged() -> None:
    assert clause_filename(_clause()) == "07.04.02-concrete-syntax.md"


def test_render_includes_front_matter_heading_and_informative_marker() -> None:
    output = render(_clause())

    assert output.startswith("---\n")
    assert 'clause: "7.4.2"' in output
    assert 'pages: "45-47"' in output
    assert "normative: true" in output
    assert "# 7.4.2 Concrete Syntax" in output
    assert "<!-- informative:note -->" in output
    # Lossless: the verbatim block text survives rendering.
    assert "The model shall use the concrete syntax." in output
    assert "NOTE This guidance is informative." in output
    assert output.endswith("\n")
    assert "\r\n" not in output


def test_single_page_range_has_no_dash() -> None:
    clause = _clause()
    clause.page_end = clause.page_start
    assert 'pages: "45"' in render(clause)


def test_render_index_lists_clauses_with_links_and_count() -> None:
    scope = Clause(
        number="1",
        title="Scope | overview",
        document="KerML",
        version="1.0",
        page_start=1,
        page_end=1,
    )

    output = render_index([scope, _clause()])

    assert output.startswith("---\n")
    assert "clauses: 2" in output
    assert "# KerML 1.0 — specification clauses" in output
    assert "| Clause | Title | Pages | File |" in output
    assert "[07.04.02-concrete-syntax.md](07.04.02-concrete-syntax.md)" in output
    # Pipe characters in a title are escaped so the table stays intact.
    assert "Scope \\| overview" in output
