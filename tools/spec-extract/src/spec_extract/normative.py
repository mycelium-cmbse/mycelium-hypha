# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Classify a clause's body into normative prose and informative NOTE/EXAMPLE asides.

The split is lossless: every input line ends up in exactly one block. A ``NOTE``/``EXAMPLE`` marker
(uppercase, as the OMG specs render them) starts an informative block that runs until the next marker
or the clause end — matching the common layout where notes and examples trail the normative text. A
clause is flagged normative when any prose block contains "shall" or "must".
"""

from __future__ import annotations

import re

from spec_extract.model import Block, Clause

_NOTE_MARKER = re.compile(r"^NOTES?\b")
_EXAMPLE_MARKER = re.compile(r"^EXAMPLES?\b")
_NORMATIVE = re.compile(r"\b(?:shall|must)\b", re.IGNORECASE)


def classify(clause: Clause) -> Clause:
    """Re-split ``clause.blocks`` into text/note/example blocks and set ``is_normative`` in place."""
    lines = clause.text.split("\n") if clause.text else []
    clause.blocks = _classify_lines(lines)
    clause.is_normative = any(
        block.kind == "text" and _NORMATIVE.search(block.text) for block in clause.blocks
    )
    return clause


def _classify_lines(lines: list[str]) -> list[Block]:
    blocks: list[Block] = []
    kind: str = "text"
    buffer: list[str] = []

    def flush() -> None:
        if buffer:
            blocks.append(Block(kind, "\n".join(buffer)))
            buffer.clear()

    for line in lines:
        marker = _marker_kind(line)
        if marker is not None:
            flush()
            kind = marker
        buffer.append(line)
    flush()
    return blocks


def _marker_kind(line: str) -> str | None:
    if _NOTE_MARKER.match(line):
        return "note"
    if _EXAMPLE_MARKER.match(line):
        return "example"
    return None
