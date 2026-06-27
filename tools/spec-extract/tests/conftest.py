# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Shared test fixtures: locate the repository's source PDFs (git-ignored, present locally)."""

from __future__ import annotations

from pathlib import Path

import pytest


def _repo_root() -> Path:
    """Walk up from this file until the directory that contains ``sources/specs``."""
    for parent in Path(__file__).resolve().parents:
        if (parent / "sources" / "specs").is_dir():
            return parent
    raise RuntimeError("Could not locate the repository root (no sources/specs directory found).")


@pytest.fixture(scope="session")
def specs_dir() -> Path:
    """Path to ``sources/specs`` (the git-ignored OMG PDFs live here)."""
    return _repo_root() / "sources" / "specs"


@pytest.fixture(scope="session")
def repo_root() -> Path:
    """Repository root (the directory containing ``sources/specs``)."""
    return _repo_root()


@pytest.fixture(scope="session")
def kerml_pdf(specs_dir: Path) -> Path:
    """Path to the KerML 1.0 specification PDF; skips the test when it is not present."""
    return _require_pdf(specs_dir / "1-Kernel_Modeling_Language.pdf")


@pytest.fixture(scope="session")
def sysml_pdf(specs_dir: Path) -> Path:
    """Path to the SysML v2 specification PDF; skips the test when it is not present."""
    return _require_pdf(specs_dir / "2a-OMG_Systems_Modeling_Language.pdf")


def _require_pdf(path: Path) -> Path:
    if not path.is_file():
        pytest.skip(f"OMG spec PDF not present (git-ignored): {path}")
    return path
