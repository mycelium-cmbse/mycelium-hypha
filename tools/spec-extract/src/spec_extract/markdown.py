# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Render a :class:`Clause` to a deterministic markdown file (front matter + tagged body).

Informative blocks are wrapped in HTML comment markers rather than rewritten, so the original text is
preserved verbatim while staying greppable (`<!-- informative:note -->`). Filenames zero-pad each
clause segment so a lexical sort matches document order.
"""

from __future__ import annotations

import json
import re

from spec_extract.model import Block, Clause, Line, Span

_SLUG_STRIP = re.compile(r"[^a-z0-9]+")

SPEC_INDEX_SCHEMA_VERSION = "1.0.0"


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


def render_index_json(clauses: list[Clause]) -> str:
    """Render a machine-readable ``index.json`` catalog of one document's clauses (document order).

    The twin of ``index.md``: ``entries`` is keyed by clause number for O(1) lookup. Like the rest of
    ``knowledge/spec/`` it is git-ignored (spec-derived); it carries metadata only, never clause text.
    """
    document = clauses[0].document if clauses else ""
    version = clauses[0].version if clauses else ""
    payload = {
        "schemaVersion": SPEC_INDEX_SCHEMA_VERSION,
        "document": document,
        "version": version,
        "clauses": len(clauses),
        "entries": {
            clause.number: {
                "title": clause.title,
                "pages": _page_range(clause),
                "normative": clause.is_normative,
                "file": clause_filename(clause),
            }
            for clause in clauses
        },
    }
    return json.dumps(payload, indent=2, ensure_ascii=False) + "\n"


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
    parts = [rendered for block in clause.blocks if (rendered := _render_block(block))]
    return "\n\n".join(parts)


def _render_block(block: Block) -> str:
    if block.kind == "code":
        code = "\n".join(line.text for line in block.lines)
        return f"```\n{code}\n```" if code.strip() else ""

    body = "\n".join(_render_line(line) for line in block.lines)
    if not body.strip():
        return ""
    if block.kind in ("note", "example"):
        return f"<!-- informative:{block.kind} -->\n{body}\n<!-- /informative -->"
    return body


def _render_line(line: Line) -> str:
    if not line.spans:
        return line.text
    return "".join(_render_span(span) for span in line.spans)


def _render_span(span: Span) -> str:
    """Render an inline span, keeping any surrounding spaces *outside* the emphasis markers."""
    if span.style == "text" or not span.text.strip():
        return span.text
    marker = {"italic": "*", "bold": "**", "code": "`"}[span.style]
    lead = " " * (len(span.text) - len(span.text.lstrip(" ")))
    trail = " " * (len(span.text) - len(span.text.rstrip(" ")))
    return f"{lead}{marker}{span.text.strip(' ')}{marker}{trail}"


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
