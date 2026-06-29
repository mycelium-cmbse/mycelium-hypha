# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Reconstruct reading-ordered, style-aware lines from positioned characters.

Working at the character level (rather than pdfplumber's pre-grouped words) lets us (a) rebuild
inter-word spaces that get dropped in italic/bold runs, and (b) recover font styling — code, bold,
italic — as inline spans. It still handles the structural quirks of the OMG spec PDFs: multi-column
body text (read left column fully, then right) and running headers/footers/page numbers (dropped).
Everything here is a pure function of ``list[list[Char]]`` so it can be tested with synthetic chars.
"""

from __future__ import annotations

import math
import re
from collections import Counter

from spec_extract.model import Char, Line, Span, SpanStyle

_Y_TOLERANCE = 3.0
_SPACE_RATIO = 0.25  # a gap wider than this fraction of the glyph size is a word break
_DIGITS = re.compile(r"\d+")
_MULTISPACE = re.compile(r" {2,}")


def style_for(fontname: str) -> SpanStyle:
    """Map a PDF font name to an inline style. Courier (mono) wins, then italic, then bold."""
    name = fontname or ""
    if "Courier" in name or "Mono" in name:
        return "code"
    if "Italic" in name or "Oblique" in name:
        return "italic"
    if "Bold" in name:
        return "bold"
    return "text"


def assemble_lines(
    pages: list[list[Char]],
    *,
    y_tolerance: float = _Y_TOLERANCE,
    repeat_ratio: float = 0.3,
) -> list[Line]:
    """Return the document's body lines in reading order, headers/footers/page numbers removed."""
    per_page = [_page_lines(chars, y_tolerance) for chars in pages]
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


def _page_lines(chars: list[Char], y_tolerance: float) -> list[Line]:
    """Assemble one page's chars into ordered lines, splitting columns when a gutter is present."""
    if not chars:
        return []

    boundary = _column_boundary(chars, y_tolerance)
    if boundary is None:
        columns = [chars]
    else:
        columns = [
            [char for char in chars if (char.x0 + char.x1) / 2 < boundary],
            [char for char in chars if (char.x0 + char.x1) / 2 >= boundary],
        ]

    lines: list[Line] = []
    for column in columns:
        for row in _cluster_rows(column, y_tolerance):
            line = _build_line(row)
            if line.text:
                lines.append(line)
    return lines


class _SpanBuilder:
    """Accumulate glyphs into style-runs, flushing a new ``Span`` whenever the style changes."""

    def __init__(self) -> None:
        self._spans: list[Span] = []
        self._style: SpanStyle | None = None
        self._buffer: list[str] = []

    def add_space(self) -> None:
        if self._buffer and not self._buffer[-1].endswith(" "):
            self._buffer.append(" ")

    def add(self, text: str, style: SpanStyle, separator: str) -> None:
        if style != self._style:
            self._flush()
            self._style = style
        self._buffer.append(separator + text)

    def _flush(self) -> None:
        if self._buffer:
            self._spans.append(Span("".join(self._buffer), self._style or "text"))
            self._buffer = []

    def spans(self) -> list[Span]:
        self._flush()
        return self._spans


def _separator(previous: Char | None, char: Char) -> str:
    """A reinserted space when a wide horizontal gap separates two non-space glyphs."""
    if previous is None or previous.text.isspace():
        return ""
    return " " if char.x0 - previous.x1 > _SPACE_RATIO * char.size else ""


def _build_line(row: list[Char]) -> Line:
    """Reconstruct a line's text + style spans from its characters, restoring dropped spaces."""
    chars = sorted(row, key=lambda c: c.x0)
    builder = _SpanBuilder()
    previous: Char | None = None

    for char in chars:
        if char.text == "":
            continue
        if char.text.isspace():
            builder.add_space()
            previous = char
            continue
        builder.add(char.text, style_for(char.fontname), _separator(previous, char))
        previous = char

    spans = builder.spans()
    text = _MULTISPACE.sub(" ", "".join(span.text for span in spans)).strip()
    page = chars[0].page if chars else 0
    return Line(page=page, text=text, spans=spans, is_code=_is_code(chars))


def _is_code(chars: list[Char]) -> bool:
    glyphs = [char for char in chars if not char.text.isspace()]
    if not glyphs:
        return False
    code = sum(1 for char in glyphs if style_for(char.fontname) == "code")
    return code > 0.5 * len(glyphs)


def _cluster_rows(chars: list[Char], y_tolerance: float) -> list[list[Char]]:
    """Group characters whose vertical positions fall within ``y_tolerance`` into rows (top to bottom)."""
    rows: list[list[Char]] = []
    for char in sorted(chars, key=lambda c: (c.top, c.x0)):
        if rows and abs(rows[-1][0].top - char.top) <= y_tolerance:
            rows[-1].append(char)
        else:
            rows.append([char])
    return rows


def _column_boundary(chars: list[Char], y_tolerance: float, min_fraction: float = 0.4) -> float | None:
    """Detect a two-column gutter: the mean midpoint of central gaps shared by enough rows.

    A page is treated as two-column only when at least ``min_fraction`` of its rows have an uncovered
    vertical band near the horizontal centre — so a full-width header line cannot mask real columns,
    and ordinary single-column prose (no consistent central gap) is left alone.
    """
    rows = _cluster_rows(chars, y_tolerance)
    if len(rows) < 4:
        return None

    left = min(char.x0 for char in chars)
    right = max(char.x1 for char in chars)
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


def _central_gaps(row: list[Char], centre_lo: float, centre_hi: float, min_gap: float):
    """Yield midpoints of uncovered horizontal bands in ``[centre_lo, centre_hi]`` within one row."""
    merged: list[list[float]] = []
    for char in sorted(row, key=lambda c: c.x0):
        if merged and char.x0 <= merged[-1][1]:
            merged[-1][1] = max(merged[-1][1], char.x1)
        else:
            merged.append([char.x0, char.x1])

    for (_, end), (start, _) in zip(merged, merged[1:], strict=False):
        midpoint = (end + start) / 2
        if start - end >= min_gap and centre_lo <= midpoint <= centre_hi:
            yield midpoint
