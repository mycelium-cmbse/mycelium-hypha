# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Parse the KerML / SysML textual BNF into productions, and render a per-release grammar reference.

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
    assignments of the untyped productions it references, one level deep. Helpers are followed only
    when they declare no metaclass of their own, so an assignment is never attributed to an element
    the grammar assigns elsewhere.
    """
    by_name = {production.name: production for production in productions}
    untyped = {name for name, production in by_name.items() if production.produces is None}

    links: dict[str, dict[tuple[str, str], set[str]]] = {}
    for production in productions:
        element = _built_element(production, metaclasses)
        if element is None:
            continue

        bucket = links.setdefault(element, {})
        for contributor in _contributors(production, by_name, untyped):
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
    production: Production, by_name: dict[str, Production], untyped: set[str]
) -> list[Production]:
    """A production plus the untyped helper productions it references, one level deep."""
    contributors = [production]
    for referenced in _references(production.body):
        if referenced in untyped and referenced != production.name:
            contributors.append(by_name[referenced])
    return contributors


def render(grammar: str, tag: str, productions: list[Production]) -> str:
    """Render the grammar reference for one document, grouped by specification clause."""
    lines = [
        "---",
        f"grammar: {grammar}",
        f"tag: {tag}",
        "kind: grammar-reference",
        f"productions: {len(productions)}",
        "---",
        "",
        f"# {grammar} textual grammar — {tag}",
        "",
        "> Generated by `tools/spec-extract` from the committed `.kebnf` (EPL-2.0) — do not edit by",
        "> hand; re-run the generator.",
        "",
        "Each production shows the metaclass it builds (`produces`), the specification clause it is",
        "defined in, and the metamodel features it populates (`=` sets, `+=` adds, `?=` is a boolean",
        "flag). Both come from the grammar itself, not from name matching.",
        "",
    ]

    current: tuple[str | None, str] | None = None
    for production in productions:
        key = (production.clause, production.clause_title)
        if key != current:
            current = key
            heading = f"{production.clause} {production.clause_title}".strip() if production.clause else "Lexical"
            lines += [f"## {heading}", ""]

        lines.append(f"### {production.name}")
        lines.append("")
        facts = []
        if production.produces:
            facts.append(f"produces [{production.produces}](../metamodel/elements/{production.produces}.md)")
        if production.clause:
            facts.append(f"clause `{production.clause}`")
        if production.features:
            facts.append(
                "features "
                + ", ".join(f"`{feature} {operator}`" for feature, operator in production.features)
            )
        if facts:
            lines += [" · ".join(facts), ""]

        lines += ["```kebnf", production.body, "```", ""]

    return "\n".join(lines)
