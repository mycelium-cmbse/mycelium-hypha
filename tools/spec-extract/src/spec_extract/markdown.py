# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Render a :class:`Clause` to a deterministic markdown file (front matter + tagged body).

Informative blocks are wrapped in HTML comment markers rather than rewritten, so the original text is
preserved verbatim while staying greppable (`<!-- informative:note -->`). Filenames zero-pad each
clause segment so a lexical sort matches document order.
"""

from __future__ import annotations

import re

from spec_extract.model import Clause

_SLUG_STRIP = re.compile(r"[^a-z0-9]+")


def clause_filename(clause: Clause) -> str:
    """Deterministic file name, e.g. clause 7.4.2 "Concrete Syntax" -> ``07.04.02-concrete-syntax.md``."""
    padded = ".".join(f"{int(part):02d}" for part in clause.number.split("."))
    slug = _slugify(clause.title)
    return f"{padded}-{slug}.md" if slug else f"{padded}.md"


def render_index(clauses: list[Clause]) -> str:
    """Render a table-of-contents ``index.md`` for one document's clauses, in document order."""
    document = clauses[0].document if clauses else ""
    version = clauses[0].version if clauses else ""

    front_matter = "\n".join(
        [
            "---",
            f"document: {_yaml_string(document)}",
            f'version: "{version}"',
            f"clauses: {len(clauses)}",
            "---",
        ]
    )
    heading = f"# {document} {version} — specification clauses".strip()

    rows = ["| Clause | Title | Pages | File |", "| --- | --- | --- | --- |"]
    for clause in clauses:
        file_name = clause_filename(clause)
        rows.append(
            f"| {clause.number} | {_escape_cell(clause.title)} | {_page_range(clause)} | "
            f"[{file_name}]({file_name}) |"
        )

    table = "\n".join(rows)
    return f"{front_matter}\n\n{heading}\n\n{table}\n".replace("\r\n", "\n")


def render(clause: Clause) -> str:
    """Return the full markdown document for ``clause`` (LF endings, trailing newline)."""
    front_matter = "\n".join(
        [
            "---",
            f'clause: "{clause.number}"',
            f"title: {_yaml_string(clause.title)}",
            f"document: {_yaml_string(clause.document)}",
            f'version: "{clause.version}"',
            f'pages: "{_page_range(clause)}"',
            f"normative: {str(clause.is_normative).lower()}",
            "---",
        ]
    )
    body = _render_body(clause)
    heading = f"# {clause.number} {clause.title}".rstrip()
    return f"{front_matter}\n\n{heading}\n\n{body}\n".replace("\r\n", "\n")


def _render_body(clause: Clause) -> str:
    parts: list[str] = []
    for block in clause.blocks:
        if not block.text:
            continue
        if block.kind == "text":
            parts.append(block.text)
        else:
            parts.append(f"<!-- informative:{block.kind} -->\n{block.text}\n<!-- /informative -->")
    return "\n\n".join(parts)


def _page_range(clause: Clause) -> str:
    if clause.page_end > clause.page_start:
        return f"{clause.page_start}-{clause.page_end}"
    return str(clause.page_start)


def _yaml_string(value: str) -> str:
    """Quote a scalar for YAML front matter, escaping embedded quotes/backslashes."""
    escaped = value.replace("\\", "\\\\").replace('"', '\\"')
    return f'"{escaped}"'


def _escape_cell(value: str) -> str:
    """Escape a value for a markdown table cell (pipes break the column layout)."""
    return value.replace("|", "\\|")


def _slugify(title: str) -> str:
    slug = _SLUG_STRIP.sub("-", title.lower()).strip("-")
    return slug[:60].rstrip("-")
