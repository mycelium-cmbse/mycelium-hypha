# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Classify a clause's body lines into code, normative prose and informative NOTE/EXAMPLE blocks.

The split is lossless: every body line ends up in exactly one block. Code lines (set in the monospace
font, flagged by the layout layer) group into ``code`` blocks. A ``NOTE``/``EXAMPLE`` marker
(uppercase, as the OMG specs render them) starts an informative block that runs until the next marker,
a code line, or the clause end — matching the common layout where notes and examples trail the
normative text. A clause is flagged normative when any prose block contains "shall" or "must".
"""

from __future__ import annotations

import re

from spec_extract.model import Block, Clause, Line

_NOTE_MARKER = re.compile(r"^NOTES?\b")
_EXAMPLE_MARKER = re.compile(r"^EXAMPLES?\b")
_NORMATIVE = re.compile(r"\b(?:shall|must)\b", re.IGNORECASE)


def classify(clause: Clause) -> Clause:
    """Re-split the clause body into code/text/note/example blocks and set ``is_normative`` in place."""
    lines = [line for block in clause.blocks for line in block.lines]
    clause.blocks = _classify_lines(lines)
    clause.is_normative = any(
        block.kind == "text" and _NORMATIVE.search(block.text) for block in clause.blocks
    )
    return clause


def _classify_lines(lines: list[Line]) -> list[Block]:
    blocks: list[Block] = []
    kind: str | None = None
    sticky = "text"  # the current non-code state; NOTE/EXAMPLE markers make it informative
    buffer: list[Line] = []

    def flush() -> None:
        if buffer:
            blocks.append(Block(kind, list(buffer)))
            buffer.clear()

    for line in lines:
        if line.is_code:
            line_kind = "code"
            sticky = "text"
        else:
            marker = _marker_kind(line.text)
            if marker is not None:
                sticky = marker
            line_kind = sticky

        if line_kind != kind:
            flush()
            kind = line_kind
        buffer.append(line)

    flush()
    return blocks


def _marker_kind(line: str) -> str | None:
    if _NOTE_MARKER.match(line):
        return "note"
    if _EXAMPLE_MARKER.match(line):
        return "example"
    return None
