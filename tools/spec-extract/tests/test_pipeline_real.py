# SPDX-FileCopyrightText: 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""End-to-end smoke test against the real KerML PDF (skips when the git-ignored PDF is absent).

Asserts structural properties only (never OMG text), so nothing copyrighted enters the repo.
"""

from __future__ import annotations

from pathlib import Path

import pytest

from conftest import require_pdfs
from spec_extract.model import Clause
from spec_extract.pipeline import DocMeta, extract_document, write_clauses


@pytest.fixture(scope="module")
def kerml_clauses(repo_root: Path, installed_tags: list[str]) -> list[Clause]:
    """Clauses from the default (newest installed) release's KerML PDF."""
    kerml_pdf, _ = require_pdfs(repo_root, installed_tags[0])
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


def test_extracts_the_annexes_after_the_numbered_clauses(kerml_clauses: list[Clause]) -> None:
    numbers = [clause.number for clause in kerml_clauses]
    annexes = [number for number in numbers if number[0].isalpha()]

    assert "A" in annexes
    assert numbers[-len(annexes) :] == annexes


def test_reads_the_version_from_the_cover(kerml_clauses: list[Clause]) -> None:
    # One version for the whole document, read from its cover rather than the one passed in.
    assert len({clause.version for clause in kerml_clauses}) == 1
    assert kerml_clauses[0].version


def test_the_informative_annex_is_informative_throughout(kerml_clauses: list[Clause]) -> None:
    # Annex A, Model Execution, is informative, though some of its text says "shall".
    annex = [clause for clause in kerml_clauses if clause.number.split(".")[0] == "A"]

    assert annex
    assert not any(clause.is_normative for clause in annex)
