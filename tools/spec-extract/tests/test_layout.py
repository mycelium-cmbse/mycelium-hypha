# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Layout reconstruction: spacing repair, style spans, code detection, columns, header/footer."""

from __future__ import annotations

from spec_extract.layout import assemble_lines
from spec_extract.model import Char

TIMES = "TimesNewRomanPSMT"
ITALIC = "TimesNewRomanPS-ItalicMT"
BOLD = "Arial-BoldMT"
CODE = "CourierNewPSMT"


def _row(
    tokens: list[tuple[str, str]],
    *,
    top: float = 100.0,
    x0: float = 50.0,
    size: float = 10.0,
    char_w: float = 6.0,
    gap: float = 3.0,
    page: int = 1,
) -> list[Char]:
    """Lay tokens out left-to-right on one row; ``gap`` (> 0.25*size) separates tokens with a space."""
    chars: list[Char] = []
    x = x0
    for index, (text, font) in enumerate(tokens):
        if index > 0:
            x += gap
        for glyph in text:
            chars.append(Char(text=glyph, x0=x, x1=x + char_w, top=top, size=size, fontname=font, page=page))
            x += char_w
    return chars


def test_single_column_lines_in_reading_order() -> None:
    page = _row([("First", TIMES), ("line", TIMES)], top=100) + _row([("Second", TIMES), ("line", TIMES)], top=120)

    lines = assemble_lines([page])

    assert [line.text for line in lines] == ["First line", "Second line"]


def test_dropped_space_between_italic_words_is_restored() -> None:
    page = _row([("documented", ITALIC), ("element", ITALIC)], top=100)

    lines = assemble_lines([page])

    assert lines[0].text == "documented element"
    assert [span.style for span in lines[0].spans] == ["italic"]


def test_inline_code_word_becomes_a_code_span() -> None:
    page = _row([("use", TIMES), ("body", CODE), ("now", TIMES)], top=100)

    line = assemble_lines([page])[0]

    assert line.text == "use body now"
    assert line.is_code is False
    assert any(span.style == "code" and span.text.strip() == "body" for span in line.spans)


def test_monospace_line_is_flagged_as_code() -> None:
    page = _row([("class", CODE), ("Foo;", CODE)], top=100)

    assert assemble_lines([page])[0].is_code is True


def test_bold_run_is_a_bold_span() -> None:
    page = _row([("Plain", TIMES), ("strong", BOLD)], top=100)

    line = assemble_lines([page])[0]

    assert line.text == "Plain strong"
    assert any(span.style == "bold" and span.text.strip() == "strong" for span in line.spans)


def test_two_columns_read_left_then_right() -> None:
    page: list[Char] = []
    for index, top in enumerate((100, 120, 140, 160)):
        page += _row([(f"L{index}", TIMES)], top=top, x0=50)
        page += _row([(f"R{index}", TIMES)], top=top, x0=320)

    lines = assemble_lines([page])

    assert [line.text for line in lines] == ["L0", "L1", "L2", "L3", "R0", "R1", "R2", "R3"]


def test_running_header_and_page_numbers_are_stripped() -> None:
    page1 = (
        _row([("Running", TIMES), ("Header", TIMES)], top=10)
        + _row([("Alpha", TIMES), ("beta", TIMES)], top=100)
        + _row([("7", TIMES)], top=760)
    )
    page2 = (
        _row([("Running", TIMES), ("Header", TIMES)], top=10, page=2)
        + _row([("Gamma", TIMES), ("delta", TIMES)], top=100, page=2)
        + _row([("8", TIMES)], top=760, page=2)
    )

    lines = assemble_lines([page1, page2])

    assert [line.text for line in lines] == ["Alpha beta", "Gamma delta"]
