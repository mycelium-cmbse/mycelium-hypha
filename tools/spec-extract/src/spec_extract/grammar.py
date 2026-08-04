# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Parse the KerML / SysML textual BNF into productions, and join them to the metamodel.

.. note::
   Parsing and rendering moved to ``Hypha.Knowledge.Grammar`` on the .NET side (see
   ``tools/knowledge-gen``), which now writes ``knowledge/<tag>/textual-notation/grammar-*.md``.
   What is left here is the join used by :mod:`spec_extract.crossrefs`, and it goes the same way
   when the cross-references move.

The `.kebnf` files carry far more than syntax. Three things are stated explicitly and are worth
extracting rather than inferring:

* ``PackageDeclaration : Package =`` – the **metaclass the production produces**. Roughly three times
  as many productions declare this as happen to be *named* after a metaclass, so it is a much better
  grammar↔metamodel join than name matching.
* ``// Clause 8.2.2.5.1 Packages`` – the **specification clause** the following productions belong
  to, stated by the grammar authors rather than matched on a title.
* ``ownedRelationship += PackageMember`` – the **metamodel feature** a piece of syntax populates,
  including ``=`` (single), ``+=`` (add) and ``?=`` (boolean flag).

Everything here is a pure function of the grammar text, so it is unit-tested without the sources.
"""

from __future__ import annotations

import re
from dataclasses import dataclass, field

# A production starts at column 0: "Name =" or "Name : Metaclass =". Bodies are indented.
# Compiled with re.ASCII so \w keeps its identifier meaning rather than matching Unicode letters.
_PRODUCTION = re.compile(r"^(?P<name>[A-Za-z]\w*)\s*(?::\s*(?P<produces>[A-Za-z]\w*)\s*)?=", re.ASCII)
# Every quantifier here is unambiguous, which keeps the match linear: possessive on the dotted
# clause number (it can never usefully be re-split), and no separator between the number and the
# title - a `\s*` there would compete with `.*` for the same spaces. The title is stripped by the
# caller instead.
_CLAUSE = re.compile(r"^//\s*Clause\s+(?P<clause>\d++(?:\.\d++)*+)(?P<title>.*)$", re.ASCII)
_ASSIGNMENT = re.compile(r"(?P<feature>[A-Za-z]\w*)\s*(?P<operator>\+=|\?=|=)(?!=)", re.ASCII)
_REFERENCE = re.compile(r"\b([A-Z]\w*)\b", re.ASCII)


@dataclass(frozen=True)
class Production:
    """One grammar production, with what the grammar itself says about it."""

    name: str
    produces: str | None
    clause: str | None
    clause_title: str
    body: str

    #: ``(feature, operator)`` pairs this production assigns. ``=`` sets a value, ``+=`` adds to a
    #: collection, and ``?=`` sets a boolean flag from the presence of a keyword.
    features: tuple[tuple[str, str], ...] = field(default=())


def parse(text: str) -> list[Production]:
    """Parse a ``.kebnf`` grammar into productions, in document order."""
    lines = text.replace("\r\n", "\n").split("\n")
    productions: list[Production] = []

    clause: str | None = None
    clause_title = ""
    current: dict | None = None
    body: list[str] = []

    def flush() -> None:
        if current is not None:
            text_body = "\n".join(body).rstrip()

            # Scan for feature assignments *after* the production's own "Name : Type =" header,
            # otherwise the production name itself reads as an assignment.
            header = _PRODUCTION.match(text_body)
            scanned = text_body[header.end() :] if header else text_body

            productions.append(
                Production(
                    name=current["name"],
                    produces=current["produces"],
                    clause=current["clause"],
                    clause_title=current["clause_title"],
                    body=text_body,
                    features=tuple(
                        sorted(
                            {
                                (match.group("feature"), match.group("operator"))
                                for match in _ASSIGNMENT.finditer(scanned)
                            }
                        )
                    ),
                )
            )

    for line in lines:
        heading = _CLAUSE.match(line)
        if heading:
            flush()
            current, body = None, []
            clause, clause_title = heading.group("clause"), heading.group("title").strip()
            continue

        start = _PRODUCTION.match(line)
        if start:
            flush()
            current = {
                "name": start.group("name"),
                "produces": start.group("produces"),
                "clause": clause,
                "clause_title": clause_title,
            }
            body = [line]
            continue

        if current is not None:
            body.append(line)

    flush()
    return productions


def metaclass_links(productions: list[Production], metaclasses: set[str]) -> dict[str, list[str]]:
    """Map each metaclass to the productions that build it, using the declared type then the name.

    Only names present in ``metaclasses`` are kept, so the grammar can never introduce an element
    the metamodel does not have.
    """
    links: dict[str, set[str]] = {}
    for production in productions:
        element = _built_element(production, metaclasses)
        if element is not None:
            links.setdefault(element, set()).add(production.name)
    return {name: sorted(links[name]) for name in sorted(links)}


def clause_links(productions: list[Production], metaclasses: set[str]) -> dict[str, list[str]]:
    """Map each metaclass to the clauses its productions are defined in, per the grammar's own
    ``// Clause`` attribution.

    This is what the grammar authors state, not a match on a clause title, so a consumer can cite it
    as read rather than inferred.
    """
    links: dict[str, set[str]] = {}
    for production in productions:
        element = _built_element(production, metaclasses)
        if production.clause and element is not None:
            links.setdefault(element, set()).add(production.clause)
    return {name: sorted(links[name], key=_clause_key) for name in sorted(links)}


def _references(body: str) -> list[str]:
    """The production names a body refers to: capitalised identifiers that are not quoted literals."""
    without_literals = re.sub(r"'[^']*'", " ", body)
    return _REFERENCE.findall(without_literals)


def _clause_key(clause: str) -> tuple[tuple[int, int, str], ...]:
    """Numeric ordering for a dotted clause number, so 8.3.10 follows 8.3.2."""
    return tuple((0, int(part), "") if part.isdigit() else (1, 0, part) for part in clause.split("."))


def feature_links(productions: list[Production], metaclasses: set[str]) -> dict[str, list[dict]]:
    """Map each metaclass to the metamodel features its productions populate, and how.

    Answers both directions the knowledge base could not previously answer: *what syntax sets this
    feature*, and *what does this piece of syntax populate*.

    Assignments frequently sit in an **untyped helper** production rather than the typed one –
    ``PartUsage`` delegates to ``PartUsageDeclaration`` – so a typed production also contributes the
    assignments of the helpers it reaches, transitively.

    Two guards keep that honest. A helper must declare no metaclass of its own, so an assignment is
    never taken from an element the grammar assigns elsewhere; and it must be referenced by exactly
    one production, so a helper shared between two elements cannot spread its assignments across
    both. Under-reporting is the better failure here than misattribution.
    """
    by_name = {production.name: production for production in productions}
    exclusive = _exclusive_helpers(productions, by_name)

    links: dict[str, dict[tuple[str, str], set[str]]] = {}
    for production in productions:
        element = _built_element(production, metaclasses)
        if element is None:
            continue

        bucket = links.setdefault(element, {})
        for contributor in _contributors(production, by_name, exclusive):
            for assignment in contributor.features:
                bucket.setdefault(assignment, set()).add(contributor.name)

    return {
        name: [
            {"feature": feature, "operator": operator, "productions": sorted(assignments[(feature, operator)])}
            for feature, operator in sorted(assignments)
        ]
        for name, assignments in sorted(links.items())
        if assignments
    }


def _built_element(production: Production, metaclasses: set[str]) -> str | None:
    """The metaclass a production builds: its declared type, else its own name, else nothing."""
    for candidate in (production.produces, production.name):
        if candidate and candidate in metaclasses:
            return candidate
    return None


def _contributors(
    production: Production,
    by_name: dict[str, Production],
    exclusive: set[str],
    max_depth: int = 4,
) -> list[Production]:
    """A production plus the untyped helper productions it reaches, transitively.

    Only *exclusive* helpers are followed: untyped productions referenced by exactly one typed
    production. A helper shared between two typed productions would otherwise spread its assignments
    across both, which is worse than under-reporting. Depth is bounded and visits are tracked, so a
    grammar that references itself cannot loop.
    """
    contributors = [production]
    seen = {production.name}
    frontier = [production]

    for _ in range(max_depth):
        following = []
        for current in frontier:
            for referenced in _references(current.body):
                if referenced in exclusive and referenced not in seen:
                    seen.add(referenced)
                    helper = by_name[referenced]
                    contributors.append(helper)
                    following.append(helper)
        if not following:
            break
        frontier = following

    return contributors


def _exclusive_helpers(productions: list[Production], by_name: dict[str, Production]) -> set[str]:
    """Untyped productions referenced by exactly one typed production, directly or via another helper.

    Being referenced once is what makes a helper safe to attribute: its assignments can only belong
    to the single element that reaches it.
    """
    untyped = {name for name, production in by_name.items() if production.produces is None}

    referrers: dict[str, set[str]] = {}
    for production in productions:
        for referenced in _references(production.body):
            if referenced in untyped and referenced != production.name:
                referrers.setdefault(referenced, set()).add(production.name)

    return {name for name, callers in referrers.items() if len(callers) == 1}
