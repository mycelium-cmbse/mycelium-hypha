# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Plain data structures shared across the extraction pipeline.

These are intentionally dependency-free dataclasses so every pipeline layer can be unit-tested on
hand-written data without touching a PDF.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Literal

BlockKind = Literal["text", "note", "example"]


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
    """A line of text assembled from words, tagged with its source page (1-based)."""

    page: int
    text: str


@dataclass(frozen=True)
class Block:
    """A contiguous run of clause body text, classified as normative prose or an informative aside."""

    kind: BlockKind
    text: str


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
        """The full clause body, blocks joined — used to assert losslessness in tests."""
        return "\n".join(block.text for block in self.blocks)
