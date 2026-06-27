# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Markdown rendering: file naming, front matter, inline emphasis/code, fenced code, index."""

from __future__ import annotations

from spec_extract.markdown import clause_filename, render, render_index
from spec_extract.model import Block, Clause, Line, Span


def _line(text: str, spans: list[Span] | None = None) -> Line:
    return Line(page=45, text=text, spans=spans or [Span(text, "text")])


def _clause() -> Clause:
    return Clause(
        number="7.4.2",
        title="Concrete Syntax",
        document="KerML",
        version="1.0",
        page_start=45,
        page_end=47,
        blocks=[
            Block("text", [_line("The model shall use the concrete syntax.")]),
            Block("note", [_line("NOTE This guidance is informative.")]),
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
    assert "The model shall use the concrete syntax." in output
    assert output.endswith("\n")
    assert "\r\n" not in output


def test_inline_spans_render_emphasis_and_code_with_spaces_outside_markers() -> None:
    line = Line(
        page=1,
        text="a term word use body now",
        spans=[
            Span("a ", "text"),
            Span("term", "italic"),
            Span(" word", "bold"),
            Span(" use", "text"),
            Span(" body", "code"),
            Span(" now", "text"),
        ],
    )
    clause = Clause("1", "x", "KerML", "1.0", 1, 1, blocks=[Block("text", [line])])

    output = render(clause)

    assert "a *term* **word** use `body` now" in output


def test_code_block_is_fenced() -> None:
    clause = Clause(
        "1",
        "x",
        "KerML",
        "1.0",
        1,
        1,
        blocks=[Block("code", [Line(page=1, text="class Foo;"), Line(page=1, text="attribute x;")])],
    )

    assert "```\nclass Foo;\nattribute x;\n```" in render(clause)


def test_single_page_range_has_no_dash() -> None:
    clause = _clause()
    clause.page_end = clause.page_start
    assert 'pages: "45"' in render(clause)


def test_render_index_lists_clauses_with_links_and_count() -> None:
    scope = Clause("1", "Scope | overview", "KerML", "1.0", 1, 1)

    output = render_index([scope, _clause()])

    assert output.startswith("---\n")
    assert "clauses: 2" in output
    assert "# KerML 1.0 — specification clauses" in output
    assert "| Clause | Title | Pages | File |" in output
    assert "[07.04.02-concrete-syntax.md](07.04.02-concrete-syntax.md)" in output
    assert "Scope \\| overview" in output
