# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Reconstruct reading-ordered lines from positioned words.

Handles the two structural quirks of the OMG spec PDFs: multi-column body text (read left column
fully, then right) and running headers/footers/page numbers (dropped). Everything here is a pure
function of ``list[list[Word]]`` so it can be tested with synthetic words.
"""

from __future__ import annotations

import math
import re
from collections import Counter

from spec_extract.model import Line, Word

_DIGITS = re.compile(r"\d+")

_Y_TOLERANCE = 3.0


def assemble_lines(
    pages: list[list[Word]],
    *,
    y_tolerance: float = _Y_TOLERANCE,
    repeat_ratio: float = 0.3,
) -> list[Line]:
    """Return the document's body lines in reading order, headers/footers/page numbers removed."""
    per_page = [_page_lines(words, y_tolerance) for words in pages]
    keep = _chrome_filter(per_page, repeat_ratio)
    return [line for page_lines in per_page for line in page_lines if keep(line)]


def _chrome_filter(per_page: list[list[Line]], repeat_ratio: float):
    """Build a predicate that rejects page chrome (running headers/footers and bare page numbers)."""
    page_count = len(per_page)
    # Key on a digit-normalized form so running footers/headers that embed the page number
    # ("Kernel Modeling Language v1.0 11", "... 12", ...) are recognised as the same repeated line.
    frequency: Counter[str] = Counter()
    for page_lines in per_page:
        for key in {_normalize(line.text) for line in page_lines}:
            frequency[key] += 1
    threshold = max(2, math.ceil(repeat_ratio * page_count))

    def keep(line: Line) -> bool:
        if line.text.strip().isdigit():
            return False
        if page_count > 1 and frequency[_normalize(line.text)] >= threshold:
            return False
        return True

    return keep


def _normalize(text: str) -> str:
    return _DIGITS.sub("#", text)


def _page_lines(words: list[Word], y_tolerance: float) -> list[Line]:
    """Assemble one page's words into ordered lines, splitting columns when a gutter is present."""
    if not words:
        return []

    boundary = _column_boundary(words, y_tolerance)
    if boundary is None:
        columns = [words]
    else:
        columns = [
            [word for word in words if (word.x0 + word.x1) / 2 < boundary],
            [word for word in words if (word.x0 + word.x1) / 2 >= boundary],
        ]

    lines: list[Line] = []
    for column in columns:
        for row in _cluster_rows(column, y_tolerance):
            text = " ".join(word.text for word in sorted(row, key=lambda w: w.x0))
            text = " ".join(text.split())
            if text:
                lines.append(Line(page=row[0].page, text=text))
    return lines


def _cluster_rows(words: list[Word], y_tolerance: float) -> list[list[Word]]:
    """Group words whose vertical positions fall within ``y_tolerance`` into rows (top to bottom)."""
    rows: list[list[Word]] = []
    for word in sorted(words, key=lambda w: (w.top, w.x0)):
        if rows and abs(rows[-1][0].top - word.top) <= y_tolerance:
            rows[-1].append(word)
        else:
            rows.append([word])
    return rows


def _column_boundary(words: list[Word], y_tolerance: float, min_fraction: float = 0.4) -> float | None:
    """Detect a two-column gutter: the mean midpoint of central gaps shared by enough rows.

    A page is treated as two-column only when at least ``min_fraction`` of its rows have an uncovered
    vertical band near the horizontal centre — so a full-width header line cannot mask real columns,
    and ordinary single-column prose (no consistent central gap) is left alone.
    """
    rows = _cluster_rows(words, y_tolerance)
    if len(rows) < 4:
        return None

    left = min(word.x0 for word in words)
    right = max(word.x1 for word in words)
    width = right - left
    if width <= 0:
        return None

    centre_lo, centre_hi = left + 0.35 * width, left + 0.65 * width
    gap_midpoints: list[float] = []
    for row in rows:
        for midpoint in _central_gaps(row, centre_lo, centre_hi, min_gap=0.03 * width):
            gap_midpoints.append(midpoint)
            break

    if len(gap_midpoints) >= min_fraction * len(rows):
        return sum(gap_midpoints) / len(gap_midpoints)
    return None


def _central_gaps(row: list[Word], centre_lo: float, centre_hi: float, min_gap: float):
    """Yield midpoints of uncovered horizontal bands in ``[centre_lo, centre_hi]`` within one row."""
    merged: list[list[float]] = []
    for word in sorted(row, key=lambda w: w.x0):
        if merged and word.x0 <= merged[-1][1]:
            merged[-1][1] = max(merged[-1][1], word.x1)
        else:
            merged.append([word.x0, word.x1])

    for (_, end), (start, _) in zip(merged, merged[1:], strict=False):
        midpoint = (end + start) / 2
        if start - end >= min_gap and centre_lo <= midpoint <= centre_hi:
            yield midpoint
