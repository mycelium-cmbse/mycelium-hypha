# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""The ``python -m spec_extract`` entry point: argument parsing and the missing-PDF exit path.

Real extraction is already covered by ``test_generate.py`` against the real PDFs when present; this
only exercises the CLI wrapper itself, so it needs no PDFs at all.
"""

from __future__ import annotations

from pathlib import Path

import pytest

import spec_extract.__main__ as cli
from spec_extract.__main__ import main


def test_requires_repo_root_and_tag() -> None:
    with pytest.raises(SystemExit):
        main([])


def test_missing_pdfs_exits_non_zero_and_names_them(tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    exit_code = main(["--repo-root", str(tmp_path), "--tag", "2099-01"])

    assert exit_code == 1
    error = capsys.readouterr().err
    assert "1-Kernel_Modeling_Language.pdf" in error
    assert "2a-OMG_Systems_Modeling_Language.pdf" in error
    assert str(tmp_path / "sources" / "2099-01" / "specs") in error


def _seed_pdfs(repo_root: Path, tag: str) -> None:
    specs_dir = repo_root / "sources" / tag / "specs"
    specs_dir.mkdir(parents=True)
    (specs_dir / cli.KERML_PDF).touch()
    (specs_dir / cli.SYSML_PDF).touch()


def _stub_pipeline(monkeypatch: pytest.MonkeyPatch) -> list[Path]:
    """Replaces the pipeline calls with recorders; returns the ``out_dir``s ``write_clauses`` saw."""
    recorded: list[Path] = []
    monkeypatch.setattr(cli, "extract_document", lambda pdf, meta: ["fake-clause"])
    monkeypatch.setattr(cli, "write_clauses", lambda clauses, out_dir: recorded.append(out_dir))
    monkeypatch.setattr(cli, "write_index", lambda clauses, out_dir: None)
    monkeypatch.setattr(cli, "write_index_json", lambda clauses, out_dir: None)
    return recorded


def test_out_dir_defaults_to_repo_root_when_out_root_is_omitted(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    _seed_pdfs(tmp_path, "2026-05")
    recorded = _stub_pipeline(monkeypatch)

    exit_code = main(["--repo-root", str(tmp_path), "--tag", "2026-05"])

    assert exit_code == 0
    assert recorded == [
        tmp_path / "knowledge" / "2026-05" / "spec" / "kerml",
        tmp_path / "knowledge" / "2026-05" / "spec" / "sysml2",
    ]


def test_out_root_overrides_where_output_is_written(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    repo_root = tmp_path / "repo"
    out_root = tmp_path / "out"
    _seed_pdfs(repo_root, "2026-05")
    recorded = _stub_pipeline(monkeypatch)

    exit_code = main(
        ["--repo-root", str(repo_root), "--tag", "2026-05", "--out-root", str(out_root)]
    )

    assert exit_code == 0
    assert recorded == [
        out_root / "knowledge" / "2026-05" / "spec" / "kerml",
        out_root / "knowledge" / "2026-05" / "spec" / "sysml2",
    ]
