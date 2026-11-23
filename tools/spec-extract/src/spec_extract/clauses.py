# SPDX-FileCopyrightText: 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Detect clause/section boundaries in reading-ordered lines and group body text under each clause.

A line is only accepted as a heading when its number is the legitimate *successor* of the previous
clause (a child, sibling, or ancestor's sibling). That single rule both rejects stray in-text numbers
(e.g. "2.5 times") and skips the table of contents, whose dot-leader entries are filtered first and
whose numbers, even if matched, would not form a contiguous successor chain with the real body.

The annexes follow the numbered clauses and are lettered: ``A Annex: Model Execution``, then
``A.1``, ``A.3.2`` and so on, then ``B Annex: ...``. A bare letter is only a heading when its title
starts with "Annex", since a sentence such as "A tool shall ..." starts the same way. The first annex
is ``A``, which may follow any numbered clause; within the annexes the same successor rule applies,
the letter counting as the first part; and once an annex has started, no numbered heading is
accepted, which keeps the in-text numbers of the annexes out.

An annex heading is followed by "(Normative)" or "(Informative)", which applies to the annex and
every one of its subclauses. A heading too long for one line wraps, so the lines between the heading
and that marker finish its title: ``B Annex: API and Services Examples and`` then ``Cookbook``.
"""

from __future__ import annotations

import re
from dataclasses import dataclass

from spec_extract.model import Block, Clause, Line

_HEADING = re.compile(r"^((?:\d+|[A-Z])(?:\.\d+)*)\s+(\S.*)$")
_TOC_LEADER = re.compile(r"\.{2,}|…")  # dot leaders or an ellipsis -> a contents entry
_ANNEX_TITLE = re.compile(r"^Annex\b")
_ANNEX_STATUS = re.compile(r"^\((Normative|Informative)\)$")
_MAX_TITLE_LINES = 3  # the lines an annex title may wrap onto before its status marker

Number = tuple[int | str, ...]


def detect_clauses(lines: list[Line], document: str, version: str) -> list[Clause]:
    """Return the clauses found in ``lines``, each with its raw body in a single ``text`` block."""
    headings = _detect_headings(lines)

    clauses: list[Clause] = []
    for position, heading in enumerate(headings):
        start = heading.index
        end = headings[position + 1].index if position + 1 < len(headings) else len(lines)
        body = lines[heading.body : end]
        page_start = lines[start].page
        page_end = body[-1].page if body else page_start
        clauses.append(
            Clause(
                number=heading.number,
                title=heading.title,
                document=document,
                version=version,
                page_start=page_start,
                page_end=page_end,
                blocks=[Block("text", body)],
                declared_normative=heading.declared_normative,
            )
        )
    return clauses


@dataclass(frozen=True)
class _Heading:
    """An accepted heading: where it is, where its body starts, and what it says."""

    index: int
    body: int
    number: str
    title: str
    declared_normative: bool | None


def _detect_headings(lines: list[Line]) -> list[_Heading]:
    """Return every accepted heading, in document order."""
    headings: list[_Heading] = []
    previous: Number | None = None
    annex_status: bool | None = None
    for index, line in enumerate(lines):
        match = _HEADING.match(line.text)
        if match is None:
            continue
        number, title = match.group(1), match.group(2).strip()
        if _TOC_LEADER.search(title):
            continue
        current = _parse_number(number)
        is_annex_root = _is_annex(current) and len(current) == 1
        if is_annex_root and not _ANNEX_TITLE.match(title):
            continue
        if not _is_successor(previous, current):
            continue
        body = index + 1
        if is_annex_root:
            wrapped, annex_status = _annex_status(lines, index)
            if wrapped:
                title = " ".join([title, *(lines[k].text.strip() for k in range(body, body + wrapped))])
                body += wrapped
        status = annex_status if _is_annex(current) else None
        headings.append(_Heading(index, body, number, title, status))
        previous = current
    return headings


def _annex_status(lines: list[Line], index: int) -> tuple[int, bool | None]:
    """How many lines the annex heading at ``index`` wraps onto, and whether it is normative.

    The status marker is looked for on the heading's own page, within a few lines; without one the
    heading is taken to be one line long and its status unknown.
    """
    page = lines[index].page
    for offset in range(1, _MAX_TITLE_LINES + 2):
        position = index + offset
        if position >= len(lines) or lines[position].page != page:
            break
        text = lines[position].text.strip()
        marker = _ANNEX_STATUS.match(text)
        if marker:
            return offset - 1, marker.group(1) == "Normative"
        if _HEADING.match(text):
            break
    return 0, None


def _parse_number(number: str) -> Number:
    return tuple(part if part.isalpha() else int(part) for part in number.split("."))


def _is_annex(number: Number) -> bool:
    return isinstance(number[0], str)


def _next(part: int | str) -> int | str:
    return chr(ord(part) + 1) if isinstance(part, str) else part + 1


def _is_successor(previous: Number | None, current: Number) -> bool:
    """True when ``current`` legitimately follows ``previous`` in clause numbering."""
    if previous is None:
        return current == (1,)  # the body starts at clause 1
    if _is_annex(current) != _is_annex(previous):
        # the first annex follows any numbered clause; no numbered clause follows an annex
        return _is_annex(current) and current == ("A",)
    if current == previous + (1,):
        return True  # first child, e.g. 7.4 -> 7.4.1
    level = len(current)
    if 1 <= level <= len(previous):
        # sibling (level == len(previous)) or an ancestor's next sibling, e.g. 7.4.2 -> 7.5 or 8
        shares_ancestors = current[: level - 1] == previous[: level - 1]
        increments_tail = current[level - 1] == _next(previous[level - 1])
        if shares_ancestors and increments_tail:
            return True
    return False
