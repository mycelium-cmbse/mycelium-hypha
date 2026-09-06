# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""The ``python -m spec_extract`` entry point: argument parsing and the missing-PDF exit path.

Real extraction is already covered by ``test_generate.py`` against the real PDFs when present; this
only exercises the CLI wrapper itself, so it needs no PDFs at all.
"""

from __future__ import annotations

from pathlib import Path

import pytest

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
