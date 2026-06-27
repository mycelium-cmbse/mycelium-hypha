# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Plain data structures shared across the extraction pipeline.

These are intentionally dependency-free dataclasses so every pipeline layer can be unit-tested on
hand-written data without touching a PDF.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Literal

BlockKind = Literal["text", "note", "example", "code"]
SpanStyle = Literal["text", "italic", "bold", "code"]


@dataclass(frozen=True)
class Char:
    """A single glyph with its position, size and font (origin top-left, as pdfplumber reports)."""

    text: str
    x0: float
    x1: float
    top: float
    size: float
    fontname: str
    page: int


@dataclass(frozen=True)
class Span:
    """A run of same-styled text within a line (the unit of inline markdown emphasis)."""

    text: str
    style: SpanStyle


@dataclass(frozen=True)
class Word:
    """A single word with its position on a page (origin top-left, as pdfplumber reports)."""

    text: str
    x0: float
    x1: float
    top: float
    page: int


@dataclass(frozen=True)
class Line:
    """A line of text assembled from characters, with its source page, style spans and a code flag."""

    page: int
    text: str
    spans: list[Span] = field(default_factory=list)
    is_code: bool = False


@dataclass(frozen=True)
class Block:
    """A contiguous run of clause body lines of one kind (normative text, note, example or code)."""

    kind: BlockKind
    lines: list[Line] = field(default_factory=list)

    @property
    def text(self) -> str:
        """The block's plain text, lines joined."""
        return "\n".join(line.text for line in self.lines)


@dataclass
class Clause:
    """A specification clause: its heading, source location, and classified body blocks."""

    number: str
    title: str
    document: str
    version: str
    page_start: int
    page_end: int
    blocks: list[Block] = field(default_factory=list)
    is_normative: bool = False

    @property
    def text(self) -> str:
        """The full clause body, every block's lines joined — used to assert losslessness in tests."""
        return "\n".join(line.text for block in self.blocks for line in block.lines)
