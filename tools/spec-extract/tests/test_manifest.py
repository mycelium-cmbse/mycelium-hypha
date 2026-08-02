# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Unit tests for the installed-versions manifest (no network)."""

from __future__ import annotations

from pathlib import Path

import pytest

from spec_extract.manifest import SCHEMA_VERSION, build_manifest, read, render, tags, write

RESOLVED = {
    "2026-04": {"release": "9baca590", "pilot": "20897e31"},
    "2026-05": {"release": "de1070ae", "pilot": "fa709f28"},
}


def test_lists_versions_newest_first_regardless_of_input_order() -> None:
    manifest = build_manifest("2026-05", RESOLVED)

    assert tags(manifest) == ["2026-05", "2026-04"]


def test_records_both_upstream_commits_per_tag() -> None:
    manifest = build_manifest("2026-05", RESOLVED)
    newest = manifest["versions"][0]

    assert newest["release"]["commit"] == "de1070ae"
    assert newest["pilot"]["commit"] == "fa709f28"
    assert "Release" in newest["release"]["repo"]
    assert "Pilot" in newest["pilot"]["repo"]


def test_carries_the_default_and_schema_version() -> None:
    manifest = build_manifest("2026-04", RESOLVED)

    assert manifest["default"] == "2026-04"
    assert manifest["schemaVersion"] == SCHEMA_VERSION


def test_rejects_a_default_that_is_not_installed() -> None:
    with pytest.raises(ValueError, match="not among the resolved versions"):
        build_manifest("2026-03", RESOLVED)


def test_carries_no_model_uri() -> None:
    # The tag is the identifier; the URI inside the XMI tracks neither release nor content.
    text = render(build_manifest("2026-05", RESOLVED))

    assert "20250201" not in text
    assert "modelVersionUri" not in text


def test_render_is_deterministic_and_newline_terminated() -> None:
    first = render(build_manifest("2026-05", RESOLVED))
    second = render(build_manifest("2026-05", dict(reversed(list(RESOLVED.items())))))

    assert first == second
    assert first.endswith("}\n")


def test_round_trips_through_disk(tmp_path: Path) -> None:
    path = write(build_manifest("2026-05", RESOLVED), tmp_path / "versions.json")

    assert tags(read(path)) == ["2026-05", "2026-04"]
