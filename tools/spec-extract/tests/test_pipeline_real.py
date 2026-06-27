# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""End-to-end smoke test against the real KerML PDF (skips when the git-ignored PDF is absent).

Asserts structural properties only — never OMG text — so nothing copyrighted enters the repo.
"""

from __future__ import annotations

from pathlib import Path

import pytest

from spec_extract.model import Clause
from spec_extract.pipeline import DocMeta, extract_document, write_clauses


@pytest.fixture(scope="module")
def kerml_clauses(kerml_pdf: Path) -> list[Clause]:
    return extract_document(kerml_pdf, DocMeta("KerML", "1.0", "kerml"))


def test_extracts_clauses_with_locations(kerml_clauses: list[Clause]) -> None:
    assert len(kerml_clauses) >= 1
    for clause in kerml_clauses:
        assert clause.title
        assert clause.page_start >= 1
        assert clause.page_end >= clause.page_start
    # Parent/section headings may have empty bodies (their prose lives in subclauses), but the
    # document as a whole must carry real extracted text.
    assert any(clause.text.strip() for clause in kerml_clauses)


def test_write_clauses_emits_markdown(kerml_clauses: list[Clause], tmp_path: Path) -> None:
    paths = write_clauses(kerml_clauses[:5], tmp_path)

    assert paths
    for path in paths:
        assert path.suffix == ".md"
        assert path.read_text(encoding="utf-8").startswith("---\n")
