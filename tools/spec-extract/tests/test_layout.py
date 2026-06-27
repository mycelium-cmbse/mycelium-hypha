# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Layout reconstruction: line clustering, multi-column reading order, header/footer stripping."""

from __future__ import annotations

from spec_extract.layout import assemble_lines
from spec_extract.model import Word


def _word(text: str, x0: float, top: float, page: int = 1, width: float = 60.0) -> Word:
    return Word(text=text, x0=x0, x1=x0 + width, top=top, page=page)


def test_single_column_lines_in_reading_order() -> None:
    page = [
        _word("First", 50, 100),
        _word("line", 115, 100),
        _word("Second", 50, 120),
        _word("line", 120, 120),
    ]

    lines = assemble_lines([page])

    assert [line.text for line in lines] == ["First line", "Second line"]


def test_two_columns_read_left_then_right() -> None:
    page = []
    for index, top in enumerate((100, 120, 140, 160)):
        page.append(_word(f"L{index}", 50, top, width=120))   # left column, x 50..170
        page.append(_word(f"R{index}", 320, top, width=120))  # right column, x 320..440 (gutter 170..320)

    lines = assemble_lines([page])

    assert [line.text for line in lines] == ["L0", "L1", "L2", "L3", "R0", "R1", "R2", "R3"]


def test_running_header_and_page_numbers_are_stripped() -> None:
    page1 = [
        _word("Running", 50, 10),
        _word("Header", 115, 10),
        _word("Alpha", 50, 100),
        _word("beta", 110, 100),
        _word("7", 300, 760),
    ]
    page2 = [
        _word("Running", 50, 10, page=2),
        _word("Header", 115, 10, page=2),
        _word("Gamma", 50, 100, page=2),
        _word("delta", 110, 100, page=2),
        _word("8", 300, 760, page=2),
    ]

    lines = assemble_lines([page1, page2])

    assert [line.text for line in lines] == ["Alpha beta", "Gamma delta"]
    assert all(line.text not in {"Running Header", "7", "8"} for line in lines)
