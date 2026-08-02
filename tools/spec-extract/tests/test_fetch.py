# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Unit tests for input selection, on a synthetic Release-repo listing (no network)."""

from __future__ import annotations

import urllib.error
from pathlib import Path

import pytest

from spec_extract.fetch import SHARED_XMI, XMI_FILES, download, select_textual_paths

# A cut-down listing in the shape the Release repo really publishes at a tag.
TREE = [
    "bnf/KerML-textual-bnf.kebnf",
    "bnf/SysML-textual-bnf.kebnf",
    "bnf/SysML-graphical-bnf.kgbnf",
    "bnf/KerML-textual-bnf.html",
    "bnf/bnf_styles.css",
    "bnf/images/part-def.svg",
    "kerml/Kernel/Connections.kerml",
    "sysml/training/02. Part Definitions/Part Definition Example.sysml",
    "sysml.library/Systems Library/Parts.sysml",
    "doc/1-Kernel_Modeling_Language.pdf",
    "README.adoc",
]


def test_selects_the_grammar_but_not_its_rendering() -> None:
    selected = select_textual_paths(TREE)

    assert "bnf/KerML-textual-bnf.kebnf" in selected
    assert "bnf/SysML-graphical-bnf.kgbnf" in selected
    assert not [path for path in selected if path.endswith((".html", ".css", ".svg"))]


def test_selects_the_kerml_and_sysml_models() -> None:
    selected = select_textual_paths(TREE)

    assert "kerml/Kernel/Connections.kerml" in selected
    assert "sysml/training/02. Part Definitions/Part Definition Example.sysml" in selected


def test_excludes_pdfs_and_repository_chrome() -> None:
    selected = select_textual_paths(TREE)

    assert not [path for path in selected if path.startswith("doc/")]
    assert "README.adoc" not in selected


def test_excludes_the_model_libraries() -> None:
    # sysml.library/ is a separate concern from the grammar and example models; ingesting it is
    # deliberately out of scope here, so it must not be swept in by a loose prefix match.
    selected = select_textual_paths(TREE)

    assert not [path for path in selected if path.startswith("sysml.library")]


def test_selection_is_deterministic_and_grammar_first() -> None:
    first = select_textual_paths(TREE)
    second = select_textual_paths(list(reversed(TREE)))

    assert first == second
    assert first[0].startswith("bnf/")


def test_primitive_types_is_not_fetched_per_tag() -> None:
    # It is the OMG UML primitives library, published by neither upstream and identical per release.
    assert SHARED_XMI == "PrimitiveTypes.xmi"
    assert not [name for name in XMI_FILES.values() if name == SHARED_XMI]


def test_retries_a_reset_connection_and_succeeds(tmp_path: Path) -> None:
    # A release is a few hundred files; fetching them back to back gets the connection reset, so a
    # transient failure must not abort the run.
    attempts: list[str] = []
    delays: list[float] = []

    def reader(url: str) -> bytes:
        attempts.append(url)
        if len(attempts) < 3:
            raise ConnectionResetError("forcibly closed by the remote host")
        return b"content"

    destination = download(
        "owner/repo", "2026-05", "bnf/x.kebnf", tmp_path / "x.kebnf",
        reader=reader, sleep=delays.append,
    )

    assert destination.read_bytes() == b"content"
    assert len(attempts) == 3
    assert delays == [1.0, 2.0], "backoff should grow exponentially"


def test_gives_up_after_the_retry_budget(tmp_path: Path) -> None:
    def reader(url: str) -> bytes:
        raise urllib.error.URLError("down")

    with pytest.raises(urllib.error.URLError):
        download(
            "owner/repo", "2026-05", "bnf/x.kebnf", tmp_path / "x.kebnf",
            retries=2, reader=reader, sleep=lambda _: None,
        )


def test_skip_existing_makes_a_rerun_resume(tmp_path: Path) -> None:
    existing = tmp_path / "x.kebnf"
    existing.write_bytes(b"already here")

    def reader(url: str) -> bytes:
        raise AssertionError("should not re-download an existing file")

    result = download(
        "owner/repo", "2026-05", "bnf/x.kebnf", existing, skip_existing=True, reader=reader
    )

    assert result.read_bytes() == b"already here"


def test_skip_existing_still_fetches_an_empty_file(tmp_path: Path) -> None:
    # A zero-byte file is the signature of an interrupted write, not a completed download.
    empty = tmp_path / "x.kebnf"
    empty.write_bytes(b"")

    result = download(
        "owner/repo", "2026-05", "bnf/x.kebnf", empty, skip_existing=True, reader=lambda url: b"real"
    )

    assert result.read_bytes() == b"real"
