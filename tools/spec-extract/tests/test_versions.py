# Copyright 2026 Starion Group S.A.
# SPDX-License-Identifier: Apache-2.0
"""Unit tests for release-tag selection, on the real tag shapes both upstreams publish (no network)."""

from __future__ import annotations

import pytest

from spec_extract.versions import available_versions, is_release_tag, latest, sort_key

# Shapes actually observed upstream, so the rules are tested against reality rather than a guess.
REAL_RELEASE_TAGS = ["2026-05", "2026-04", "2026-03", "2025-09.1", "2025-09", "2025-07", "2020-10"]

# Release-shaped, but published only by the Pilot repo - excluded for lack of a counterpart, not shape.
PILOT_ONLY_BUT_VALID = ["2024-08", "2023-01"]

# Not release-shaped at all: pre-releases, internal drops and letter revisions.
NON_RELEASE_TAGS = ["2026-05-pre", "2021-05a", "2021-08-internal", "2021-02b"]


@pytest.mark.parametrize("tag", ["2026-05", "2020-10", "2025-09.1", "2023-07.1"])
def test_accepts_release_shaped_tags(tag: str) -> None:
    assert is_release_tag(tag)


@pytest.mark.parametrize("tag", ["2026-05-pre", "2021-05a", "2021-08-internal", "2021-02b", "main", ""])
def test_rejects_pre_releases_and_oddities(tag: str) -> None:
    assert not is_release_tag(tag)


def test_sorts_chronologically_not_lexically() -> None:
    assert sorted(["2025-09", "2026-01", "2025-12"], key=sort_key) == ["2025-09", "2025-12", "2026-01"]


def test_point_release_sorts_after_its_base_tag() -> None:
    assert sorted(["2025-09.1", "2025-09"], key=sort_key) == ["2025-09", "2025-09.1"]


def test_sort_key_rejects_a_non_release_tag() -> None:
    with pytest.raises(ValueError, match="not a release tag"):
        sort_key("2026-05-pre")


def test_available_versions_is_the_intersection_newest_first() -> None:
    versions = available_versions(REAL_RELEASE_TAGS, REAL_RELEASE_TAGS)

    assert versions[0] == "2026-05"
    assert versions[-1] == "2020-10"
    assert versions == sorted(versions, key=sort_key, reverse=True)


def test_a_tag_missing_from_the_pilot_repo_is_not_offered() -> None:
    # 2023-07.1 exists only in the Release repo; without the metamodel there is no version to build.
    versions = available_versions(["2026-05", "2023-07.1"], ["2026-05"])

    assert versions == ["2026-05"]


def test_a_release_shaped_tag_missing_from_the_release_repo_is_not_offered() -> None:
    # 2024-08 and 2023-01 exist only in the Pilot repo: valid shapes, but there is no spec side.
    versions = available_versions(["2026-05"], ["2026-05"] + PILOT_ONLY_BUT_VALID)

    assert versions == ["2026-05"]


def test_pre_releases_are_excluded_even_when_present_in_both() -> None:
    versions = available_versions(["2026-05"] + NON_RELEASE_TAGS, ["2026-05"] + NON_RELEASE_TAGS)

    assert versions == ["2026-05"]


def test_latest_takes_the_rolling_window() -> None:
    versions = available_versions(REAL_RELEASE_TAGS, REAL_RELEASE_TAGS)

    assert latest(versions, 2) == ["2026-05", "2026-04"]
