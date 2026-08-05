# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Regenerate the specification clause knowledge base for every installed release.

Running pytest with the OMG PDFs present under ``sources/<tag>/specs/`` populates
``knowledge/<tag>/spec/{kerml,sysml2}/``; without them it skips.

The clause text is git-ignored (OMG terms) and is the only thing this pipeline produces. The
cross-references that used to be written here moved to ``Hypha.Knowledge.CrossReferences``; see
``CrossReferenceGenerationTests``, which no longer needs the PDFs at all.
"""

from __future__ import annotations

import json
from pathlib import Path

from conftest import require_pdfs
from spec_extract.pipeline import DocMeta, extract_document, write_clauses, write_index, write_index_json


def test_regenerates_spec_knowledge_base(repo_root: Path, installed_tags: list[str]) -> None:
    for tag in installed_tags:
        kerml_pdf, sysml_pdf = require_pdfs(repo_root, tag)
        targets = [
            (kerml_pdf, DocMeta("KerML", "1.0", "kerml")),
            (sysml_pdf, DocMeta("SysML", "2.0", "sysml2")),
        ]

        for pdf_path, meta in targets:
            clauses = extract_document(pdf_path, meta)
            out_dir = repo_root / "knowledge" / tag / "spec" / meta.out_subdir
            paths = write_clauses(clauses, out_dir)
            index_path = write_index(clauses, out_dir)
            index_json_path = write_index_json(clauses, out_dir)

            assert paths, f"no clauses extracted from {pdf_path.name} at {tag}"
            assert all(path.read_text(encoding="utf-8").startswith("---\n") for path in paths)
            assert index_path.read_text(encoding="utf-8").startswith("---\n")

            catalog = json.loads(index_json_path.read_text(encoding="utf-8"))
            assert catalog["clauses"] == len(clauses)
            assert len(catalog["entries"]) == len(clauses)
