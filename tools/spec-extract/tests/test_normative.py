# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Block classification: code, normative prose, informative NOTE/EXAMPLE — and losslessness."""

from __future__ import annotations

from spec_extract.model import Block, Clause, Line, Span
from spec_extract.normative import classify


def _line(text: str, *, code: bool = False) -> Line:
    style = "code" if code else "text"
    return Line(page=1, text=text, spans=[Span(text, style)], is_code=code)


def _clause(lines: list[Line]) -> Clause:
    return Clause(
        number="7.1",
        title="Example",
        document="KerML",
        version="1.0",
        page_start=1,
        page_end=1,
        blocks=[Block("text", lines)],
    )


def test_splits_text_note_and_example_and_flags_normative() -> None:
    lines = [
        _line("The element shall conform to the rules."),
        _line("It provides additional behaviour."),
        _line("NOTE This explanation is informative."),
        _line("More note text continues here."),
        _line("EXAMPLE 1 A short illustration."),
        _line("example body line."),
    ]

    clause = classify(_clause(lines))

    assert [block.kind for block in clause.blocks] == ["text", "note", "example"]
    assert clause.is_normative is True
    # Lossless: concatenating the blocks reproduces the input exactly.
    assert clause.text == "\n".join(line.text for line in lines)


def test_monospace_lines_become_a_code_block() -> None:
    lines = [
        _line("Intro prose."),
        _line("class Foo;", code=True),
        _line("attribute x;", code=True),
        _line("Trailing prose."),
    ]

    clause = classify(_clause(lines))

    assert [block.kind for block in clause.blocks] == ["text", "code", "text"]
    assert clause.is_normative is False


def test_capitalised_note_word_is_not_a_marker() -> None:
    clause = classify(_clause([_line("Note that this paragraph is ordinary prose.")]))

    assert [block.kind for block in clause.blocks] == ["text"]
    assert clause.is_normative is False
