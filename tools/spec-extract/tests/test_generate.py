# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Regenerate the (git-ignored) spec knowledge base from the local PDFs.

This mirrors how the C# metamodel-gen tests write the knowledge base: running pytest with the OMG
PDFs present in ``sources/specs/`` populates ``knowledge/spec/{kerml,sysml2}/``; without them it skips.
The output is git-ignored, so it is never committed.
"""

from __future__ import annotations

from pathlib import Path

from spec_extract.pipeline import DocMeta, extract_document, write_clauses, write_index


def test_regenerates_spec_knowledge_base(repo_root: Path, kerml_pdf: Path, sysml_pdf: Path) -> None:
    targets = [
        (kerml_pdf, DocMeta("KerML", "1.0", "kerml")),
        (sysml_pdf, DocMeta("SysML", "2.0", "sysml2")),
    ]

    for pdf_path, meta in targets:
        clauses = extract_document(pdf_path, meta)
        out_dir = repo_root / "knowledge" / "spec" / meta.out_subdir
        paths = write_clauses(clauses, out_dir)
        index_path = write_index(clauses, out_dir)

        assert paths, f"no clauses extracted from {pdf_path.name}"
        assert all(path.read_text(encoding="utf-8").startswith("---\n") for path in paths)
        assert index_path.read_text(encoding="utf-8").startswith("---\n")
