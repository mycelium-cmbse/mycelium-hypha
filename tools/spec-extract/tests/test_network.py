# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Tests for the networked layer, against a stubbed ``urlopen`` – no real requests are made.

These cover the parts that talk to GitHub: tag pagination, the tree listing and its truncation guard,
token handling, and the per-tag fetch orchestration.
"""

from __future__ import annotations

import json
import urllib.request
from pathlib import Path

import pytest

from spec_extract import fetch, manifest, versions


class _FakeResponse:
    """Minimal stand-in for the object ``urlopen`` returns as a context manager."""

    def __init__(self, payload: bytes) -> None:
        self._payload = payload

    def read(self) -> bytes:
        return self._payload

    def __enter__(self) -> _FakeResponse:
        return self

    def __exit__(self, *_: object) -> bool:
        return False


@pytest.fixture
def captured(monkeypatch: pytest.MonkeyPatch) -> list[urllib.request.Request]:
    """Records every request and answers it from ``routes`` keyed by URL substring."""
    seen: list[urllib.request.Request] = []
    monkeypatch.delenv("GITHUB_TOKEN", raising=False)
    monkeypatch.delenv("GH_TOKEN", raising=False)

    def fake_urlopen(request, timeout=None):  # noqa: ANN001, ANN202 - test double
        seen.append(request)
        return _FakeResponse(routes(request.full_url))

    monkeypatch.setattr(urllib.request, "urlopen", fake_urlopen)
    return seen


def routes(url: str) -> bytes:
    """Canned payloads for the URLs the modules build."""
    if "/tags?" in url:
        page = int(url.rsplit("page=", 1)[1])
        if page == 1:
            return json.dumps([{"name": f"2026-{index:02d}"} for index in range(1, 13)]).encode()
        return b"[]"
    if "/git/trees/" in url:
        return json.dumps(
            {
                "truncated": False,
                "tree": [
                    {"type": "blob", "path": "bnf/SysML-textual-bnf.kebnf"},
                    {"type": "blob", "path": "kerml/Kernel/Connections.kerml"},
                    {"type": "tree", "path": "bnf"},
                ],
            }
        ).encode()
    if "/commits/" in url:
        return json.dumps({"sha": "de1070ae8e79c21532b8004fc663d47b35d0e9fa"}).encode()
    return b"file-content"


def test_default_token_prefers_github_token(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setenv("GITHUB_TOKEN", "primary")
    monkeypatch.setenv("GH_TOKEN", "secondary")

    assert versions.default_token() == "primary"


def test_default_token_falls_back_to_gh_token(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.delenv("GITHUB_TOKEN", raising=False)
    monkeypatch.setenv("GH_TOKEN", "secondary")

    assert versions.default_token() == "secondary"


def test_default_token_is_none_when_unset(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.delenv("GITHUB_TOKEN", raising=False)
    monkeypatch.delenv("GH_TOKEN", raising=False)

    assert versions.default_token() is None


def test_api_headers_are_anonymous_without_a_token(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.delenv("GITHUB_TOKEN", raising=False)
    monkeypatch.delenv("GH_TOKEN", raising=False)

    assert "Authorization" not in versions.api_headers(None)


def test_api_headers_authenticate_from_the_environment(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setenv("GITHUB_TOKEN", "from-env")

    assert versions.api_headers(None)["Authorization"] == "Bearer from-env"


def test_api_headers_prefer_an_explicit_token(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setenv("GITHUB_TOKEN", "from-env")

    assert versions.api_headers("explicit")["Authorization"] == "Bearer explicit"


def test_fetch_tags_follows_pagination_until_a_page_is_empty(captured: list) -> None:
    tags = versions.fetch_tags(versions.RELEASE_REPO)

    assert len(tags) == 12
    assert tags[0] == "2026-01"
    assert len(captured) == 2, "should stop at the first empty page"


def test_list_tree_returns_blob_paths_only(captured: list) -> None:
    paths = fetch.list_tree(fetch.RELEASE_REPO, "2026-05")

    assert paths == ["bnf/SysML-textual-bnf.kebnf", "kerml/Kernel/Connections.kerml"]


def test_list_tree_refuses_a_truncated_listing(monkeypatch: pytest.MonkeyPatch) -> None:
    # A truncated tree would silently drop inputs, so it must fail loudly.
    monkeypatch.setattr(
        urllib.request,
        "urlopen",
        lambda request, timeout=None: _FakeResponse(json.dumps({"truncated": True, "tree": []}).encode()),
    )

    with pytest.raises(RuntimeError, match="truncated"):
        fetch.list_tree(fetch.RELEASE_REPO, "2026-05")


def test_read_url_returns_the_body(captured: list) -> None:
    assert fetch.read_url("https://example.invalid/x") == b"file-content"


def test_fetch_metamodel_writes_both_models(captured: list, tmp_path: Path) -> None:
    written = fetch.fetch_metamodel("2026-05", tmp_path)

    assert sorted(path.name for path in written) == ["KerML_only_xmi.uml", "SysML_only_xmi.uml"]
    assert all(path.parent == tmp_path / "2026-05" / "xmi" for path in written)
    assert all(path.read_bytes() == b"file-content" for path in written)


def test_fetch_metamodel_does_not_write_primitive_types(captured: list, tmp_path: Path) -> None:
    written = fetch.fetch_metamodel("2026-05", tmp_path)

    assert fetch.SHARED_XMI not in [path.name for path in written]


def test_fetch_textual_preserves_the_upstream_layout(captured: list, tmp_path: Path) -> None:
    written = fetch.fetch_textual("2026-05", tmp_path)
    relative = sorted(str(path.relative_to(tmp_path / "2026-05" / "textual")).replace("\\", "/") for path in written)

    assert relative == ["bnf/SysML-textual-bnf.kebnf", "kerml/Kernel/Connections.kerml"]


def test_fetch_specs_writes_into_the_git_ignored_folder(captured: list, tmp_path: Path) -> None:
    written = fetch.fetch_specs("2026-05", tmp_path)

    assert len(written) == len(fetch.SPEC_PDFS)
    assert all(path.parent == tmp_path / "2026-05" / "specs" for path in written)


def test_resolve_commit_returns_the_sha(captured: list) -> None:
    sha = manifest.resolve_commit(manifest.RELEASE_REPO, "2026-05")

    assert sha == "de1070ae8e79c21532b8004fc663d47b35d0e9fa"
