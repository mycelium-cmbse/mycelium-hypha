# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Shared test fixtures: locate the repository and the installed releases.

Inputs are per release tag now: the (git-ignored) OMG PDFs live under ``sources/<tag>/specs/`` and
the generated knowledge under ``knowledge/<tag>/``. Which tags are installed comes from the
committed manifest, so tests follow the rolling window rather than a hard-coded version.
"""

from __future__ import annotations

import json
from pathlib import Path

import pytest

KERML_PDF = "1-Kernel_Modeling_Language.pdf"
SYSML_PDF = "2a-OMG_Systems_Modeling_Language.pdf"


def _repo_root() -> Path:
    """Walk up from this file until the directory that holds both ``sources`` and ``knowledge``."""
    for parent in Path(__file__).resolve().parents:
        if (parent / "sources").is_dir() and (parent / "knowledge").is_dir():
            return parent
    raise RuntimeError("Could not locate the repository root (no sources/ + knowledge/ found).")


@pytest.fixture(scope="session")
def repo_root() -> Path:
    """Repository root."""
    return _repo_root()


@pytest.fixture(scope="session")
def installed_tags(repo_root: Path) -> list[str]:
    """The release tags this checkout carries, newest first; skips when there is no manifest.

    The manifest is written by ``Hypha.Knowledge`` on the .NET side; the PDF chain only needs to know
    which releases exist, so it reads the file directly rather than owning a model for it.
    """
    manifest_path = repo_root / "knowledge" / "versions.json"
    if not manifest_path.is_file():
        pytest.skip(f"no version manifest at {manifest_path}")

    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))

    return [version["tag"] for version in manifest["versions"]]


def specs_dir(repo_root: Path, tag: str) -> Path:
    """Where one release's (git-ignored) OMG PDFs live."""
    return repo_root / "sources" / tag / "specs"


def require_pdfs(repo_root: Path, tag: str) -> tuple[Path, Path]:
    """The KerML and SysML PDFs for ``tag``; skips the test when either is absent."""
    directory = specs_dir(repo_root, tag)
    kerml, sysml = directory / KERML_PDF, directory / SYSML_PDF

    missing = [path.name for path in (kerml, sysml) if not path.is_file()]
    if missing:
        pytest.skip(f"OMG spec PDF(s) not present for {tag} (git-ignored): {', '.join(missing)}")

    return kerml, sysml
