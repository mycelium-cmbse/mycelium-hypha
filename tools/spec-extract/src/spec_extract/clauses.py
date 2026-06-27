# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Detect clause/section boundaries in reading-ordered lines and group body text under each clause.

A line is only accepted as a heading when its number is the legitimate *successor* of the previous
clause (a child, sibling, or ancestor's sibling). That single rule both rejects stray in-text numbers
(e.g. "2.5 times") and skips the table of contents — whose dot-leader entries are filtered first and
whose numbers, even if matched, would not form a contiguous successor chain with the real body.
"""

from __future__ import annotations

import re

from spec_extract.model import Block, Clause, Line

_HEADING = re.compile(r"^(\d+(?:\.\d+)*)\s+(\S.*)$")
_TOC_LEADER = re.compile(r"\.{2,}|…")  # dot leaders or an ellipsis -> a contents entry


def detect_clauses(lines: list[Line], document: str, version: str) -> list[Clause]:
    """Return the clauses found in ``lines``, each with its raw body in a single ``text`` block."""
    headings = _detect_headings(lines)

    clauses: list[Clause] = []
    for position, (start, number, title) in enumerate(headings):
        end = headings[position + 1][0] if position + 1 < len(headings) else len(lines)
        body = lines[start + 1 : end]
        page_start = lines[start].page
        page_end = body[-1].page if body else page_start
        clauses.append(
            Clause(
                number=number,
                title=title,
                document=document,
                version=version,
                page_start=page_start,
                page_end=page_end,
                blocks=[Block("text", body)],
            )
        )
    return clauses


def _detect_headings(lines: list[Line]) -> list[tuple[int, str, str]]:
    """Return ``(line_index, number, title)`` for every accepted heading, in document order."""
    headings: list[tuple[int, str, str]] = []
    previous: tuple[int, ...] | None = None
    for index, line in enumerate(lines):
        match = _HEADING.match(line.text)
        if match is None:
            continue
        number, title = match.group(1), match.group(2).strip()
        if _TOC_LEADER.search(title):
            continue
        current = _parse_number(number)
        if _is_successor(previous, current):
            headings.append((index, number, title))
            previous = current
    return headings


def _parse_number(number: str) -> tuple[int, ...]:
    return tuple(int(part) for part in number.split("."))


def _is_successor(previous: tuple[int, ...] | None, current: tuple[int, ...]) -> bool:
    """True when ``current`` legitimately follows ``previous`` in clause numbering."""
    if previous is None:
        return current == (1,)  # the body starts at clause 1
    if current == previous + (1,):
        return True  # first child, e.g. 7.4 -> 7.4.1
    level = len(current)
    if 1 <= level <= len(previous):
        # sibling (level == len(previous)) or an ancestor's next sibling, e.g. 7.4.2 -> 7.5 or 8
        shares_ancestors = current[: level - 1] == previous[: level - 1]
        increments_tail = current[level - 1] == previous[level - 1] + 1
        if shares_ancestors and increments_tail:
            return True
    return False
