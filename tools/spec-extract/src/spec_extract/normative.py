# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Classify a clause's body lines into code, normative prose and informative NOTE/EXAMPLE blocks.

The split is lossless: every body line ends up in exactly one block. Code lines (set in the monospace
font, flagged by the layout layer) group into ``code`` blocks. An informative block starts at a
note/example label and runs until the next marker, a code line, or the clause end — matching the
layout where notes and examples trail the normative text.

Label detection is font-aware: the KerML/SysML PDFs render formal notes as a **bold** "Note." label
(not uppercase "NOTE"), so a bold leading "Note"/"Example" token marks the block — which also avoids
mistaking ordinary prose ("Note that ...", in the regular font) for a note. A bare uppercase
"NOTE"/"EXAMPLE" at the start of a line is still accepted as a fallback. A clause is flagged normative
when any prose block contains "shall" or "must".
"""

from __future__ import annotations

import re

from spec_extract.model import Block, Clause, Line

_LABEL = re.compile(r"(?i)^(note|example)s?\b")
_UPPER_NOTE = re.compile(r"^NOTES?\b")
_UPPER_EXAMPLE = re.compile(r"^EXAMPLES?\b")
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
    sticky = "text"  # the current non-code state; note/example labels make it informative
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
            marker = _marker_kind(line)
            if marker is not None:
                sticky = marker
            line_kind = sticky

        if line_kind != kind:
            flush()
            kind = line_kind
        buffer.append(line)

    flush()
    return blocks


def _marker_kind(line: Line) -> str | None:
    """Return 'note'/'example' if the line opens an informative block, else None."""
    bold = _bold_label(line)
    if bold is not None:
        return bold
    # Fallback: a bare uppercase NOTE/EXAMPLE at the start of the line (any font).
    if _UPPER_NOTE.match(line.text):
        return "note"
    if _UPPER_EXAMPLE.match(line.text):
        return "example"
    return None


def _bold_label(line: Line) -> str | None:
    """If the line's first visible span is a bold 'Note'/'Example' label, return its kind."""
    for span in line.spans:
        text = span.text.strip()
        if not text:
            continue
        if span.style != "bold":
            return None  # first visible token isn't bold -> ordinary prose, not a formal label
        match = _LABEL.match(text)
        if match is None:
            return None
        return "note" if match.group(1).lower().startswith("n") else "example"
    return None
