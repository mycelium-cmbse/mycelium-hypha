# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Regenerate the (git-ignored) spec knowledge base from the local PDFs.

This mirrors how the C# metamodel-gen tests write the knowledge base: running pytest with the OMG
PDFs present in ``sources/specs/`` populates ``knowledge/spec/{kerml,sysml2}/``; without them it skips.
The output is git-ignored, so it is never committed.
"""

from __future__ import annotations

import json
from pathlib import Path

import pytest

from spec_extract.crossrefs import (
    build_cross_references,
    load_clause_titles,
    load_elements,
    load_examples,
    load_productions,
    render,
    write_cross_references,
)
from spec_extract.pipeline import DocMeta, extract_document, write_clauses, write_index, write_index_json


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
        index_json_path = write_index_json(clauses, out_dir)

        assert paths, f"no clauses extracted from {pdf_path.name}"
        assert all(path.read_text(encoding="utf-8").startswith("---\n") for path in paths)
        assert index_path.read_text(encoding="utf-8").startswith("---\n")

        catalog = json.loads(index_json_path.read_text(encoding="utf-8"))
        assert catalog["clauses"] == len(clauses)
        assert len(catalog["entries"]) == len(clauses)


def test_regenerates_cross_references(repo_root: Path) -> None:
    """Write the committed ``knowledge/cross-references.json``.

    Unlike the spec tree this output *is* committed – it holds clause identifiers, never clause text. It
    still needs the spec clause catalogs to exist, so it skips when they have not been generated locally.
    """
    knowledge = repo_root / "knowledge"
    spec_indexes = {doc: knowledge / "spec" / doc / "index.json" for doc in ("kerml", "sysml2")}
    missing = [str(path) for path in spec_indexes.values() if not path.is_file()]
    if missing:
        pytest.skip("spec clause catalog not generated (git-ignored): " + ", ".join(missing))

    elements, model_version_uri = load_elements(knowledge / "metamodel" / "index.json")

    clause_titles: dict[str, dict[str, str]] = {}
    documents: dict[str, dict[str, str]] = {}
    for doc, path in spec_indexes.items():
        clause_titles[doc], documents[doc] = load_clause_titles(path)

    bnf_dir = repo_root / "sources" / "textual" / "bnf"
    productions = {
        "kerml": load_productions(bnf_dir / "KerML-textual-bnf.kebnf"),
        "sysml": load_productions(bnf_dir / "SysML-textual-bnf.kebnf"),
    }

    examples = load_examples(knowledge / "textual-notation" / "examples", "textual-notation/examples/")

    document = build_cross_references(elements, clause_titles, documents, productions, examples, model_version_uri)
    path = write_cross_references(document, knowledge / "cross-references.json")

    written = path.read_text(encoding="utf-8")
    assert written == render(document), "written file must match the rendered document byte for byte"
    assert document["counts"]["elements"] == len(elements)
    assert document["counts"]["withClauses"] > 0, "no element matched a clause title - check the spec catalog"
    assert document["counts"]["withExamples"] > 0, "no element matched a worked example"
    assert '"title"' not in written, "cross-references must never carry OMG clause titles"
