# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Compose the extraction layers: PDF -> ordered lines -> clauses -> per-clause markdown files."""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

from spec_extract import clauses, layout, markdown, normative, pdf_reader
from spec_extract.model import Clause


@dataclass(frozen=True)
class DocMeta:
    """Identifies a source specification and where its clause files are written."""

    document: str
    version: str
    out_subdir: str


def extract_document(pdf_path: str | Path, meta: DocMeta) -> list[Clause]:
    """Extract and classify all clauses from a spec PDF (pure: reads the PDF, writes nothing)."""
    pages = pdf_reader.extract_chars(pdf_path)
    lines = layout.assemble_lines(pages)
    detected = clauses.detect_clauses(lines, meta.document, meta.version)
    return [normative.classify(clause) for clause in detected]


def write_clauses(clauses_to_write: list[Clause], out_dir: str | Path) -> list[Path]:
    """Write one markdown file per clause into ``out_dir`` (created if needed). Returns sorted paths."""
    out_dir = Path(out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    written: list[Path] = []
    for clause in clauses_to_write:
        path = out_dir / markdown.clause_filename(clause)
        path.write_text(markdown.render(clause), encoding="utf-8", newline="\n")
        written.append(path)
    return sorted(written)


def write_index(clauses_to_index: list[Clause], out_dir: str | Path) -> Path:
    """Write an ``index.md`` table of contents for ``clauses_to_index`` into ``out_dir``."""
    out_dir = Path(out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    path = out_dir / "index.md"
    path.write_text(markdown.render_index(clauses_to_index), encoding="utf-8", newline="\n")
    return path


def write_index_json(clauses_to_index: list[Clause], out_dir: str | Path) -> Path:
    """Write the machine-readable ``index.json`` clause catalog into ``out_dir``."""
    out_dir = Path(out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    path = out_dir / "index.json"
    path.write_text(markdown.render_index_json(clauses_to_index), encoding="utf-8", newline="\n")
    return path
